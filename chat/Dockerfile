FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY chat/chat.csproj ./chat/

RUN dotnet restore chat/chat.csproj

COPY . .

WORKDIR "/src/chat"
RUN dotnet publish "chat.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "chat.dll"]
















