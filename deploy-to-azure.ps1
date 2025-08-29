# Deploy ZapFinance to Azure App Service
# Make sure you have Azure CLI installed and are logged in

Write-Host "Starting deployment to Azure App Service..." -ForegroundColor Green

# Build the project
Write-Host "Building the project..." -ForegroundColor Yellow
dotnet build SERVICES/Core.Service/Core.Service/Core.Service.csproj --configuration Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

# Publish the project
Write-Host "Publishing the project..." -ForegroundColor Yellow
dotnet publish SERVICES/Core.Service/Core.Service/Core.Service.csproj -c Release -o ./publish

if ($LASTEXITCODE -ne 0) {
    Write-Host "Publish failed!" -ForegroundColor Red
    exit 1
}

# Deploy to Azure using Azure CLI
Write-Host "Deploying to Azure App Service..." -ForegroundColor Yellow
az webapp deployment source config-zip --resource-group zapfinance-rg --name zapfinanceapi-f7eagyetbdf5azhm --src ./publish.zip

# Create zip file for deployment
Write-Host "Creating deployment package..." -ForegroundColor Yellow
Compress-Archive -Path ./publish/* -DestinationPath ./publish.zip -Force

# Deploy using Azure CLI
az webapp deployment source config-zip --resource-group zapfinance-rg --name zapfinanceapi-f7eagyetbdf5azhm --src ./publish.zip

if ($LASTEXITCODE -eq 0) {
    Write-Host "Deployment successful!" -ForegroundColor Green
    Write-Host "Your app is available at: https://zapfinanceapi-f7eagyetbdf5azhm.brazilsouth-01.azurewebsites.net" -ForegroundColor Green
} else {
    Write-Host "Deployment failed!" -ForegroundColor Red
}

# Clean up
Remove-Item ./publish -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item ./publish.zip -Force -ErrorAction SilentlyContinue
