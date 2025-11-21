using users.Model;
using usersServiceClient;

namespace users.Utils;

public static class ExcelToRegisterRequest
{
    public static List<RegisterRequest> ConvertToRegisterRequest(ExcelData excelData)
    {
        var requests = new List<RegisterRequest>();

        var headerMap = excelData.Headers
            .Select((header, index) => new { Header = header, Index = index })
            .ToDictionary(x => x.Header, x => x.Index, StringComparer.OrdinalIgnoreCase);

        if (!headerMap.ContainsKey("Name") ||
            !headerMap.ContainsKey("Email") ||
            !headerMap.ContainsKey("Role"))
        {
            throw new Exception("Excel file is missing required columns (Name, Email, or Role).");
        }

        int nameIndex = headerMap["Name"];
        int emailIndex = headerMap["Email"];
        int roleIndex = headerMap["Role"];

        foreach (var row in excelData.Rows)
        {
            if (row.Count <= nameIndex || row.Count <= emailIndex || row.Count <= roleIndex)
                continue;

            string nameValue = row[nameIndex] ?? "Unknown";
            string emailValue = row[emailIndex] ?? "no-email";
            string roleString = row[roleIndex] ?? "Guest";

            var newReq = new RegisterRequest
            {
                Name = nameValue,
                Email = emailValue,
                Role = roleString
            };

            requests.Add(newReq);
        }

        return requests;
    }
}
