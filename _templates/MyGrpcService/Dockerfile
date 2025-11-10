FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY MyGrpcService/MyGrpcService.csproj ./MyGrpcService/

RUN dotnet restore MyGrpcService/MyGrpcService.csproj

COPY . .

WORKDIR "/src/MyGrpcService"
RUN dotnet publish "MyGrpcService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MyGrpcService.dll"]















