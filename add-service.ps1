# ---
# SCRIPT: add-service.ps1
# 
# This script automates adding a new gRPC service to the solution.
# 
# NAMING CONVENTIONS:
# - Folder & Project Files (.csproj): lowercase (e.g., 'grades/grades.csproj')
# - C# Classes & Namespaces: PascalCase (e.g., 'public class GradesService')
# - Internal Proto Package: camelCase (e.g., 'package gradesService')
#
# USAGE:
# ./add-service.ps1 -ServiceName Grades
# ---

param (
    [Parameter(Mandatory=$true)]
    [string]$ServiceName
)

# --- 1. Calculate Casing Variants ---
# PascalCase: "Grades" (For Namespaces, Classes)
$PascalName = $ServiceName.Substring(0,1).ToUpper() + $ServiceName.Substring(1)

# camelCase: "grades" (For internal Proto/Variables)
$CamelName  = $ServiceName.Substring(0,1).ToLower() + $ServiceName.Substring(1)

# Lowercase: "grades" (For File Names and Folder Names)
$LowerName  = $ServiceName.ToLower()

Write-Host "--- 1. Creating new service '$PascalName' in folder '$LowerName' ---" -ForegroundColor Green

# Find the ROOT solution file automatically
$rootSolution = Get-ChildItem -Path . -Filter *.sln | Select-Object -First 1
if (-not $rootSolution) {
    Write-Host "Error: No .sln file found in the current directory." -ForegroundColor Red
    return
}

# --- 2. Run the Template ---
# We use $PascalName for the name so the CODE (Namespaces/Classes) is generated as 'Grades'.
# We use $LowerName for the output folder so the directory is 'grades'.
dotnet new grpcservice --name $PascalName -o $LowerName
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: 'dotnet new' command failed." -ForegroundColor Red
    return
}

# --- 3. Rename Project/Solution Files to Lowercase ---
Write-Host "--- 2. Renaming artifacts to lowercase ($LowerName) ---" -ForegroundColor Cyan

$targetDir = Join-Path . $LowerName

# Rename .csproj (e.g., Grades.csproj -> grades.csproj)
$generatedCsproj = Join-Path $targetDir "$PascalName.csproj"
$finalCsproj     = Join-Path $targetDir "$LowerName.csproj"

if (Test-Path $generatedCsproj) {
    Rename-Item -Path $generatedCsproj -NewName "$LowerName.csproj"
    Write-Host "   Renamed project file to: $LowerName.csproj" -ForegroundColor Gray
}

# Rename .sln if the template generated one (e.g., Grades.sln -> grades.sln)
$generatedSln = Join-Path $targetDir "$PascalName.sln"
if (Test-Path $generatedSln) {
    Rename-Item -Path $generatedSln -NewName "$LowerName.sln"
    Write-Host "   Renamed solution file to: $LowerName.sln" -ForegroundColor Gray
}

# --- 4. Apply camelCase Patches to Code ---
Write-Host "--- 3. Applying camelCase patches ($CamelName) ---" -ForegroundColor Cyan

# A. Replace text content inside files (__CAMEL_NAME__ -> grades)
Get-ChildItem -Path $targetDir -Recurse -File | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    if ($content -match "__CAMEL_NAME__") {
        $newContent = $content -replace "__CAMEL_NAME__", $CamelName
        Set-Content -Path $_.FullName -Value $newContent
        Write-Host "   Updated content in: $($_.Name)" -ForegroundColor Gray
    }
}

# B. Rename files containing __CAMEL_NAME__ (e.g., __CAMEL_NAME__Service.proto -> gradesService.proto)
Get-ChildItem -Path $targetDir -Recurse | Where-Object { $_.Name -match "__CAMEL_NAME__" } | ForEach-Object {
    $newName = $_.Name -replace "__CAMEL_NAME__", $CamelName
    Rename-Item -Path $_.FullName -NewName $newName
    Write-Host "   Renamed file to: $newName" -ForegroundColor Gray
}


# --- 5. Add Project to the Root Solution ---
Write-Host "--- 4. Adding project to solution '$($rootSolution.Name)' ---" -ForegroundColor Green

# Note: We now point to the lowercase project file
$projectPath = Join-Path -Path $LowerName -ChildPath "$LowerName.csproj"

if (Test-Path $projectPath) {
    dotnet sln $rootSolution.Name add $projectPath
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error: 'dotnet sln add' command failed." -ForegroundColor Red
        return
    }
} else {
    Write-Host "Error: Could not find project file at $projectPath" -ForegroundColor Red
    return
}


# --- 6. Update docker-compose.yml ---
Write-Host "--- 5. Updating docker-compose.yml ---" -ForegroundColor Green
$composeFile = "./docker-compose.yml"

if (Test-Path $composeFile) {
    $composeContent = Get-Content $composeFile -Raw

    # Find highest port used
    $regex = [regex]'\-\s*["'']?(\d+):\d+["'']?'
    $matches = $regex.Matches($composeContent)
    $portList = $matches | ForEach-Object { [int]$_.Groups[1].Value }

    $highestPort = 0
    if ($portList) {
        $highestPort = $portList | Sort-Object -Descending | Select-Object -First 1
    }
    if ($highestPort -lt 8080) { $highestPort = 8080 }

    $newPort = $highestPort + 1
    
    # Use lowercase name for the docker service key
    $serviceKey = $LowerName 

    # Define the new service block
    # Note: dockerfile path uses lowercase folder
    $newServiceBlock = @"

  ${serviceKey}:
    build:
      context: .
      dockerfile: ./$LowerName/Dockerfile
    ports:
      - "${newPort}:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - Logging__SeqUrl=http://seq
    depends_on:
      - seq
    networks:
      - campus-network
"@

    Add-Content -Path $composeFile -Value $newServiceBlock
    Write-Host "Added '$serviceKey' to docker-compose.yml on port $newPort."
} else {
    Write-Host "Warning: docker-compose.yml not found." -ForegroundColor Yellow
}


# --- 7. Final Instructions ---
Write-Host "--- 6. SCRIPT COMPLETE ---" -ForegroundColor Green
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host " 1. Update 'commons.csproj' to include the new .proto file."
Write-Host " 2. Update client projects (Add <Protobuf> reference & AddGrpcClient)."
Write-Host ""
Write-Host "Done."