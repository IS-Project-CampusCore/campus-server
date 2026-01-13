FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY announcements/announcements.csproj ./announcements/

RUN dotnet restore announcements/announcements.csproj

COPY . .

WORKDIR "/src/announcements"
RUN dotnet publish "announcements.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "announcements.dll"]
















