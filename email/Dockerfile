FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY email/email.csproj ./email/

RUN dotnet restore email/email.csproj

COPY . .

WORKDIR "/src/email"
RUN dotnet publish "email.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "email.dll"]















