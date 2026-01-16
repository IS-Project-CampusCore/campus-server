using commons.RequestBase;
using MassTransit.Util;
using users.Model;

namespace users.Utils;

public static class ExcelToUserModels
{
    private static readonly string[] s_excelHeader = { "Email", "Name", "Role", "University", "Year", "Group", "Major", "Department", "Title" };

    public static (List<User>, List<string>) ConvertToUserModels(ExcelData excelData, bool requireName = true, bool requireRole = true)
    {
        var users = new List<User>();
        var errors = new List<string>();

        if (excelData.Headers.Except(s_excelHeader).Any())
        {
            throw new BadRequestException($"[Excel Error] Excel file is missing required columns.");
        }

        var headerMap = excelData.Headers
            .Select((header, index) => new { Header = header, Index = index })
            .ToDictionary(x => x.Header, x => x.Index, StringComparer.OrdinalIgnoreCase);

        foreach (var row in excelData.Rows)
        {
            int rowNr = excelData.Rows.IndexOf(row) + 1;

            if (!TryStringFromCell(row, headerMap["Email"], out string emailValue))
            {
                errors.Add(FieldError("Email", rowNr, headerMap["Email"] + 1));
            }

            if (!TryStringFromCell(row, headerMap["Name"], out string nameValue))
            {
                errors.Add(FieldError("Name", rowNr, headerMap["Name"] + 1));
            }

            if (!TryStringFromCell(row, headerMap["Role"], out string roleString))
            {
                errors.Add(FieldError("Role", rowNr, headerMap["Role"] + 1));
            }

            if (string.IsNullOrEmpty(emailValue) || (string.IsNullOrEmpty(nameValue) && requireName) || (string.IsNullOrEmpty(roleString) && requireRole))
                continue;

            UserType role = User.StringToRole(roleString);

            User newUser;

            try
            {
                switch (role)
                {
                    case UserType.CAMPUS_STUDENT:
                        if (!TryStringFromCell(row, headerMap["University"], out string university))
                        {
                            errors.Add(FieldError("University", rowNr, headerMap["University"] + 1));
                        }
                        if (!TryIntFromCell(row, headerMap["Year"], out int year))
                        {
                            errors.Add(FieldError("Year", rowNr, headerMap["Year"] + 1));
                        }
                        if (!TryIntFromCell(row, headerMap["Group"], out int group))
                        {
                            errors.Add(FieldError("Group", rowNr, headerMap["Group"] + 1));
                        }
                        if (!TryStringFromCell(row, headerMap["Major"], out string major))
                        {
                            errors.Add(FieldError("Major", rowNr, headerMap["Major"] + 1));
                        }

                        if (string.IsNullOrEmpty(university) || string.IsNullOrEmpty(major) || group == 0 || year == 0)
                            continue;

                        newUser = new CampusStudent
                        {
                            Email = emailValue,
                            Name = nameValue,
                            Role = role,
                            University = university,
                            Year = year,
                            Group = group,
                            Major = major
                        };
                        users.Add(newUser);
                        break;
                    case UserType.STUDENT:
                        if (!TryStringFromCell(row, headerMap["University"], out university))
                        {
                            errors.Add(FieldError("University", rowNr, headerMap["University"] + 1));
                        }
                        if (!TryIntFromCell(row, headerMap["Year"], out year))
                        {
                            errors.Add(FieldError("Year", rowNr, headerMap["Year"] + 1));
                        }
                        if (!TryIntFromCell(row, headerMap["Group"], out group))
                        {
                            errors.Add(FieldError("Group", rowNr, headerMap["Group"] + 1));
                        }
                        if (!TryStringFromCell(row, headerMap["Major"], out major))
                        {
                            errors.Add(FieldError("Major", rowNr, headerMap["Major"] + 1));
                        }

                        if (string.IsNullOrEmpty(university) || string.IsNullOrEmpty(major) || group == 0 || year == 0)
                            continue;

                        newUser = new Student
                        {
                            Email = emailValue,
                            Name = nameValue,
                            Role = role,
                            University = university,
                            Year = year,
                            Group = group,
                            Major = major
                        };
                        users.Add(newUser);
                        break;
                    case UserType.PROFESSOR:
                        if (!TryStringFromCell(row, headerMap["University"], out university))
                        {
                            errors.Add(FieldError("University", rowNr, headerMap["University"] + 1));
                        }
                        if (!TryStringFromCell(row, headerMap["Department"], out string department))
                        {
                            errors.Add(FieldError("Department", rowNr, headerMap["Department"] + 1));
                        }
                        if (!TryStringFromCell(row, headerMap["Title"], out string title))
                        {
                            errors.Add(FieldError("Title", rowNr, headerMap["Title"] + 1));
                        }

                        if (string.IsNullOrEmpty(university) || string.IsNullOrEmpty(department) || string.IsNullOrEmpty(title))
                            continue;

                        newUser = new Professor
                        {
                            Email = emailValue,
                            Name = nameValue,
                            Role = role,
                            University = university,
                            Department = department,
                            Title = title
                        };
                        users.Add(newUser);
                        break;
                    case UserType.MANAGEMENT:
                        newUser = new Management
                        {
                            Email = emailValue,
                            Name = nameValue,
                            Role = role
                        };
                        users.Add(newUser);
                        break;
                    case UserType.ADMIN:
                        newUser = new Admin
                        {
                            Email = emailValue,
                            Name = nameValue,
                            Role = role
                        };
                        users.Add(newUser);
                        break;
                    case UserType.GUEST:
                        newUser = new User
                        {
                            Email = emailValue,
                            Name = nameValue,
                            Role = role
                        };
                        users.Add(newUser);
                        break;
                    default:
                        errors.Add($"[Bulk Register Error] Invalid role at row:{rowNr}, column:{headerMap["Role"] + 1}");
                        break;
                }
            }
            catch (Exception ex)
            {
                throw new InternalErrorException(ex.Message);
            }
        }

        return (users, errors);
    }

    private static bool TryStringFromCell(List<ExcelCell?> row, int col, out string value)
    {
        value = row[col]?.Value?.ToString() ?? string.Empty;
        return !string.IsNullOrEmpty(value);
    }

    private static bool TryIntFromCell(List<ExcelCell?> row, int col, out int value)
    {
        if (int.TryParse(row[col]?.Value?.ToString(), out var result))
        {
            value = result;
            return true;
        }

        value = 0;
        return false;
    }

    private static string FieldError(string field, int row, int col)
    {
        return $"[Bulk Register Error] '{field}' field missing at row:{row}, column:{col}";
    }
}