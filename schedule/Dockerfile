FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY schedule/schedule.csproj ./schedule/

RUN dotnet restore schedule/schedule.csproj

COPY . .

WORKDIR "/src/schedule"
RUN dotnet publish "schedule.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "schedule.dll"]
















