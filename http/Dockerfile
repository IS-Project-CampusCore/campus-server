FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY http/http.csproj ./http/

RUN dotnet restore http/http.csproj

COPY . .

WORKDIR "/src/http"
RUN dotnet publish "http.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "http.dll"]