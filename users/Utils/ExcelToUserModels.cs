using commons.RequestBase;
using MassTransit.NewIdProviders;
using MassTransit.Util;
using users.Model;
using usersServiceClient;

namespace users.Utils;

public static class ExcelToUserModels
{
    private static readonly string[] s_excelHeader = { "Email", "Name", "Role", "University", "Year", "Group", "Major", "Dormitory", "Room", "Department", "Title" };

    public static List<User> ConvertToUserModels(ExcelData excelData)
    {
        var users = new List<User>();

        if (excelData.Headers.Except(s_excelHeader).Any())
        {
            throw new BadRequestException("[Excel Error] Excel file is missing required columns (Name, Email, or Role).");
        }

        var headerMap = excelData.Headers
            .Select((header, index) => new { Header = header, Index = index })
            .ToDictionary(x => x.Header, x => x.Index, StringComparer.OrdinalIgnoreCase);

        foreach (var row in excelData.Rows)
        {
            string emailValue = GetStringFromCell(row, headerMap["Email"]) ?? throw new BadRequestException("Email field missing");
            string nameValue = GetStringFromCell(row, headerMap["Name"]) ?? throw new BadRequestException("Name field missing");
            string roleString = GetStringFromCell(row, headerMap["Role"]) ?? throw new BadRequestException("Role field missing");

            UserType role = User.StringToRole(roleString);

            User newUser;

            try
            {
                newUser = role switch
                {
                    UserType.CAMPUS_STUDENT => new CampusStudent
                    {
                        Email = emailValue,
                        Name = nameValue,
                        Role = role,
                        University = GetStringFromCell(row, headerMap["University"]) ?? throw new BadRequestException("'University' field is mising for Campus Student"),
                        Year = GetIntFromCell(row, headerMap["Year"]) ?? throw new BadRequestException("'Year' field is mising for Campus Student"),
                        Group = GetIntFromCell(row, headerMap["Group"]) ?? throw new BadRequestException("'Group' field is mising for Campus Student"),
                        Major = GetStringFromCell(row, headerMap["Major"]) ?? throw new BadRequestException("'Major' field is mising for Campus Student"),
                        Dormitory = GetStringFromCell(row, headerMap["Dormitory"]) ?? throw new BadRequestException("'Dormitory' field is mising for Campus Student"),
                        Room = GetIntFromCell(row, headerMap["Room"]) ?? throw new BadRequestException("'Room' field is mising for Campus Student")
                    },

                    UserType.STUDENT => new Student
                    {
                        Email = emailValue,
                        Name = nameValue,
                        Role = role,
                        University = GetStringFromCell(row, headerMap["University"]) ?? throw new BadRequestException("'University' field is mising for Student"),
                        Year = GetIntFromCell(row, headerMap["Year"]) ?? throw new BadRequestException("'Year' field is mising for Student"),
                        Group = GetIntFromCell(row, headerMap["Group"]) ?? throw new BadRequestException("'Group' field is mising for Student"),
                        Major = GetStringFromCell(row, headerMap["Major"]) ?? throw new BadRequestException("'Major' field is mising for Student")
                    },

                    UserType.PROFESSOR => new Professor
                    {
                        Email = emailValue,
                        Name = nameValue,
                        Role = role,
                        University = GetStringFromCell(row, headerMap["University"]) ?? throw new BadRequestException("'University' field is mising for Professor"),
                        Subjects = [],
                        Department = GetStringFromCell(row, headerMap["Department"]) ?? throw new BadRequestException("'Department' field is mising for Professor"),
                        Title = GetStringFromCell(row, headerMap["Title"]) ?? throw new BadRequestException("'Title' field is mising for Professor")
                    },

                    UserType.MANAGEMENT => new Management
                    {
                        Email = emailValue,
                        Name = nameValue,
                        Role = role,
                    },

                    UserType.ADMIN => new Admin
                    {
                        Email = emailValue,
                        Name = nameValue,
                        Role = role,
                    },

                    UserType.GUEST => new User
                    {
                        Email = emailValue,
                        Name = nameValue,
                        Role = role,
                    },

                    _ => throw new BadRequestException("Unsuported role")
                };

                users.Add(newUser);
            }
            catch (Exception ex)
            {
                throw new InternalErrorException(ex.Message);
            }
        }

        return users;
    }

    private static string? GetStringFromCell(List<ExcelCell?> row, int col)
    {
        return row[col]?.Value?.ToString();
    }

    private static int? GetIntFromCell(List<ExcelCell?> row, int col) {
        if (int.TryParse(row[col]?.Value?.ToString(), out var result))
            return result;
        return null;
    }
}