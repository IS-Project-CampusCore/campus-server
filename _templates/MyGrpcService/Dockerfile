FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY __CAMEL_NAME__/__CAMEL_NAME__.csproj ./__CAMEL_NAME__/

RUN dotnet restore __CAMEL_NAME__/__CAMEL_NAME__.csproj

COPY . .

WORKDIR "/src/__CAMEL_NAME__"
RUN dotnet publish "__CAMEL_NAME__.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "__CAMEL_NAME__.dll"]















