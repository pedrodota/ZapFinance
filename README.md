# 🏦 ZapFinance - API com Arquitetura Hexagonal

Sistema de gestão financeira com arquitetura de microserviços.

## 🚀 Inicialização Rápida

### ⚠️ Problema com Volumes Compartilhados?
Se você receber erro sobre volumes compartilhados não habilitados:

**Solução A: Habilitar no Docker Desktop**
1. Abra Docker Desktop → Configurações (⚙️)
2. Vá em "Resources" → "File Sharing"
3. Adicione a unidade C: (ou onde está o projeto)
4. Clique "Apply & Restart"

**Solução B: Usar versão sem volumes compartilhados**
```powershell
.\start-zapfinance-no-volumes.ps1
```

### Opções de Inicialização:

#### Opção 1: Visual Studio (Recomendado)
1. Abra o arquivo `ZapFinance.sln` no Visual Studio
2. Defina `docker-compose` como projeto de inicialização
3. Pressione F5 ou clique em "Iniciar"

#### Opção 2: Script PowerShell (Com volumes)
```powershell
.\start-zapfinance.ps1
```

#### Opção 3: Script PowerShell (Sem volumes)
```powershell
.\start-zapfinance-no-volumes.ps1
```

#### Opção 4: Docker Compose Manual
```bash
# Versão com volumes compartilhados
docker-compose up --build -d

# Versão sem volumes compartilhados
docker-compose -f docker-compose.production.yml up --build -d
```

## 🏗️ Arquitetura

```
ZapFinance/
├── GATEWAYS/                    # Gateways de entrada
│   ├── MobileAggregator/        # Gateway para aplicações mobile
│   ├── WebGateway/              # Gateway para aplicações web
│   └── IntegrationGateway/      # Gateway para integrações
├── LIBRARIES/                   # Bibliotecas compartilhadas
│   └── ProtoServer/             # Definições gRPC (protobuf)
└── SERVICES/                    # Serviços de domínio
    └── Core.Service/            # Serviço principal (hexagonal)
```

## 🚀 Como Executar

### Pré-requisitos
- Docker Desktop
- .NET 9.0 SDK (para desenvolvimento)

### Execução Rápida
```bash
# Executar script PowerShell
.\start-zapfinance.ps1

# OU executar manualmente
docker-compose up --build
```

### Endpoints Disponíveis
- **🌐 API Gateway (Nginx)**: `http://localhost:80` - Gateway principal com balanceamento
- **🔗 Core Service**: `http://localhost:5001` - Serviço gRPC + REST
- **📱 Mobile Aggregator**: `http://localhost:5002` - Gateway para mobile
- **🌍 Web Gateway**: `http://localhost:5003` - Gateway para web
- **🔗 Integration Gateway**: `http://localhost:5004` - Gateway para integrações
- **📚 Swagger UI**: `http://localhost/swagger` - Documentação da API
- **🗄️ SQL Server**: `localhost:1433` - Banco de dados

## 📡 APIs Disponíveis

### Mobile Aggregator

#### Autenticação
- `POST /api/auth/login` - Login de usuário
- `POST /api/auth/refresh-token` - Renovar token
- `POST /api/auth/change-password` - Alterar senha
- `POST /api/auth/forgot-password` - Recuperar senha

#### Usuários
- `POST /api/user` - Criar usuário
- `GET /api/user/{id}` - Obter usuário
- `PUT /api/user/{id}` - Atualizar usuário
- `DELETE /api/user/{id}` - Deletar usuário
- `GET /api/user` - Listar usuários

## 🔧 Desenvolvimento

### Estrutura do Core Service (Hexagonal)
```
Core.Service/
├── Application/
│   └── Domain/
│       └── Models/              # Entidades de domínio
├── Infrastructure/
│   ├── Adapter/                 # Adaptadores (gRPC)
│   ├── Data/
│   │   ├── DbContext/          # Contexto EF
│   │   └── Repositories/       # Repositórios
│   └── UnityOfWork/            # Unit of Work
```

### Comandos Úteis

```bash
# Ver logs dos serviços
docker-compose logs -f

# Parar serviços
docker-compose down

# Rebuild completo
docker-compose down -v
docker-compose up --build

# Executar migrations manualmente
cd SERVICES/Core.Service/Core.Service
dotnet ef database update
```

## 🗄️ Banco de Dados

- **SGBD**: SQL Server 2022
- **Database**: ZapFinanceDb
- **Usuário**: sa
- **Senha**: YourStrong@Passw0rd

### Tabelas
- `Usuarios` - Dados dos usuários do sistema

## 🔐 Autenticação

O sistema usa JWT Bearer tokens para autenticação:
- **Issuer**: ZapFinance.MobileAggregator
- **Audience**: ZapFinance.Mobile
- **Algoritmo**: HS256

## 🐳 Docker

### Serviços
- **sqlserver**: SQL Server 2022 Express
- **coreservice**: API gRPC do domínio
- **mobileaggregator**: Gateway REST para mobile

### Rede
Todos os serviços estão na rede `zapfinance-network` para comunicação interna.

## 📝 Logs e Monitoramento

- Health checks configurados em todos os serviços
- Logs estruturados com diferentes níveis
- Swagger UI disponível em desenvolvimento

## 🛠️ Tecnologias

- **.NET 9.0** - Framework principal
- **gRPC** - Comunicação entre serviços
- **Entity Framework Core** - ORM
- **SQL Server** - Banco de dados
- **Docker** - Containerização
- **JWT** - Autenticação
- **Swagger** - Documentação API

## 📋 Status do Projeto

✅ **Implementado:**
- Estrutura hexagonal no Core Service
- Comunicação gRPC entre serviços
- Gateway Mobile Aggregator
- Autenticação JWT
- Containerização Docker
- Documentação Swagger

🔄 **Em Desenvolvimento:**
- Web Gateway
- Integration Gateway
- Testes automatizados
- CI/CD Pipeline
