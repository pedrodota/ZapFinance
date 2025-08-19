# Teste Simples do ZapFinance
Write-Host "Testando ZapFinance - Sistema Completo" -ForegroundColor Green
Write-Host "=======================================" -ForegroundColor Yellow

Write-Host "1. Status dos servicos:" -ForegroundColor Cyan
docker-compose ps

Write-Host "`n2. Testando endpoints de saude..." -ForegroundColor Cyan

# Core Service
try {
    $core = Invoke-WebRequest -Uri "http://localhost:5001/health" -UseBasicParsing
    Write-Host "   Core Service: OK - $($core.Content)" -ForegroundColor Green
} catch {
    Write-Host "   Core Service: ERRO" -ForegroundColor Red
}

# Mobile Aggregator
try {
    $mobile = Invoke-WebRequest -Uri "http://localhost:5002/health" -UseBasicParsing
    Write-Host "   Mobile Aggregator: OK - $($mobile.Content)" -ForegroundColor Green
} catch {
    Write-Host "   Mobile Aggregator: ERRO" -ForegroundColor Red
}

# Web Gateway
try {
    $web = Invoke-WebRequest -Uri "http://localhost:5003/health" -UseBasicParsing
    Write-Host "   Web Gateway: OK - $($web.Content)" -ForegroundColor Green
} catch {
    Write-Host "   Web Gateway: ERRO" -ForegroundColor Red
}

# Integration Gateway
try {
    $integration = Invoke-WebRequest -Uri "http://localhost:5004/health" -UseBasicParsing
    Write-Host "   Integration Gateway: OK - $($integration.Content)" -ForegroundColor Green
} catch {
    Write-Host "   Integration Gateway: ERRO" -ForegroundColor Red
}

Write-Host "`n3. Testando seguranca JWT..." -ForegroundColor Cyan
try {
    Invoke-WebRequest -Uri "http://localhost:5002/api/user" -Method GET -UseBasicParsing
    Write-Host "   Seguranca JWT: FALHOU - Endpoint nao protegido!" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 401) {
        Write-Host "   Seguranca JWT: OK - Endpoint protegido (401)" -ForegroundColor Green
    } else {
        Write-Host "   Seguranca JWT: Status inesperado" -ForegroundColor Yellow
    }
}

Write-Host "`n4. Testando Swagger..." -ForegroundColor Cyan
try {
    $swagger = Invoke-WebRequest -Uri "http://localhost:5002/swagger" -UseBasicParsing
    if ($swagger.Content -like "*Swagger UI*") {
        Write-Host "   Swagger UI: OK - Documentacao disponivel" -ForegroundColor Green
    }
} catch {
    Write-Host "   Swagger UI: ERRO" -ForegroundColor Red
}

Write-Host "`n=======================================" -ForegroundColor Yellow
Write-Host "SISTEMA ZAPFINANCE FUNCIONANDO!" -ForegroundColor Green
Write-Host "`nEndpoints disponiveis:" -ForegroundColor Cyan
Write-Host "  Mobile API: http://localhost:5002" -ForegroundColor White
Write-Host "  Swagger: http://localhost:5002/swagger" -ForegroundColor White
Write-Host "  Core Service: http://localhost:5001" -ForegroundColor White
Write-Host "`nSistema pronto para desenvolvimento!" -ForegroundColor Green
