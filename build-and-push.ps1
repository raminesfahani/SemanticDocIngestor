# Build and push script for Docker Compose deployment
# This script builds all images and pushes them to your container registry

param(
    [Parameter(Mandatory=$true)]
    [string]$Registry,
    
    [Parameter(Mandatory=$false)]
    [string]$Version = "latest",
    
    [Parameter(Mandatory=$false)]
    [string]$AdditionalTag = ""
)

# Configuration
$ApiImageName = "semanticdocingestor-api"
$AppHostImageName = "semanticdocingestor-apphost"

Write-Host "=== Building Docker Images ===" -ForegroundColor Green

# Build API Service
Write-Host "`nBuilding API Service..." -ForegroundColor Yellow
docker build -f src/apps/SemanticDocIngestor.AppHost.ApiService/Dockerfile -t ${ApiImageName}:${Version} .
if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to build API Service" -ForegroundColor Red
    exit 1
}

# Build AppHost
Write-Host "`nBuilding AppHost..." -ForegroundColor Yellow
docker build -f src/apps/SemanticDocIngestor.AppHost/Dockerfile -t ${AppHostImageName}:${Version} .
if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to build AppHost" -ForegroundColor Red
    exit 1
}

Write-Host "`n=== Tagging Images ===" -ForegroundColor Green

# Tag API Service
Write-Host "`nTagging API Service..." -ForegroundColor Yellow
docker tag ${ApiImageName}:${Version} ${Registry}/${ApiImageName}:${Version}
docker tag ${ApiImageName}:${Version} ${Registry}/${ApiImageName}:latest

if ($AdditionalTag) {
    docker tag ${ApiImageName}:${Version} ${Registry}/${ApiImageName}:${AdditionalTag}
}

# Tag AppHost
Write-Host "`nTagging AppHost..." -ForegroundColor Yellow
docker tag ${AppHostImageName}:${Version} ${Registry}/${AppHostImageName}:${Version}
docker tag ${AppHostImageName}:${Version} ${Registry}/${AppHostImageName}:latest

if ($AdditionalTag) {
    docker tag ${AppHostImageName}:${Version} ${Registry}/${AppHostImageName}:${AdditionalTag}
}

Write-Host "`n=== Pushing Images to Registry ===" -ForegroundColor Green

# Push API Service
Write-Host "`nPushing API Service..." -ForegroundColor Yellow
docker push ${Registry}/${ApiImageName}:${Version}
docker push ${Registry}/${ApiImageName}:latest

if ($AdditionalTag) {
    docker push ${Registry}/${ApiImageName}:${AdditionalTag}
}

# Push AppHost
Write-Host "`nPushing AppHost..." -ForegroundColor Yellow
docker push ${Registry}/${AppHostImageName}:${Version}
docker push ${Registry}/${AppHostImageName}:latest

if ($AdditionalTag) {
    docker push ${Registry}/${AppHostImageName}:${AdditionalTag}
}

Write-Host "`n=== Build and Push Complete ===" -ForegroundColor Green
Write-Host "`nImages pushed to:" -ForegroundColor Cyan
Write-Host "  - ${Registry}/${ApiImageName}:${Version}" -ForegroundColor White
Write-Host "  - ${Registry}/${ApiImageName}:latest" -ForegroundColor White
Write-Host "  - ${Registry}/${AppHostImageName}:${Version}" -ForegroundColor White
Write-Host "  - ${Registry}/${AppHostImageName}:latest" -ForegroundColor White

if ($AdditionalTag) {
    Write-Host "  - ${Registry}/${ApiImageName}:${AdditionalTag}" -ForegroundColor White
    Write-Host "  - ${Registry}/${AppHostImageName}:${AdditionalTag}" -ForegroundColor White
}
