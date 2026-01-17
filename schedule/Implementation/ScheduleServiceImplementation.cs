using commons.Database;
using commons.RequestBase;
using commons.Tools;
using excelServiceClient;
using MassTransit;
using MongoDB.Driver;
using schedule.Models;

namespace schedule.Implementation;

public class ScheduleServiceImplementation(
    ILogger<ScheduleServiceImplementation> logger,
    excelService.excelServiceClient excelService,
    IDatabase database
)
{
    private readonly ILogger<ScheduleServiceImplementation> _logger = logger;
    private readonly excelService.excelServiceClient _excelService = excelService;

    private readonly AsyncLazy<IDatabaseCollection<UniversitySchedule>> _uniSchedule = new(() => GetUniScheduleCollection(database));
    private readonly AsyncLazy<IDatabaseCollection<AccommodationSchedule>> _accSchedule = new(() => GetAccScheduleCollection(database));

    private static readonly string[] s_days = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"];

    public async Task<List<UniversitySchedule>> GetUniversitySchedulesAsync(string university, string major, int year, int group)
    {
        if (year <= 0)
            return await GetUniversitySchedulesAsync(university, major);

        if (group <= 0)
            return await GetUniversitySchedulesAsync(university, major, year);

        var schedules = await _uniSchedule;
        return await schedules.MongoCollection.Find(s => s.University == university && s.Major == major && s.Year == year && s.Group == group).ToListAsync();
    }

    public async Task<List<UniversitySchedule>> GetUniversitySchedulesAsync(string university, string major, int year)
    {
        var schedules = await _uniSchedule;
        return await schedules.MongoCollection.Find(s => s.University == university && s.Major == major && s.Year == year).ToListAsync();
    }

    public async Task<List<UniversitySchedule>> GetUniversitySchedulesAsync(string university, string major)
    {
        var schedules = await _uniSchedule;
        return await schedules.MongoCollection.Find(s => s.University == university && s.Major == major).ToListAsync();
    }

    public async Task<AccommodationSchedule> GetAccommodationScheduleAsync(string name)
    {
        var schedules = await _accSchedule;
        return await schedules.GetOneAsync(s => s.Name ==  name);
    }

    public async Task<BulkResult<List<UniversitySchedule>>> UpsertScheduleAsync(string fileName, string university, string major, int year, int numberOfGroups)
    {
        List<string> headers;
        List<List<(string? CellType, object? Value)>>? rows;

        string[] types = ["string", "double"];
        types = [.. types, .. Enumerable.Repeat("string?", numberOfGroups).ToArray()];

        (headers, rows) = await GetExcelData(fileName, types);
        if (rows is null)
        {
            throw new BadRequestException("Excel is empty");
        }

        if (headers.GetRange(0, 2).Except(["Day", "Hour"]).Any() || headers.Count != types.Length)
        {
            _logger.LogError("Excel template does not match");
            throw new BadRequestException("Excel template does not match");
        }

        int[] groups = new int[numberOfGroups];
        try
        {
            for (int i = 0; i < numberOfGroups; i++)
            {
                groups[i] = int.Parse(headers[i + 2]);
            }
        }
        catch (Exception e) 
        {
            _logger.LogError(e, $"Could not get group number Exception:{e.GetType().Name}, Msg:{e.Message}");
            throw new BadRequestException("Group number cannot be parsed");
        }

        int total = 0;
        int success = 0;
        int skipped = 0;
        List<string> errors = [];

        Dictionary<(string, int), List<UniScheduleData>> scheduleOfGroupPerDay = [];
        foreach (var row in rows)
        {
            total++;

            object? day = row.ElementAt(0).Value;
            object? hour = row.ElementAt(1).Value;

            if (day is not string dayStr || hour is not int hourInt)
            {
                skipped++;
                errors.Add($"Missing data at row:{total}");
                continue;
            }

            if (!s_days.Contains(dayStr))
            {
                _logger.LogError($"'{dayStr}' is not a valid day of week");
                skipped++;
                errors.Add($"Invalid day of week at rpw:{total}, column:1");
                continue;
            }

            if (hourInt < 0 || hourInt > 23)
            {
                skipped++;
                errors.Add($"Invalid hour at row:{total}");
                continue;
            }

            for (int i = 0; i < numberOfGroups; i++)
            {
                object? data = row.ElementAt(i + 2).Value;
                if (data is null)
                {
                    continue;
                }

                if (data is not string dataStr)
                {
                    skipped++;
                    errors.Add($"Cannot get schedule data at row:{total}, column:{i + 2}");
                    continue;
                }

                string[] courseAndLocation = dataStr.Split('-');
                if (courseAndLocation.Length != 2 )
                {
                    skipped++;
                    errors.Add($"Cannot get the schedule data as 'Course-Location' at row:{total}, column:{i + 2}");
                    continue;
                }

                if (!scheduleOfGroupPerDay.ContainsKey((dayStr, groups[i])))
                {
                    scheduleOfGroupPerDay.Add((dayStr, groups[i]), []);
                }

                UniScheduleData scheduleData = new UniScheduleData(hourInt, courseAndLocation[0], courseAndLocation[1]);
                scheduleOfGroupPerDay[(dayStr, groups[i])].Add(scheduleData);
            }

            success++;
        }

        var schedules = await _uniSchedule;

        List<UniversitySchedule> uniSchedules = [];
        foreach (var group in groups)
        {
            for (int d = 0; d < 7; d++)
            {
                if (!scheduleOfGroupPerDay.ContainsKey((s_days[d], group)) || !scheduleOfGroupPerDay[(s_days[d], group)].Any())
                {
                    continue;
                }

                UniversitySchedule newSchedule = new UniversitySchedule
                {
                    University = university,
                    Major = major,
                    Year = year,
                    Day = s_days[d],
                    Group = group,
                    Schedule = scheduleOfGroupPerDay[(s_days[d], group)].OrderBy(sd => sd.Hour).ToList()
                };

                UniversitySchedule oldSchedule = await schedules.GetOneAsync(us => us.University == university && us.Year == year && us.Day == s_days[d] && us.Group == group);
                if (oldSchedule is null)
                {
                    await schedules.InsertAsync(newSchedule);
                }
                else
                {
                    newSchedule.Id = oldSchedule.Id;
                    await schedules.ReplaceAsync(newSchedule);
                }

                uniSchedules.Add(newSchedule);
            }
        }

        return new BulkResult<List<UniversitySchedule>>
        {
            TotalCount = total,
            SuccessCount = success,
            SkipedCount = skipped,
            Result = uniSchedules,
            Errors = errors
        };
    }

    public async Task<BulkResult<AccommodationSchedule>> UpsertScheduleAsync(string fileName, string accommodationName)
    {
        List<string> headers;
        List<List<(string? CellType, object? Value)>>? rows;

        string[] types = ["double", "string?", "string?", "string?", "string?", "string?", "string?", "string?"];

        (headers, rows) = await GetExcelData(fileName, types);
        if (rows is null)
        {
            throw new BadRequestException("Excel is empty");
        }

        if (rows is null)
        {
            throw new BadRequestException("Excel is empty");
        }

        if (headers.GetRange(0, 2).Except(["Hour", "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"]).Any() || headers.Count != types.Length)
        {
            _logger.LogError("Excel template does not match");
            throw new BadRequestException("Excel template does not match");
        }

        int total = 0;
        int success = 0;
        int skipped = 0;
        List<string> errors = [];

        Dictionary<string, List<AccScheduleData>> weekSchedule = [];
        foreach (var row in rows)
        {
            total++;

            object? hour = row.ElementAt(0).Value;
            if (hour is not int hourInt)
            {
                skipped++;
                errors.Add($"Missing data at row:{total}");
                continue;
            }

            if (hourInt < 0 || hourInt > 23)
            {
                skipped++;
                errors.Add($"Invalid hour at row:{total}");
                continue;
            }

            for (int d = 0; d < 7; d++)
            {
                object? status = row.ElementAt((int)d + 1).Value;
                if (status is not string statusStr)
                {
                    skipped++;
                    errors.Add($"Missing data at row:{total}");
                    continue;
                }

                if (statusStr != "Open" && statusStr != "Close")
                {
                    skipped++;
                    errors.Add($"Invalid status at row:{total}");
                    continue;
                }

                if (!weekSchedule.ContainsKey(s_days[d]) || weekSchedule[s_days[d]] is null || !weekSchedule[s_days[d]].Any())
                {
                    weekSchedule.Add(s_days[d], new List<AccScheduleData>());
                }

                AccScheduleData data = new AccScheduleData(hourInt, statusStr);
                weekSchedule[s_days[d]].Add(data);
            }

            success++;
        }

        AccommodationSchedule newSchedule = new AccommodationSchedule
        {
            Name = accommodationName,
            Schedule = weekSchedule
        };

        var schedules = await _accSchedule;

        AccommodationSchedule oldSchedule = await schedules.GetOneAsync(us => us.Name == accommodationName);
        if (oldSchedule is null)
        {
            await schedules.InsertAsync(newSchedule);
        }
        else
        {
            newSchedule.Id = oldSchedule.Id;
            await schedules.ReplaceAsync(newSchedule);
        }

        return new BulkResult<AccommodationSchedule>
        {
            TotalCount = total,
            SuccessCount = success,
            SkipedCount = skipped,
            Result = newSchedule,
            Errors = errors
        };
    }

    private async Task<(List<string> Headers, List<List<(string? CellType, object? Value)>>? Rows)> GetExcelData(string fileName, string[] types)
    {
        if (fileName is null)
            throw new BadRequestException("No file selected");

        var request = new ParseExcelRequest
        {
            FileName = fileName
        };

        request.CellTypes.AddRange(types);

        var response = await _excelService.ParseExcelAsync(request);
        if (!response.Success)
            throw new InternalErrorException(response.Errors);

        if (string.IsNullOrEmpty(response.Body))
            throw new BadRequestException("Empty excel file");

        var payload = response.Payload;
        var errors = payload.GetArray("Errors").IterateStrings().ToList();

        if (errors is not null && errors.Any())
        {
            string aggregatedErrors = string.Join("\n", errors);
            _logger.LogError($"Excel parsing failed with {errors.Count()} errors for file {fileName}");
            throw new BadRequestException(aggregatedErrors);
        }

        var headers = payload.GetArray("Headers").IterateStrings().ToList();
        var rows = payload.GetArray("Rows")
            .Iterate()
            .Select(rowBody => rowBody.Iterate()
            .Select(cellBody => (
                CellType: cellBody.TryGetString("CellType"),
                Value: ExtractValue(cellBody)
            ))
            .ToList())
            .ToList();

        if (headers is null || !headers.Any() || rows is null || !rows.Any())
        {
            _logger.LogError("Excel could not be parsed");
            throw new InternalErrorException("Excel could not be parsed");
        }

        return (headers, rows);
    }

    private object? ExtractValue(commons.Protos.MessageBody cell)
    {
        if (cell.TryGetString("Value") is string s)
            return s;

        if (cell.TryGetInt32("Value") is int b)
            return b;

        return null;
    }

    internal static async Task<IDatabaseCollection<UniversitySchedule>> GetUniScheduleCollection(IDatabase database)
    {
        var collection = database.GetCollection<UniversitySchedule>();

        var uniIndex = Builders<UniversitySchedule>.IndexKeys.Ascending(us => us.University);
        var yearIndex = Builders<UniversitySchedule>.IndexKeys.Ascending(us => us.Year);
        var groupIndex = Builders<UniversitySchedule>.IndexKeys.Ascending(us => us.Group);

        await collection.MongoCollection.Indexes.CreateManyAsync([
            new CreateIndexModel<UniversitySchedule>(uniIndex, new CreateIndexOptions { Name = "uniIndex" }),
            new CreateIndexModel<UniversitySchedule>(yearIndex, new CreateIndexOptions { Name = "yearIndex" }),
            new CreateIndexModel<UniversitySchedule>(groupIndex, new CreateIndexOptions { Name = "groupIndex" })
        ]);
        return collection;
    }

    internal static async Task<IDatabaseCollection<AccommodationSchedule>> GetAccScheduleCollection(IDatabase database)
    {
        var collection = database.GetCollection<AccommodationSchedule>();

        var nameIndex = Builders<AccommodationSchedule>.IndexKeys.Ascending(us => us.Name);

        await collection.MongoCollection.Indexes.CreateManyAsync([
            new CreateIndexModel<AccommodationSchedule>(nameIndex, new CreateIndexOptions { Name = "nameIndex" , Unique = true})
        ]);
        return collection;
    }
}
