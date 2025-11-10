# ---
# SCRIPT: add-service.ps1
# 
# This script automates adding a new gRPC service to the solution.
# It calls the 'grpcservice' template and then patches all
# docker-compose and Dockerfile files to integrate the new service.
#
# USAGE:
# ./add-service.ps1 -ServiceName MyNewService
# ---

param (
    [Parameter(Mandatory=$true)]
    [string]$ServiceName
)

Write-Host "--- 1. Creating new service '$ServiceName' from template ---" -ForegroundColor Green

# Find the solution file automatically
$solutionFile = Get-ChildItem -Path . -Filter *.sln | Select-Object -First 1
if (-not $solutionFile) {
    Write-Host "Error: No .sln file found in the current directory." -ForegroundColor Red
    return
}

# Run the 'grpcservice' template
# The template itself will add the project to the solution via its post-action
dotnet new grpcservice --name $ServiceName -o $ServiceName
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: 'dotnet new' command failed. Make sure the 'grpcservice' template is installed." -ForegroundColor Red
    return
}

# --- Step 2: Add project to the solution (Reliable way) ---
Write-Host "--- 2. Adding new project to solution '$($solutionFile.Name)' ---" -ForegroundColor Green
$projectPath = Join-Path -Path $ServiceName -ChildPath "$ServiceName.csproj"
dotnet sln $solutionFile.Name add $projectPath
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: 'dotnet sln add' command failed." -ForegroundColor Red
    return
}


# --- Step 3: Update docker-compose.yml ---
Write-Host "--- 3. Updating docker-compose.yml ---" -ForegroundColor Green
$composeFile = "./docker-compose.yml"
$composeContent = Get-Content $composeFile -Raw

# Robust regex to find all external ports (e.g., "8080:8080", '8083:8080', "5342:80")
$regex = [regex]'\-\s*["'']?(\d+):\d+["'']?'
$matches = $regex.Matches($composeContent)

$portList = $matches | ForEach-Object { [int]$_.Groups[1].Value }

$highestPort = 0
if ($portList) {
    # Sort the list and get the highest number
    $highestPort = $portList | Sort-Object -Descending | Select-Object -First 1
}

# If no ports are found at all, default to 8080
if ($highestPort -lt 8080) {
    $highestPort = 8080
}

$newPort = $highestPort + 1
$serviceNameLower = $ServiceName.ToLower()

# Define the new service block
$newServiceBlock = @"

  ${serviceNameLower}:
    build:
      context: .
      dockerfile: ./$ServiceName/Dockerfile
    ports:
      - "${newPort}:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - Logging__SeqUrl=http://seq
    depends_on:
      - seq
"@

# Append the new block to the docker-compose file
Add-Content -Path $composeFile -Value $newServiceBlock
Write-Host "Added '$serviceNameLower' to docker-compose.yml on port $newPort."


# --- Step 4: Final Manual Steps ---
Write-Host "--- 4. SCRIPT COMPLETE ---" -ForegroundColor Green
Write-Host ""
Write-Host "There are a few manual steps left:" -ForegroundColor Yellow
Write-Host " 1. Update 'commons.csproj' to include this new .proto file."
Write-Host ""
Write-Host " 2. Update 'http.csproj' (and any other client) to be able to call this service:"
Write-Host "    - Add a new <Protobuf ... /> item to 'http.csproj' for the new proto file."
Write-Host "    - Add the new client to 'Program.cs' with 'builder.Services.AddGrpcClient<...>()'"
Write-Host ""
Write-Host "Done."