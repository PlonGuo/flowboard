# FlowBoard - Azure Deployment Guide

> Step-by-step guide to deploy FlowBoard to Microsoft Azure.

---

## Architecture Overview

```
                    Azure Front Door (optional)
                           |
              +------------+------------+
              |                         |
     Azure Static Web Apps       Azure App Service
       (Angular Frontend)        (.NET API + SignalR)
                                        |
                                +-------+-------+
                                |               |
                     Azure PostgreSQL     Azure Key Vault
                     (Flexible Server)   (Secrets Management)
                                |
                     Azure Application Insights
                         (Monitoring)
```

### Azure Services Required

| Service | Purpose | SKU (Starting) | Est. Cost/Month |
|---------|---------|-----------------|-----------------|
| App Service | Backend API + SignalR | B1 | ~$13 |
| Azure Database for PostgreSQL | Database | Burstable B1ms | ~$15 |
| Static Web Apps | Frontend hosting | Free | $0 |
| Key Vault | Secrets management | Standard | ~$1 |
| Application Insights | Monitoring | Free tier (5GB) | ~$1 |
| **Total** | | | **~$30/month** |

---

## Prerequisites

- [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli) installed
- [.NET 10 SDK](https://dotnet.microsoft.com/download) installed
- [Node.js 20+](https://nodejs.org/) and [pnpm](https://pnpm.io/) installed
- An Azure account (free tier or student credits work)
- GitHub repository access

---

## Step 1: Azure Account Setup

```bash
# Login to Azure
az login

# (Optional) Set the subscription if you have multiple
az account list --output table
az account set --subscription "<subscription-id>"
```

---

## Step 2: Create Azure Resources

### 2.1 Resource Group

```bash
az group create \
  --name rg-flowboard \
  --location westus3
```

### 2.2 Azure Database for PostgreSQL

```bash
# Create PostgreSQL Flexible Server
az postgres flexible-server create \
  --resource-group rg-flowboard \
  --name flowboard-db-<unique-suffix> \
  --location westus3 \
  --admin-user flowadmin \
  --admin-password "<STRONG_PASSWORD>" \
  --sku-name Standard_B1ms \
  --tier Burstable \
  --storage-size 32 \
  --version 16

# Create the database
az postgres flexible-server db create \
  --resource-group rg-flowboard \
  --server-name flowboard-db-<unique-suffix> \
  --database-name flowboard

# Allow Azure services to access (required for App Service connection)
az postgres flexible-server firewall-rule create \
  --resource-group rg-flowboard \
  --name flowboard-db-<unique-suffix> \
  --rule-name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0
```

> **Note:** Replace `<unique-suffix>` with a unique identifier (e.g., your initials + date). Replace `<STRONG_PASSWORD>` with a strong password.

### 2.3 Azure Key Vault

```bash
# Create Key Vault
az keyvault create \
  --name flowboard-kv-<unique-suffix> \
  --resource-group rg-flowboard \
  --location westus3

# Grant yourself permission to manage secrets (Key Vault uses RBAC by default)
az role assignment create \
  --role "Key Vault Secrets Officer" \
  --assignee $(az ad signed-in-user show --query id -o tsv) \
  --scope $(az keyvault show --name flowboard-kv-<unique-suffix> --query id -o tsv)
# Wait 1-2 minutes for the role assignment to propagate

# Store the JWT secret key (generate a strong 64+ character key)
az keyvault secret set \
  --vault-name flowboard-kv-<unique-suffix> \
  --name "Jwt--Key" \
  --value "<YOUR_STRONG_JWT_KEY_AT_LEAST_32_CHARS>"

# Store the database connection string
az keyvault secret set \
  --vault-name flowboard-kv-<unique-suffix> \
  --name "ConnectionStrings--DefaultConnection" \
  --value "Host=flowboard-db-<unique-suffix>.postgres.database.azure.com;Database=flowboard;Username=flowadmin;Password=<STRONG_PASSWORD>;SSL Mode=Require;Trust Server Certificate=true"
```

> **Important:** Azure Key Vault uses `--` as separator for nested configuration keys (e.g., `Jwt--Key` maps to `Jwt:Key` in .NET configuration).

### 2.4 App Service (Backend API)

```bash
# Create App Service Plan (Linux)
az appservice plan create \
  --name plan-flowboard \
  --resource-group rg-flowboard \
  --location westus3 \
  --sku B1 \
  --is-linux

# Create Web App
az webapp create \
  --resource-group rg-flowboard \
  --plan plan-flowboard \
  --name flowboard-api-<unique-suffix> \
  --runtime "DOTNETCORE:10.0"

# Enable WebSockets (required for SignalR)
az webapp config set \
  --resource-group rg-flowboard \
  --name flowboard-api-<unique-suffix> \
  --web-sockets-enabled true

# Enable Managed Identity
az webapp identity assign \
  --resource-group rg-flowboard \
  --name flowboard-api-<unique-suffix>
```

### 2.5 Grant Key Vault Access to App Service

```bash
# Get the App Service's managed identity principal ID
PRINCIPAL_ID=$(az webapp identity show \
  --resource-group rg-flowboard \
  --name flowboard-api-<unique-suffix> \
  --query principalId -o tsv)

# Grant Key Vault read access via RBAC
az role assignment create \
  --role "Key Vault Secrets User" \
  --assignee $PRINCIPAL_ID \
  --scope $(az keyvault show --name flowboard-kv-<unique-suffix> --query id -o tsv)
```

### 2.6 Configure App Service Settings

```bash
# Set environment variables
az webapp config appsettings set \
  --resource-group rg-flowboard \
  --name flowboard-api-<unique-suffix> \
  --settings \
    ASPNETCORE_ENVIRONMENT=Production \
    KeyVault__Name=flowboard-kv-<unique-suffix> \
    Cors__AllowedOrigins__0=https://<your-static-web-app-url>
```

> **Note:** You'll update `Cors__AllowedOrigins__0` after creating the Static Web App in the next step.

### 2.7 Static Web Apps (Frontend)

```bash
# Create Static Web App (linked to GitHub for auto-deploy)
# Note: Static Web Apps only supports limited regions (westus2, centralus, eastus2, westeurope, eastasia)
az staticwebapp create \
  --name flowboard-web-<unique-suffix> \
  --resource-group rg-flowboard \
  --location eastus2 \
  --source https://github.com/<your-username>/flowboard \
  --branch main \
  --app-location "/flowboard-web" \
  --output-location "dist/flowboard-web/browser" \
  --login-with-github
```

> After creation, note the URL (e.g., `https://xxx.azurestaticapps.net`), then update the CORS setting:

```bash
az webapp config appsettings set \
  --resource-group rg-flowboard \
  --name flowboard-api-<unique-suffix> \
  --settings \
    Cors__AllowedOrigins__0=https://<your-static-web-app>.azurestaticapps.net
```

### 2.8 Application Insights (Optional but Recommended)

```bash
# Create Application Insights
az monitor app-insights component create \
  --app flowboard-insights \
  --location westus3 \
  --resource-group rg-flowboard \
  --application-type web

# Get the connection string
az monitor app-insights component show \
  --app flowboard-insights \
  --resource-group rg-flowboard \
  --query connectionString -o tsv

# Add to App Service
az webapp config appsettings set \
  --resource-group rg-flowboard \
  --name flowboard-api-<unique-suffix> \
  --settings \
    ApplicationInsights__ConnectionString="<connection-string-from-above>"
```

---

## Step 3: Configure Frontend API URL

If the frontend and backend are on **different domains** (Static Web Apps + App Service), update the production environment file:

**`flowboard-web/src/environments/environment.prod.ts`:**

```typescript
export const environment = {
  production: true,
  apiUrl: 'https://flowboard-api-<unique-suffix>.azurewebsites.net/api',
  signalRUrl: 'https://flowboard-api-<unique-suffix>.azurewebsites.net/hubs',
};
```

> If deploying frontend and backend together on the same App Service, keep the relative paths (`/api`, `/hubs`).

---

## Step 4: Database Migration

Run EF Core migrations against the Azure database:

```bash
# From the project root
cd src/FlowBoard.API

# Set the connection string for migration
export ConnectionStrings__DefaultConnection="Host=flowboard-db-<unique-suffix>.postgres.database.azure.com;Database=flowboard;Username=flowadmin;Password=<STRONG_PASSWORD>;SSL Mode=Require;Trust Server Certificate=true"

# Apply migrations
dotnet ef database update \
  --project ../FlowBoard.Infrastructure/FlowBoard.Infrastructure.csproj
```

---

## Step 5: Deploy Backend

### Option A: Deploy via GitHub Actions (Recommended)

1. **Download Publish Profile from Azure Portal:**
   - Open [Azure Portal](https://portal.azure.com) and navigate to your App Service
   - In the left sidebar, click **Overview**
   - Click the **"Download publish profile"** button at the top toolbar (or go to **Deployment Center** > **Manage publish profile**)
   - A `.PublishSettings` file will download - open it in a text editor and **copy all of its contents**

2. **Add Publish Profile to GitHub Secrets:**
   - Go to your GitHub repository: `https://github.com/<your-username>/flowboard`
   - Click **Settings** tab > **Secrets and variables** > **Actions**
   - Click **"New repository secret"**
   - Name: `AZURE_WEBAPP_PUBLISH_PROFILE`
   - Value: paste the **entire content** of the `.PublishSettings` file
   - Click **"Add secret"**

3. Update `AZURE_WEBAPP_NAME` in `.github/workflows/deploy-backend.yml` to your App Service name
4. Push to `main` branch to trigger deployment

### Option B: Deploy via Azure CLI

```bash
# Build the project
cd src/FlowBoard.API
dotnet publish -c Release -o ./publish

# Deploy
az webapp deploy \
  --resource-group rg-flowboard \
  --name flowboard-api-<unique-suffix> \
  --src-path ./publish \
  --type zip
```

### Option C: Deploy via Docker

```bash
# Build Docker image (from src/ directory)
cd src
docker build -f FlowBoard.API/Dockerfile -t flowboard-api .

# Tag and push to Azure Container Registry (if using ACR)
# az acr create --name flowboardacr --resource-group rg-flowboard --sku Basic
# az acr login --name flowboardacr
# docker tag flowboard-api flowboardacr.azurecr.io/flowboard-api:latest
# docker push flowboardacr.azurecr.io/flowboard-api:latest
```

---

## Step 6: Deploy Frontend

### Option A: Auto-deploy via Static Web Apps (Recommended)

If you linked the Static Web App to GitHub in Step 2.7, it will auto-deploy on push to `main`.

### Option B: Manual deploy via GitHub Actions

1. **Get Static Web App deployment token from Azure Portal:**
   - Open [Azure Portal](https://portal.azure.com) and navigate to your Static Web App
   - In the left sidebar, click **Overview**
   - Click **"Manage deployment token"** at the top toolbar
   - Copy the token value

2. **Add token to GitHub Secrets:**
   - Go to your GitHub repository Settings > **Secrets and variables** > **Actions**
   - Click **"New repository secret"**
   - Name: `AZURE_STATIC_WEB_APPS_API_TOKEN`
   - Value: paste the deployment token
   - Click **"Add secret"**

3. Push to `main` to trigger `.github/workflows/deploy-frontend.yml`

---

## Step 7: Verify Deployment

```bash
# Check backend health
curl https://flowboard-api-<unique-suffix>.azurewebsites.net/health

# Check readiness (includes database check)
curl https://flowboard-api-<unique-suffix>.azurewebsites.net/health/ready

# Check frontend
open https://<your-static-web-app>.azurestaticapps.net
```

---

## Post-Deployment Checklist

- [ ] Backend `/health` returns 200 OK
- [ ] Backend `/health/ready` returns 200 OK (database connected)
- [ ] Frontend loads successfully
- [ ] User registration works
- [ ] User login works and returns JWT token
- [ ] Board creation works
- [ ] SignalR real-time updates work (open two browser tabs)
- [ ] Canvas/whiteboard collaboration works
- [ ] CORS is correctly configured (no browser errors)
- [ ] Application Insights shows telemetry data

---

## Troubleshooting

### Common Issues

**1. CORS Errors**
- Verify `Cors__AllowedOrigins__0` in App Service settings matches your frontend URL exactly
- Check that the URL includes `https://` and has no trailing slash

**2. SignalR Connection Fails**
- Ensure WebSockets are enabled: `az webapp config set --web-sockets-enabled true`
- Check that `signalRUrl` in `environment.prod.ts` points to the correct backend URL

**3. Database Connection Fails**
- Check the firewall rule allows Azure services
- Verify the connection string in Key Vault is correct
- Ensure SSL Mode is set to `Require`

**4. Key Vault Access Denied**
- Verify Managed Identity is enabled on the App Service
- Check Key Vault access policy includes the App Service's principal ID

**5. 500 Internal Server Error**
- Check App Service logs: `az webapp log tail --name flowboard-api-<unique-suffix> --resource-group rg-flowboard`
- Check Application Insights for detailed error traces

### Useful Commands

```bash
# View App Service logs
az webapp log tail \
  --name flowboard-api-<unique-suffix> \
  --resource-group rg-flowboard

# SSH into App Service
az webapp ssh \
  --name flowboard-api-<unique-suffix> \
  --resource-group rg-flowboard

# Restart App Service
az webapp restart \
  --name flowboard-api-<unique-suffix> \
  --resource-group rg-flowboard

# View App Service configuration
az webapp config appsettings list \
  --name flowboard-api-<unique-suffix> \
  --resource-group rg-flowboard \
  --output table
```

---

## Scaling Considerations

### When to Scale

| Metric | Action |
|--------|--------|
| CPU > 80% sustained | Scale up App Service SKU |
| Memory > 80% sustained | Scale up App Service SKU |
| SignalR connections > 100 | Add Azure SignalR Service |
| Global users | Add Azure Front Door + CDN |
| Database connections > 50 | Scale up PostgreSQL SKU |

### Azure SignalR Service (for scale-out)

If concurrent SignalR connections exceed ~100, add Azure SignalR Service:

```bash
az signalr create \
  --name flowboard-signalr-<unique-suffix> \
  --resource-group rg-flowboard \
  --sku Free_F1 \
  --service-mode Default

# Get connection string
az signalr key list \
  --name flowboard-signalr-<unique-suffix> \
  --resource-group rg-flowboard
```

Then add the `Azure.SignalR` NuGet package and update `Program.cs`:

```csharp
builder.Services.AddSignalR().AddAzureSignalR();
```

---

## Cost Optimization Tips

1. **Use Free/B1 tiers** for development and low-traffic production
2. **Static Web Apps Free tier** includes custom domains and SSL
3. **Stop App Service** when not in use during development
4. **Use Azure Student Credits** ($100/year) if eligible
5. **Set up budget alerts** in Azure Cost Management

---

## Appendix: Azure Portal (Web UI) Operations Guide

While all resource creation can be done via CLI, certain management tasks are easier through the [Azure Portal](https://portal.azure.com). Below is a guide for common Portal operations.

### A.1 Download App Service Publish Profile

1. Log in to [portal.azure.com](https://portal.azure.com)
2. Search for your App Service name (e.g., `flowboard-api-xxx`) in the top search bar
3. Click **Overview** in the left sidebar
4. Click **"Download publish profile"** in the top toolbar
5. Save the `.PublishSettings` file - this is used for GitHub Actions deployment

### A.2 Get Static Web App Deployment Token

1. Search for your Static Web App in the Portal search bar
2. Click **Overview** in the left sidebar
3. Click **"Manage deployment token"** in the top toolbar
4. Click the copy icon to copy the token
5. Use this token as the `AZURE_STATIC_WEB_APPS_API_TOKEN` GitHub secret

### A.3 View Application Insights Monitoring

1. Search for `flowboard-insights` in the Portal search bar
2. **Overview** page shows key metrics: server response time, requests, failed requests
3. Click **Live Metrics** in the left sidebar for real-time monitoring
4. Click **Failures** to investigate errors (see stack traces, exception details)
5. Click **Performance** to find slow API endpoints
6. Click **Logs** to run custom KQL queries, e.g.:
   ```kusto
   requests
   | where resultCode >= 500
   | order by timestamp desc
   | take 20
   ```

### A.4 View App Service Logs

1. Navigate to your App Service in the Portal
2. In the left sidebar, under **Monitoring**, click **Log stream**
3. This shows real-time stdout/stderr logs from your application
4. For historical logs: click **App Service logs** > enable **Application Logging (Filesystem)** > set level to **Information**
5. Then go to **Advanced Tools (Kudu)** > **Log stream** for more detailed output

### A.5 Configure Custom Domain

1. Navigate to your App Service or Static Web App
2. In the left sidebar, click **Custom domains**
3. Click **"Add custom domain"**
4. Enter your domain name (e.g., `api.flowboard.com`)
5. Follow the DNS validation steps (add CNAME or TXT records at your domain registrar)
6. After validation, Azure will auto-provision a free SSL certificate

### A.6 Manage Key Vault Secrets

1. Search for your Key Vault in the Portal search bar
2. Click **Secrets** in the left sidebar
3. Click a secret name to view its versions
4. Click **"+ Generate/Import"** to add a new secret
5. To update: click the secret > **New Version** > enter new value

### A.7 Scale Up / Scale Out App Service

1. Navigate to your App Service
2. **Scale up (vertical)**: left sidebar > **Scale up (App Service plan)** > select a higher SKU tier
3. **Scale out (horizontal)**: left sidebar > **Scale out (App Service plan)** > set instance count or configure auto-scaling rules based on CPU/memory metrics

### A.8 Database Management

1. Search for your PostgreSQL server in the Portal
2. Click **Databases** to see all databases on the server
3. Click **Connection security** to manage firewall rules
4. Click **Server parameters** to tune PostgreSQL configuration
5. Click **Backups** to configure backup retention (default: 7 days)

---

## Security Checklist

- [ ] All secrets stored in Azure Key Vault (never in code)
- [ ] Managed Identity used for service-to-service auth
- [ ] HTTPS enforced on all endpoints
- [ ] CORS restricted to specific origins
- [ ] Database firewall rules properly configured
- [ ] Application Insights enabled for monitoring
- [ ] Regular dependency updates for security patches
