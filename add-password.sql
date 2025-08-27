-- Adicionar coluna de senha na tabela Usuarios
USE ZapFinanceDb;

-- Adicionar coluna Senha se não existir
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Usuarios' AND COLUMN_NAME = 'Senha')
BEGIN
    ALTER TABLE Usuarios ADD Senha NVARCHAR(255) NULL;
    PRINT 'Coluna Senha adicionada com sucesso!';
END

-- Inserir usuário de teste com senha
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
        DataAtualizacao,
        Senha
    ) VALUES (
        'Admin ZapFinance',
        'admin@zapfinance.com',
        '11999999999',
        '12345678901',
        0,
        1,
        GETUTCDATE(),
        GETUTCDATE(),
        '123456'
    );
    PRINT 'Usuario admin@zapfinance.com criado com senha 123456';
END
ELSE
BEGIN
    -- Atualizar senha se usuário já existe
    UPDATE Usuarios SET Senha = '123456' WHERE Email = 'admin@zapfinance.com';
    PRINT 'Senha atualizada para usuario admin@zapfinance.com';
END

-- Verificar usuário criado
SELECT Id, Nome, Email, Senha, Ativo FROM Usuarios WHERE Email = 'admin@zapfinance.com';
