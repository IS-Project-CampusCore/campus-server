FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY notification/notification.csproj ./notification/

RUN dotnet restore notification/notification.csproj

COPY . .

WORKDIR "/src/notification"
RUN dotnet publish "notification.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "notification.dll"]
















