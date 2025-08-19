-- Script para inicializar o banco de dados ZapFinance
USE master;
GO

-- Criar o banco se não existir
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'ZapFinanceDb')
BEGIN
    CREATE DATABASE ZapFinanceDb;
END
GO

USE ZapFinanceDb;
GO

-- Verificar se as tabelas já existem
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Usuarios' AND xtype='U')
BEGIN
    PRINT 'Banco de dados ZapFinanceDb criado e pronto para migrations.';
END
ELSE
BEGIN
    PRINT 'Banco de dados ZapFinanceDb já existe com tabelas.';
END
GO
