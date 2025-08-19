-- Script para criar usuário de teste no ZapFinance
USE ZapFinanceDb;

-- Inserir usuário de teste se não existir
IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE Email = 'admin@zapfinance.com')
BEGIN
    INSERT INTO Usuarios (
        Nome,
        Email,
        Telefone,
        Documento,
        TipoDocumento,
        Ativo,
        DataCriacao,
        DataAtualizacao
    ) VALUES (
        'Administrador ZapFinance',
        'admin@zapfinance.com',
        '11999999999',
        '12345678901',
        0, -- CPF
        1, -- Ativo
        GETUTCDATE(),
        GETUTCDATE()
    );
    
    PRINT 'Usuário admin@zapfinance.com criado com sucesso!';
END
ELSE
BEGIN
    PRINT 'Usuário admin@zapfinance.com já existe.';
END

-- Verificar se o usuário foi criado
SELECT 
    Id,
    Nome,
    Email,
    Telefone,
    Documento,
    TipoDocumento,
    Ativo,
    DataCriacao
FROM Usuarios 
WHERE Email = 'admin@zapfinance.com';
