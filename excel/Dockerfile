FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY excel/excel.csproj ./excel/

RUN dotnet restore excel/excel.csproj

COPY . .

WORKDIR "/src/excel"
RUN dotnet publish "excel.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "excel.dll"]















