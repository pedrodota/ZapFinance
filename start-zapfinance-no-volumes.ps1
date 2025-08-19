# Script para inicializar o ZapFinance SEM volumes compartilhados
Write-Host "🚀 Iniciando ZapFinance (sem volumes compartilhados)..." -ForegroundColor Green

# Verificar se Docker está rodando
Write-Host "📋 Verificando Docker..." -ForegroundColor Yellow
docker --version
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Docker não está instalado ou não está rodando!" -ForegroundColor Red
    Write-Host "💡 Certifique-se de que o Docker Desktop está rodando" -ForegroundColor Yellow
    exit 1
}

# Verificar se Docker Compose está disponível
docker-compose --version
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Docker Compose não está disponível!" -ForegroundColor Red
    exit 1
}

# Limpar containers anteriores se existirem
Write-Host "🧹 Limpando containers anteriores..." -ForegroundColor Yellow
docker-compose -f docker-compose.production.yml down -v

# Build e start dos serviços usando arquivo de produção
Write-Host "🔨 Fazendo build e iniciando serviços..." -ForegroundColor Yellow
docker-compose -f docker-compose.production.yml up --build -d

# Aguardar serviços ficarem prontos
Write-Host "⏳ Aguardando serviços ficarem prontos..." -ForegroundColor Yellow
Start-Sleep -Seconds 30

# Verificar status dos serviços
Write-Host "📊 Status dos serviços:" -ForegroundColor Green
docker-compose -f docker-compose.production.yml ps

Write-Host ""
Write-Host "✅ ZapFinance iniciado com todos os serviços!" -ForegroundColor Green
Write-Host "📍 Endpoints disponíveis:" -ForegroundColor Cyan
Write-Host "   🌐 API Gateway (Nginx): http://localhost:80" -ForegroundColor Yellow
Write-Host "   🔗 Core Service: http://localhost:5001" -ForegroundColor White
Write-Host "   📱 Mobile Aggregator: http://localhost:5002" -ForegroundColor White
Write-Host "   📚 Swagger UI: http://localhost/swagger" -ForegroundColor Green
Write-Host "   🗄️ SQL Server: localhost:1433" -ForegroundColor White
Write-Host ""FG
Write-Host "🔍 Para ver logs: docker-compose -f docker-compose.production.yml logs -f" -ForegroundColor Yellow
Write-Host "🛑 Para parar: docker-compose -f docker-compose.production.yml down" -ForegroundColor Yellow
Write-Host ""
Write-Host "💡 Este script não usa volumes compartilhados, evitando problemas de permissão" -ForegroundColor Cyan
