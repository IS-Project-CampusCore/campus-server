using campus.Models;
using commons.Database;
using commons.Protos;
using commons.RequestBase;
using commons.Tools;
using emailServiceClient;
using excelServiceClient;
using MongoDB.Driver;
using System;
using System.Text.Json;
using usersServiceClient;
namespace campus.Implementation;


public interface ICampusService
{
    Task<Scheddule?> GetAccommodationScheduleById(string id);
    Task<List<Accommodation>> GetAccommodations();
    Task<Accommodation?> GetAccommodationById(string id);
    Task<Accommodation> CreateAccommodationAsync(string name, string description, int openTime, int closeTime);
    Task GenerateDistributionAsync(string placeholder);
    Task<Payment> CreatePaymentAsync(string userId, double amount, string cardNumber, string expDate, string cvv);
    Task<Issue> ReportIssueAsync(string issuerId, string location, string description);
}
public class CampusServiceImplementation(
    ILogger<CampusServiceImplementation> logger,
    IDatabase database,
    usersService.usersServiceClient usersService,
    emailService.emailServiceClient emailService,
    excelService.excelServiceClient excelService



) : ICampusService
{
    private readonly ILogger<CampusServiceImplementation> _logger = logger;
    private readonly usersService.usersServiceClient _usersService = usersService;
    private readonly emailService.emailServiceClient _emailService = emailService;
    private readonly excelService.excelServiceClient _excelService = excelService;

    private readonly AsyncLazy<IDatabaseCollection<Accommodation>> _accommodations= new (() => GetAccommodationsCollection(database));
    private readonly AsyncLazy<IDatabaseCollection<Dormitory>> _dormitories = new(() => GetDormitoriesCollection(database));
    private readonly AsyncLazy<IDatabaseCollection<Room>> _rooms = new(() => GetRoomsCollection(database));
    private readonly AsyncLazy<IDatabaseCollection<Payment>> _payments = new(() => GetPaymentsCollection(database));
    private readonly AsyncLazy<IDatabaseCollection<Issue>> _issues = new(() => GetIssuesCollection(database));


    public async Task<Scheddule?> GetAccommodationScheduleById(string id)
    {
        var col = await _accommodations;
        
        
        var accommodation = await col.GetOneByIdAsync(id);
        if (accommodation is null)
        {
            _logger.LogInformation("GetAccommodationSchedule: accommodation not found for Id:{Id}", id);
            throw new NotFoundException("Accommodation not found");
        }

        return accommodation.Timetable;
        
    }

    public async Task<Accommodation?> GetAccommodationById(string id)
    {
        var db = await _accommodations;
        try
        {
            return await db.GetOneByIdAsync(id);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAccommodations failed");
            throw new InternalErrorException("Failed to retrieve accommodation");
        }
    }

    public async Task<List<Accommodation>> GetAccommodations()
    {
        var col = await _accommodations;
        try
        {
            return await col.MongoCollection.Find(_ => true).ToListAsync();
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAccommodations failed");
            throw new InternalErrorException("Failed to retrieve accommodations");
        }
    }

    public async Task GenerateDistributionAsync(string placeholder)
    {
        var excelData = await ParseAndValidateExcelAsync(placeholder);
        var dormitoriesCol = await _dormitories;
        var roomsCol = await _rooms;
        await SeedDormitoriesAndRoomsAsync(excelData, dormitoriesCol, roomsCol);

        await AssignStudentsToRoomsAsync(roomsCol);
    }

    private async Task<ExcelData> ParseAndValidateExcelAsync(string placeholder)
    {
        var exactTypes = new List<string>
        {
            "String",
            "String",
            "Double",
            "Double",
            "Double"
        };

        var finalReq = new ParseExcelRequest { FileName = placeholder };
        finalReq.CellTypes.AddRange(exactTypes);

        MessageResponse finalRes;
        try
        {
            finalRes = await _excelService.ParseExcelAsync(finalReq);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ParseExcel (final) gRPC call failed for file {File}", placeholder);
            throw new InternalErrorException("Failed to parse excel file");
        }

        if (!finalRes.Success)
        {
            _logger.LogWarning("ParseExcel (final) returned failure: {Errors}", finalRes.Errors);
            throw new InternalErrorException(finalRes.Errors ?? "Excel parse failed");
        }

        var excelData = finalRes.GetPayload<ExcelData>();
        if (excelData is null)
        {
            _logger.LogWarning("ParseExcel (final) returned no payload for file {File}", placeholder);
            throw new BadRequestException("Parsed excel file is empty");
        }

        if (excelData.Errors is { Count: > 0 })
        {
            var stringErrors = string.Join("; ", excelData.Errors);
            foreach (var e in excelData.Errors)
                _logger.LogWarning("Excel parse warning: {Err}", e);
            throw new BadRequestException("Excel parse errors: " + stringErrors);
        }

        return excelData;
    }

    private static string? CellToString(object? v)
    {
        if (v is null) return null;
        if (v is JsonElement je)
        {
            if (je.ValueKind == JsonValueKind.String) return je.GetString();
            if (je.ValueKind == JsonValueKind.Number) return je.GetRawText();
            if (je.ValueKind == JsonValueKind.True) return "true";
            if (je.ValueKind == JsonValueKind.False) return "false";
            return je.ToString();
        }
        return v.ToString();
    }

    private static bool TryCellToInt(object? v, out int result)
    {
        result = 0;
        if (v is null) return false;
        if (v is JsonElement je)
        {
            if (je.ValueKind == JsonValueKind.Number && je.TryGetInt32(out result)) return true;
            if (je.ValueKind == JsonValueKind.String && int.TryParse(je.GetString(), out result)) return true;
            return false;
        }
        if (v is int i) { result = i; return true; }
        if (v is long l) { result = (int)l; return true; }
        if (v is double d) { result = Convert.ToInt32(d); return true; }
        if (v is string s && int.TryParse(s, out result)) return true;
        return false;
    }

    private async Task SeedDormitoriesAndRoomsAsync(ExcelData excelData, IDatabaseCollection<Dormitory> dormitoriesCol, IDatabaseCollection<Room> roomsCol)
    {
        var dormCache = new Dictionary<string, Dormitory>(StringComparer.OrdinalIgnoreCase);
        int rowIndex = 0;

        foreach (var row in excelData.Rows)
        {
            rowIndex++;
            if (row is null) continue;

            if (row.Count < 5)
            {
                _logger.LogInformation("Skipping row {Row}: not enough columns", rowIndex + 1);
                continue;
            }

            var dormNameCell = row[0];
            var dormAddrCell = row[1];
            var amountCell = row[2];
            var roomNumberCell = row[3];
            var capacityCell = row[4];

            string? dormName = CellToString(dormNameCell?.Value);
            string? dormAddr = CellToString(dormAddrCell?.Value);

            if (!TryCellToInt(roomNumberCell?.Value, out var roomNumberInt))
            {
                _logger.LogInformation("Skipping row {Row}: invalid room number (must be numeric)", rowIndex + 1);
                throw new BadRequestException("Invalid room number");
            }

            string roomNumber = roomNumberInt.ToString();

            if (!TryCellToInt(capacityCell?.Value, out var capacity))
            {
                _logger.LogInformation("Skipping row {Row}: invalid capacity (must be numeric)", rowIndex + 1);
                throw new BadRequestException("Invalid capacity");
            }

            if (!TryCellToInt(amountCell?.Value, out var amountInt))
            {
                _logger.LogInformation("Skipping row {Row}: invalid amount", rowIndex + 1);
                throw new BadRequestException("Invalid amount");
            }

            if (string.IsNullOrWhiteSpace(dormName))
            {
                _logger.LogInformation("Skipping row {Row}: dormitory name missing", rowIndex + 1);
                throw new BadRequestException("Dormitory name missing");
            }

            string dormKey = $"{dormName.Trim().ToLowerInvariant()}|{(dormAddr ?? string.Empty).Trim().ToLowerInvariant()}";

            Dormitory? dorm;
            if (!dormCache.TryGetValue(dormKey, out dorm))
            {
                dorm = await dormitoriesCol.GetOneAsync(d => d.Name == dormName && d.Address == (dormAddr ?? string.Empty));

                if (dorm is null)
                {
                    dorm = new Dormitory
                    {
                        Name = dormName,
                        Address = dormAddr ?? string.Empty,
                        Amount = amountInt
                    };
                    await dormitoriesCol.InsertAsync(dorm);
                    _logger.LogInformation("Created Dormitory: {Dorm} ({Addr})", dorm.Name, dorm.Address);
                }

                dormCache[dormKey] = dorm;
            }

            var existingRoom = await roomsCol.GetOneAsync(r => r.DormitoryId == dorm.Id && r.RoomNumber == roomNumber);
            if (existingRoom is not null)
            {
                _logger.LogInformation("Row {Row}: room {Room} already exists in dorm {Dorm}", rowIndex + 1, roomNumber, dorm.Name);
                continue;
            }

            var room = new Room
            {
                DormitoryId = dorm.Id,
                RoomNumber = roomNumber,
                Capacity = capacity,
                MembersId = new()
            };

            await roomsCol.InsertAsync(room);
            _logger.LogInformation("Created Room {Room} in Dormitory {Dorm} (Capacity: {Cap})", roomNumber, dorm.Name, capacity);
        }
    }

    private async Task AssignStudentsToRoomsAsync(IDatabaseCollection<Room> roomsCol)
    {
        var allUsers = await FetchAllUsers();
      
        var allRooms = await roomsCol.MongoCollection
                                     .Find(_ => true)
                                     .ToListAsync();

        var assignedStudentIdsFromRooms = allRooms
            .SelectMany(r => r.MembersId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .ToHashSet();


        /*
                // Prefer using CampusStudent.Dormitory/Room from users service
                List<string> homelessStudentIds;
                var campusStudents = allUsers?.OfType<CampusStudent>().ToList();

                if (campusStudents is null || campusStudents.Count == 0)
                {
                    // Fallback: no users returned from service — fall back to DB-only approach (everyone not in rooms)
                    _logger.LogWarning("No campus students returned from users service; falling back to DB membership check.");
                    // If you have a list of all student IDs somewhere, you would subtract assignedStudentIdsFromRooms from that.
                    // Here we have no full student list, so abort early to avoid assigning incorrectly.
                    throw new InternalErrorException("Cannot generate distribution: no campus students available from users service.");
                }
                else
                {
                    // Students that report no room/dorm are considered homeless
                    homelessStudentIds = campusStudents
                        .Where(cs => string.IsNullOrWhiteSpace(cs.Dormitory) && string.IsNullOrWhiteSpace(cs.Room) && !string.IsNullOrWhiteSpace(cs.Id))
                        .Select(cs => cs.Id!)
                        .ToList();

                    // Optional: detect mismatches where a user reports being assigned but DB doesn't contain them
                    var reportedAssigned = campusStudents
                        .Where(cs => !string.IsNullOrWhiteSpace(cs.Dormitory) || !string.IsNullOrWhiteSpace(cs.Room))
                        .Select(cs => cs.Id!)
                        .Where(id => !assignedStudentIdsFromRooms.Contains(id))
                        .ToList();

                    if (reportedAssigned.Count > 0)
                    {
                        _logger.LogWarning("Users service reports these students as assigned but rooms DB has no membership entries: {UserIds}", string.Join(",", reportedAssigned));
                        // Decide whether to reconcile (e.g. add them to the rooms DB) or to ignore — here we only log.
                    }
                }*/
        if (allRooms.Count == 0)
        {
            _logger.LogError("GenerateDistribution failed: No rooms found in database. Excel seed may have failed.");
            throw new InternalErrorException("No rooms found. Excel seed may have failed.");
        }

        var assignedStudentIds = allRooms
            .SelectMany(r => r.MembersId)
            .ToHashSet();

        var homelessStudentIds = allUsers?
                    .Where(u => string.Equals(u.RoleString, "5", StringComparison.OrdinalIgnoreCase)
                                && !assignedStudentIds.Contains(u.Id))
                    .Select(u => u.Id)
                    .ToList() ?? new List<string?>();

        if (homelessStudentIds.Count == 0)
        {
            _logger.LogInformation("GenerateDistribution: All students are already assigned. No changes made.");
            return;
        }

        var random = new Random();
        var shuffledStudents = homelessStudentIds.OrderBy(x => random.Next()).ToList();

        var roomQueue = new Queue<Room>(allRooms.Where(r => r.MembersId.Count < r.Capacity));

        int assignedCount = 0;

        while (shuffledStudents.Count > 0 && roomQueue.Count > 0)
        {
            var currentRoom = roomQueue.Dequeue();
            bool roomWasModified = false;

            while (currentRoom.MembersId.Count < currentRoom.Capacity && shuffledStudents.Count > 0)
            {
                if (shuffledStudents[0] != null)
                {
                    currentRoom.MembersId.Add(shuffledStudents[0]!);
                    assignedCount++;
                    roomWasModified = true;
                }
                shuffledStudents.RemoveAt(0);
            }

            if (roomWasModified)
            {
                await roomsCol.ReplaceAsync(currentRoom);
            }
        }

        if (shuffledStudents.Count > 0)
        {
            throw new BadRequestException($"The Dormitories are full! students without a room left {shuffledStudents.Count}");
        }

        _logger.LogInformation("Distribution Complete. Assigned {Assigned} new students. Rooms total: {Rooms}", assignedCount, allRooms.Count);
    }

    private async Task<List<UserDto>?> FetchAllUsers()
    {
        var response = await _usersService.GetAllUsersAsync(new GetAllUsersRequest { Placeholder = "" });

        if (!response.Success || string.IsNullOrEmpty(response.Body))
        {
            _logger.LogWarning("Could not fetch users: " + response.Errors);
            return [];
        }

        return JsonSerializer.Deserialize<List<UserDto>>(response.Body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    /*    private async Task<List<User>?> FetchAllUsers()
        {
            var responseByRole = await _usersService.GetUsersByRoleAsync(new UsersRoleRequest { Role = "campus_student" });

            if (!responseByRole.Success || string.IsNullOrEmpty(responseByRole.Body))
            {
                _logger.LogWarning("Could not fetch users by role 'campus_student': {Errors}", responseByRole.Errors);
                return null;
            }

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var users = new List<User>();

            try
            {
                using var doc = JsonDocument.Parse(responseByRole.Body);
                if (doc.RootElement.ValueKind != JsonValueKind.Array)
                {
                    _logger.LogWarning("Users response JSON is not an array");
                    return users;
                }

                foreach (var el in doc.RootElement.EnumerateArray())
                {
                    bool isCampusStudent = false;

                    if (el.TryGetProperty("Role", out var roleProp))
                    {
                        if (roleProp.ValueKind == JsonValueKind.String)
                        {
                            var rv = roleProp.GetString();
                            if (!string.IsNullOrEmpty(rv) &&
                                (rv.Equals("CAMPUS_STUDENT", StringComparison.OrdinalIgnoreCase) ||
                                 rv.Equals("campus_student", StringComparison.OrdinalIgnoreCase)))
                            {
                                isCampusStudent = true;
                            }
                            else if (!isCampusStudent && int.TryParse(rv, out var rn) && rn == (int)UserType.CAMPUS_STUDENT)
                            {
                                isCampusStudent = true;
                            }
                        }
                        else if (roleProp.ValueKind == JsonValueKind.Number && roleProp.TryGetInt32(out var rint))
                        {
                            if (rint == (int)UserType.CAMPUS_STUDENT) isCampusStudent = true;
                        }
                    }

                    try
                    {
                        if (isCampusStudent)
                        {
                            var cs = JsonSerializer.Deserialize<CampusStudent>(el.GetRawText(), options);
                            if (cs != null) users.Add(cs);
                            else
                            {
                                var fallback = JsonSerializer.Deserialize<User>(el.GetRawText(), options);
                                if (fallback != null) users.Add(fallback);
                            }
                        }
                        else
                        {
                            var u = JsonSerializer.Deserialize<User>(el.GetRawText(), options);
                            if (u != null) users.Add(u);
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Failed to deserialize a user element; skipping");
                    }
                }

                return users;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse users response JSON");
                return null;
            }
        }
    */
    public async Task<Accommodation> CreateAccommodationAsync(string name, string description, int openTime, int closeTime)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            _logger.LogInformation("Accommodation name cannot be empty or null");
            throw new BadRequestException("Accommodation name cannot be empty");
        }

        if (openTime < 0 || openTime > 24 || closeTime < 0 || closeTime > 24)
        {
            _logger.LogInformation("Invalid open/close time provided");
            throw new BadRequestException("OpenTime and CloseTime must be between 0 and 24");
        }

        var accommodation = new Accommodation
        {
            Name = name,
            Description = description ?? string.Empty,
            Timetable = new Scheddule
            {
                OpenTime = openTime,
                CloseTime = closeTime
            }
        };

        var col = await _accommodations;
        _logger.LogInformation("Creating accommodation {Name}", name);
        await col.InsertAsync(accommodation);

        return accommodation;
    }

    public async Task<Issue> ReportIssueAsync(string issuerId, string location, string description)
    {
        if (string.IsNullOrWhiteSpace(issuerId))
        {
            _logger.LogInformation("ReportIssue failed: issuerId empty");
            throw new BadRequestException("IssuerId cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(location) && string.IsNullOrWhiteSpace(description))
        {
            _logger.LogInformation("ReportIssue failed: no details provided");
            throw new BadRequestException("Location or description must be provided");
        }

        try
        {
            var getUserRes = await _usersService.GetUserByIdAsync(new UserIdRequest { Id = issuerId });
            if (!getUserRes.Success)
            {
                _logger.LogInformation("ReportIssue: user lookup returned failure. Code:{Code}, Errors:{Errors}", getUserRes.Code, getUserRes.Errors);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInformation("ReportIssue: users service lookup failed ({Type}) {Msg}", ex.GetType().Name, ex.Message);
        }

        var issue = new Issue
        {
            IssuerId = issuerId,
            Location = location ?? string.Empty,
            Description = description ?? string.Empty
        };

        var col = await _issues;
        _logger.LogInformation("Inserting new issue by Issuer:{IssuerId} Location:{Location}", issuerId, location);
        await col.InsertAsync(issue);

        try
        {
            var templateData = JsonSerializer.Serialize(new
            {
                Location = location,
                Description = description,
            });

            var emailReq = new SendEmailRequest
            {
                ToEmail = "birlea94@gmail.com",        
                ToName = "Campus Support",
                TemplateName = "Issue",        
                TemplateData = templateData
            };

            try
            {
                await _emailService.SendEmailAsync(emailReq);
                _logger.LogInformation("Issue notification email sent to issues@example.com");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send issue notification email to issues@example.com");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to prepare issue notification email for issue {IssueId}", issue.Id);
        }

        return issue;
    }

    public bool IsCardNumberFormatValid(string cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber)) return false;

        string cleanNumber = cardNumber.Replace(" ", "").Replace("-", "");

        return System.Text.RegularExpressions.Regex.IsMatch(cleanNumber, @"^\d{16}$");
    }

    public bool IsLuhnValid(string cardNumber)
    {
        int sum = 0;
        bool alternate = false;
        for (int i = cardNumber.Length - 1; i >= 0; i--)
        {
            char c = cardNumber[i];
            if (!char.IsDigit(c)) return false; 

            int n = c - '0'; 

            if (alternate)
            {
                n *= 2;
                if (n > 9) n -= 9; 
            }

            sum += n;
            alternate = !alternate;
        }

        return (sum % 10 == 0);
    }

    public bool IsCvvValid(string cvv)
    {
        if (string.IsNullOrWhiteSpace(cvv)) return false;

        return System.Text.RegularExpressions.Regex.IsMatch(cvv.Trim(), @"^\d{3}$");
    }

    public bool IsNotExpired(string expirationString)
    {
        if (string.IsNullOrWhiteSpace(expirationString)) return false;

        expirationString = expirationString.Trim();

        var parts = expirationString.Split('/');
        if (parts.Length != 2) return false; 

        if (!int.TryParse(parts[0], out int month) || !int.TryParse(parts[1], out int year))
        {
            return false; 
        }

        if (month < 1 || month > 12) return false;

        int fullYear = year < 100 ? 2000 + year : year;

        int lastDay = DateTime.DaysInMonth(fullYear, month);
        var cardExpiry = new DateTime(fullYear, month, lastDay, 23, 59, 59);

        return cardExpiry > DateTime.Now;
    }

    public async Task<Payment> CreatePaymentAsync(string userId, double amount,string cardNumber,string expDate,string cvv)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogInformation("CreatePayment failed: userId is empty");
            throw new BadRequestException("UserId cannot be empty");
        }

        if (amount <= 0)
        {
            _logger.LogInformation("CreatePayment failed: invalid amount {Amount}", amount);
            throw new BadRequestException("Amount must be greater than zero");
        }
        if (!IsCardNumberFormatValid(cardNumber))
        {
            _logger.LogInformation("CreatePayment failed: invalid card number format");
            throw new BadRequestException("Invalid card number format");
        }
        if (!IsLuhnValid(cardNumber))
        {
            _logger.LogInformation("CreatePayment failed: card number failed Luhn check");
            throw new BadRequestException("Invalid card number");
        }
        if (!IsCvvValid(cvv))
        {
            _logger.LogInformation("CreatePayment failed: invalid CVV");
            throw new BadRequestException("Invalid CVV");
        }
        if (!IsNotExpired(expDate))
        {
            _logger.LogInformation("CreatePayment failed: card is expired");
            throw new BadRequestException("Card is expired");
        }

        var getUserRes = await _usersService.GetUserByIdAsync(new UserIdRequest { Id = userId });
        if (!getUserRes.Success)
        {
            _logger.LogInformation("CreatePayment: user not found. Code:{Code}, Errors:{Errors}", getUserRes.Code, getUserRes.Errors);
            throw new NotFoundException("User not found.");
        }

        var payload = getUserRes.Payload;
        string userName = "User";
        string userEmail = "example@example.com";

        try
        {
            userName = payload.GetString("Name") ?? userName;
        }
        catch { /* ignore - fallback to default */ }

        
        
        var maybeEmail = payload.GetString("Email");
        if (!string.IsNullOrWhiteSpace(maybeEmail))
        {
            userEmail = maybeEmail;
        }
        else
        {
            throw new BadRequestException("The user doesn't have an email set!");
        }        
        

        var payment = new Payment
        {
            UserId = userId,
            Amount = Convert.ToDecimal(amount),
            IsActive = true,
            LastPaymentDate = DateTime.UtcNow,
            NextPaymentDate = DateTime.UtcNow.AddMonths(1)
        };

        var paymentsCol = await _payments;
        _logger.LogInformation("Creating payment for User:{UserId} Amount:{Amount}", userId, amount);
        await paymentsCol.InsertAsync(payment);

        try
        {
            var templateData = JsonSerializer.Serialize(new
            {
                Name = userName,
                Amount = payment.Amount.ToString("F2"),
                Date = payment.LastPaymentDate.ToString("yyyy-MM-dd")
            });

            var emailReq = new SendEmailRequest
            {
                ToEmail = userEmail,
                ToName = userName ?? string.Empty,
                TemplateName = "Payment", 
                TemplateData = templateData
            };

            try
            {
                await _emailService.SendEmailAsync(emailReq);
                _logger.LogInformation("Payment notification email sent to {Email}", userEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send payment notification email to {Email}", userEmail);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to prepare payment notification email for user {UserId}", userId);
        }

        return payment;
    }

    internal static async Task<IDatabaseCollection<Accommodation>> GetAccommodationsCollection(IDatabase database)
    {
        var collection = database.GetCollection<Accommodation>();

        var nameIndex = Builders<Accommodation>.IndexKeys.Ascending(a => a.Name);

        CreateIndexModel<Accommodation> index1 = new(nameIndex, new CreateIndexOptions
        {
            Name = "accommodationNameIndex"
        });

        await collection.MongoCollection.Indexes.CreateManyAsync(new[] { index1 });
        return collection;
    }

    internal static async Task<IDatabaseCollection<Dormitory>> GetDormitoriesCollection(IDatabase database)
    {
        var collection = database.GetCollection<Dormitory>();

        var nameIndex = Builders<Dormitory>.IndexKeys.Ascending(d => d.Name);

        CreateIndexModel<Dormitory> index1 = new(nameIndex, new CreateIndexOptions
        {
            Name = "dormitoryNameIndex"
        });

        await collection.MongoCollection.Indexes.CreateManyAsync(new[] { index1 });
        return collection;
    }

    internal static async Task<IDatabaseCollection<Room>> GetRoomsCollection(IDatabase database)
    {
        var collection = database.GetCollection<Room>();

        var dormIndex = Builders<Room>.IndexKeys.Ascending(r => r.DormitoryId);
        var roomNumberIndex = Builders<Room>.IndexKeys.Ascending(r => r.RoomNumber);

        CreateIndexModel<Room> index1 = new(dormIndex, new CreateIndexOptions
        {
            Name = "roomDormitoryIndex"
        });

        CreateIndexModel<Room> index2 = new(roomNumberIndex, new CreateIndexOptions
        {
            Name = "roomNumberIndex"
        });

        await collection.MongoCollection.Indexes.CreateManyAsync(new[] { index1, index2 });
        return collection;
    }

    internal static async Task<IDatabaseCollection<Payment>> GetPaymentsCollection(IDatabase database)
    {
        var collection = database.GetCollection<Payment>();

        var userIndex = Builders<Payment>.IndexKeys.Ascending(p => p.UserId);

        CreateIndexModel<Payment> index1 = new(userIndex, new CreateIndexOptions
        {
            Name = "paymentUserIndex"
        });

        await collection.MongoCollection.Indexes.CreateManyAsync(new[] { index1 });
        return collection;
    }

    internal static async Task<IDatabaseCollection<Issue>> GetIssuesCollection(IDatabase database)
    {
        var collection = database.GetCollection<Issue>();

        var issuerIndex = Builders<Issue>.IndexKeys.Ascending(i => i.IssuerId);

        CreateIndexModel<Issue> index1 = new(issuerIndex, new CreateIndexOptions
        {
            Name = "issueIssuerIndex"
        });

        await collection.MongoCollection.Indexes.CreateManyAsync(new[] { index1 });
        return collection;
    }
}

public class UserDto
{
    public string? Id { get; set; }
    public string? Email { get; set; }
    public string? Name { get; set; }
    public int? Role { get; set; }
    public string RoleString => Role?.ToString() ?? string.Empty;
}
public class ExcelData
{
    public List<string> Headers { get; set; } = new();
    public List<List<ExcelCell?>> Rows { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}

public record ExcelCell(string CellType, object? Value);







