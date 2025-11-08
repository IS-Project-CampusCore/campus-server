param (
    [Parameter(Mandatory=$true)]
    [string]$ServiceName
)

Write-Host "--- 1. Creating new service: $ServiceName ---"
# Step 1: Run the template. This creates the folder and adds to .sln
dotnet new grpcservice --name $ServiceName -o $ServiceName
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to create new service from template."
    exit 1
}
Write-Host "Successfully created service and added to solution."
Write-Host ""


# --- Step 2: Update all Dockerfiles ---
Write-Host "--- 2. Updating all Dockerfiles ---"
$dockerfiles = Get-ChildItem -Path . -Filter Dockerfile -Recurse

# These are the lines to inject into every Dockerfile
$newCopyLine = "COPY $ServiceName/$ServiceName.csproj ./$ServiceName/"
$newRestoreLine = "RUN dotnet restore $ServiceName/$ServiceName.csproj"

foreach ($file in $dockerfiles) {
    Write-Host "Patching $($file.FullName)..."
    $content = Get-Content $file.FullName -Raw
    
    # Inject the new lines into the restore sections
    $content = $content -replace '(# --- END DYNAMIC RESTORE ---)', "`t$newCopyLine`n$1"
    $content = $content -replace '(# --- END DYNAMIC RESTORE RUN ---)', "`t$newRestoreLine`n$1"
    
    Set-Content -Path $file.FullName -Value $content
}
Write-Host "All Dockerfiles patched."
Write-Host ""


# --- Step 3: Update docker-compose.yml ---
Write-Host "--- 3. Updating docker-compose.yml ---"
$composeFile = "./docker-compose.yml"
$composeContent = Get-Content $composeFile -Raw

# Find the highest port used by a service (e.g., 8080, 8083)
$regex = [regex]'- "(\d+):8080"'
$matches = $regex.Matches($composeContent)
$highestPort = $matches | ForEach-Object { [int]$_.Groups[1].Value } | Sort-Object -Descending | Select-Object -First 1

if (-not $highestPort) {
    $highestPort = 8080 # Default if none found
}

$newPort = $highestPort + 1
$serviceNameLower = $ServiceName.ToLower()

# Define the new service block
$newServiceBlock = @"

  $serviceNameLower:
    build:
      context: .
      dockerfile: ./$ServiceName/Dockerfile
    ports:
      - "$newPort:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - Logging__SeqUrl=http://seq
    depends_on:
      - seq
"@

# Append the new service to the end of the file
Add-Content -Path $composeFile -Value $newServiceBlock
Write-Host "Added $ServiceName to docker-compose.yml on port $newPort."
Write-Host ""
Write-Host "--- DONE ---"
Write-Host "Next steps:"
Write-Host "1. (Manual) Open '$ServiceName.csproj' and add any gRPC Client <ProjectReference> lines if it needs to call other services."
Write-Host "2. Run 'docker compose up --build'"