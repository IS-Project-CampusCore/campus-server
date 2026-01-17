FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY grades/grades.csproj ./grades/

RUN dotnet restore grades/grades.csproj

COPY . .

WORKDIR "/src/grades"
RUN dotnet publish "grades.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "grades.dll"]
















