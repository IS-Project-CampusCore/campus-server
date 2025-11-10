# This Dockerfile assumes it is being built from the SOLUTION ROOT

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# === THIS IS THE FIX ===
# Copy all .csproj files, *preserving* their folder structure
COPY SomeService/SomeService.csproj ./SomeService/

# Restore dependencies for each project individually
# This will now work, as the paths are correct
RUN dotnet restore SomeService/SomeService.csproj

# Copy the rest of the source code
COPY . .

# Publish the specific project
WORKDIR "/src/SomeService"
RUN dotnet publish "SomeService.csproj" -c Release -o /app/publish

# Stage 2: Final Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SomeService.dll"]















