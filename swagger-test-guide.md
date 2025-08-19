# 🧪 Guia de Teste no Swagger - ZapFinance

## 📍 URLs de Acesso

### Swagger UI
- **Mobile Aggregator**: http://localhost:5002/swagger
- **Core Service**: http://localhost:5001 (gRPC - sem Swagger)

### APIs Diretas
- **Mobile API**: http://localhost:5002
- **Core Service**: http://localhost:5001
- **API Gateway (Nginx)**: http://localhost:80

## 🔐 Endpoints de Autenticação Disponíveis

### 1. **Autenticação de Teste** (Aceita qualquer credencial)
```
POST /api/test-auth/login
```
**Body:**
```json
{
  "email": "qualquer@email.com",
  "password": "qualquersenha"
}
```

### 2. **Autenticação Real** (Valida no banco de dados)
```
POST /api/working-auth/login
```
**Body:**
```json
{
  "email": "admin@zapfinance.com",
  "password": "12345678"
}
```

## 👤 Usuário de Teste no Banco
- **Email**: `admin@zapfinance.com`
- **Senha**: `12345678`
- **Nome**: `Admin ZapFinance`
- **Status**: Ativo

## 🔄 Fluxo de Teste Completo

### Passo 1: Fazer Login
1. Acesse: http://localhost:5002/swagger
2. Use o endpoint `POST /api/working-auth/login`
3. Insira as credenciais do usuário de teste
4. **Copie o token JWT** da resposta

### Passo 2: Testar Endpoints Protegidos
1. Clique no botão **"Authorize"** no Swagger
2. Digite: `Bearer SEU_TOKEN_AQUI`
3. Teste os endpoints protegidos:
   - `GET /api/user` - Listar usuários
   - `POST /api/user` - Criar usuário
   - `PUT /api/user/{id}` - Atualizar usuário
   - `DELETE /api/user/{id}` - Deletar usuário

## 📝 Exemplo de Criação de Usuário

**Endpoint:** `POST /api/user`

**Body:**
```json
{
  "name": "João Silva",
  "email": "joao.silva@zapfinance.com",
  "phone": "11999999999",
  "document": "12345678901",
  "documentType": 0
}
```

**Tipos de Documento:**
- `0` = CPF
- `1` = CNPJ
- `2` = Passaporte

## ✅ Validações Esperadas

### Autenticação
- ✅ Login com credenciais corretas retorna token JWT
- ❌ Login com credenciais incorretas retorna 401
- ❌ Endpoints protegidos sem token retornam 401

### Usuários
- ✅ Criar usuário com dados válidos
- ❌ Criar usuário com email duplicado
- ❌ Criar usuário com dados inválidos
- ✅ Listar usuários (paginado)
- ✅ Atualizar usuário existente
- ✅ Deletar usuário

## 🔧 Troubleshooting

### Se o Swagger não carregar:
```bash
docker-compose restart mobileaggregator
```

### Se a autenticação falhar:
- Verifique se o usuário existe no banco
- Confirme se a senha está correta: `12345678`
- Verifique se o serviço Core está rodando

### Verificar logs:
```bash
docker-compose logs mobileaggregator
docker-compose logs coreservice
```

## 🚀 Sistema Pronto!

O ZapFinance está funcionando com:
- ✅ Autenticação JWT completa
- ✅ Comunicação gRPC entre serviços
- ✅ Banco de dados SQL Server
- ✅ Swagger UI para testes
- ✅ Arquitetura hexagonal implementada

**Divirta-se testando!** 🎉
