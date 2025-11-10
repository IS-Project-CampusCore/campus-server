FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY users/users.csproj ./users/

RUN dotnet restore users/users.csproj

COPY . .

WORKDIR "/src/users"
RUN dotnet publish "users.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "users.dll"]















