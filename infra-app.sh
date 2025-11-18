#!/usr/bin/env bash
set -euo pipefail

# ================================
# CONFIGURAÇÕES PRINCIPAIS
# ================================
DB_NAME="Skillbridge"  # Nome do banco de dados
DB_HOST=4.157.142.163     # Defina o IP ou hostname do banco de dados
DB_PORT=3306
DB_USER="root"
DB_PASSWORD="senha123"  # Senha do MySQL
MYSQL_CLIENT_CMD="mysql"
MYSQL_CMD="${MYSQL_CLIENT_CMD} -h $DB_HOST -P $DB_PORT -u $DB_USER -p$DB_PASSWORD"

RESOURCE_GROUP="rg_${DB_NAME}_webapp"
APP_SERVICE_PLAN="plan-${DB_NAME}"
WEB_APP_NAME="${DB_NAME}-webapp"
LOCATION="eastus"
RUNTIME="DOTNET|8.0"
#a
# ================================
# 1. Validação da Presença do Cliente MySQL
# ================================
echo "🔍 Verificando a presença do cliente MySQL..."
if ! command -v mysql &> /dev/null; then
    echo "❌ Cliente MySQL não encontrado. Instale o MySQL Client antes de continuar."
    exit 1
fi
echo "✅ Cliente MySQL encontrado."

# ================================
# 2. Criação do Banco de Dados
# ================================
echo "🔨 Conectando ao MySQL e criando o banco de dados se não existir..."
$MYSQL_CMD -e "CREATE DATABASE IF NOT EXISTS $DB_NAME;"
echo "✅ Banco de dados '$DB_NAME' criado ou já existe."

# ================================
# 3. Criação das Tabelas via Migrations .NET
# ================================
echo "🔄 Executando as migrations do .NET para criar as tabelas..."
dotnet ef database update --project Skillbridge-dotnet.csproj --environment Development --verbose
echo "✅ Tabelas criadas via migrations do .NET."

# ================================
# 4. Criação de Infraestrutura no Azure (WebApp)
# ================================
echo "🔧 Criando infraestrutura no Azure..."

# Verificar se o Resource Group existe
if ! az group exists --name "$RESOURCE_GROUP"; then
  echo "📁 Criando o Resource Group '$RESOURCE_GROUP' no Azure..."
  az group create --name "$RESOURCE_GROUP" --location "$LOCATION" > /dev/null
  echo "✅ Resource Group '$RESOURCE_GROUP' criado."
else
  echo "✅ Resource Group '$RESOURCE_GROUP' já existe."
fi

# Verificar se o App Service Plan existe
if ! az appservice plan show --name "$APP_SERVICE_PLAN" --resource-group "$RESOURCE_GROUP" &>/dev/null; then
  echo "⚙️ Criando o App Service Plan '$APP_SERVICE_PLAN'..."
  az appservice plan create --name "$APP_SERVICE_PLAN" --resource-group "$RESOURCE_GROUP" --sku B1 --is-linux --location "$LOCATION" > /dev/null
  echo "✅ App Service Plan '$APP_SERVICE_PLAN' criado."
else
  echo "✅ App Service Plan '$APP_SERVICE_PLAN' já existe."
fi

# Verificar se o Web App existe
if ! az webapp show --name "$WEB_APP_NAME" --resource-group "$RESOURCE_GROUP" &>/dev/null; then
  echo "🌐 Criando Web App '$WEB_APP_NAME'..."
  az webapp create --name "$WEB_APP_NAME" --resource-group "$RESOURCE_GROUP" --plan "$APP_SERVICE_PLAN" --runtime "$RUNTIME" > /dev/null
  echo "✅ Web App '$WEB_APP_NAME' criado."
else
  echo "✅ Web App '$WEB_APP_NAME' já existe."
fi

# ================================
# 5. Configuração de Logs do Serviço de Aplicativo
# ================================
echo "📈 Verificando configuração de logs do Web App..."

LOGGING_CONFIGURED=$(az webapp log show --name "$WEB_APP_NAME" --resource-group "$RESOURCE_GROUP" --query "applicationLogs.fileSystem.enabled" -o tsv)

if [[ "$LOGGING_CONFIGURED" != "true" ]]; then
  echo "🛠️ Habilitando logs para o Web App..."
  az webapp log config --name "$WEB_APP_NAME" --resource-group "$RESOURCE_GROUP" --application-logs-file-system --level Information > /dev/null
  echo "✅ Logs habilitados no Web App."
else
  echo "✅ Logs já estão configurados no Web App."
fi

# ================================
# 6. Conclusão
# ================================
echo "=========================================="
echo "✅ Processo Concluído!"
echo "🔹 Banco de dados '$DB_NAME' criado ou já existe."
echo "🔹 Tabelas criadas com migrations do .NET."
echo "🔹 Infraestrutura do Web App criada no Azure."
echo "🔹 Logs configurados para o Web App."
echo "=========================================="
