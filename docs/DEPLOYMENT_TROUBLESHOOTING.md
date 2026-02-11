# FlowBoard Deployment Troubleshooting Guide

Common issues encountered during Azure deployment and their solutions.

---

## 1. Backend 503 - Application Insights Connection String Format

**Symptom:** Backend App Service returns 503. Docker logs show:

```
System.FormatException: The connection string is not a valid Connection String.
   at Microsoft.ApplicationInsights.Extensibility.Implementation.Endpoints.ConnectionString.ConnectionStringParser.Parse(String connectionString)
```

**Cause:** The `ApplicationInsights__ConnectionString` value was set without the `InstrumentationKey=` prefix. Azure Portal's Application Insights shows the connection string starting with `InstrumentationKey=...`, but users sometimes copy only the GUID.

**Fix:**

```bash
az webapp config appsettings set \
  --resource-group rg-flowboard \
  --name flowboard-api-<suffix> \
  --settings "ApplicationInsights__ConnectionString=InstrumentationKey=<guid>;IngestionEndpoint=https://..."
```

The full connection string should look like:
```
InstrumentationKey=xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx;IngestionEndpoint=https://westus3-1.in.applicationinsights.azure.com/;LiveEndpoint=https://westus3.livediagnostics.monitor.azure.com/;ApplicationId=...
```

---

## 2. Frontend 404 on Azure Static Web Apps

**Symptom:** Accessing the SWA URL (e.g., `https://xxx.azurestaticapps.net`) returns a 404 page.

**Cause:** `staticwebapp.config.json` was placed in `flowboard-web/` root directory, but Angular only copies files from the `public/` directory to the build output. Without this config file, SWA doesn't know how to handle SPA routing.

**Fix:** Move the config to the `public/` directory:

```bash
mv flowboard-web/staticwebapp.config.json flowboard-web/public/staticwebapp.config.json
```

The `angular.json` assets config only includes files from `public/`:
```json
"assets": [
  {
    "glob": "**/*",
    "input": "public"
  }
]
```

---

## 3. Frontend Calling localhost Instead of Azure API

**Symptom:** Browser console shows errors like:

```
POST http://localhost:5254/api/auth/login net::ERR_CONNECTION_REFUSED
```

**Cause:** `angular.json` production configuration was missing `fileReplacements`, so the production build used `environment.ts` (with `localhost` URLs) instead of `environment.prod.ts` (with Azure URLs).

**Fix:** Add `fileReplacements` to the production config in `angular.json`:

```json
"configurations": {
  "production": {
    "fileReplacements": [
      {
        "replace": "src/environments/environment.ts",
        "with": "src/environments/environment.prod.ts"
      }
    ],
    "budgets": [...]
  }
}
```

---

## 4. pnpm Version Conflict in GitHub Actions

**Symptom:** GitHub Actions frontend deployment fails with:

```
ERR_PNPM_LOCKFILE_CONFIG_MISMATCH: Cannot proceed with the frozen lockfile...
```

**Cause:** The `pnpm/action-setup@v4` action installed a different pnpm version than what was used to generate the lockfile.

**Fix:** Either let `pnpm/action-setup@v4` auto-detect from `packageManager` field in `package.json`, or pin the version explicitly:

```yaml
- name: Install pnpm
  uses: pnpm/action-setup@v4
  # v4 auto-reads the packageManager field from package.json
```

Make sure `package.json` has the correct `packageManager` field:
```json
{
  "packageManager": "pnpm@9.x.x"
}
```

---

## 5. Duplicate SWA Workflow Files

**Symptom:** Two GitHub Actions workflows trigger for the same SWA deployment, causing conflicts or double deploys.

**Cause:** When creating a Static Web App via `az staticwebapp create --login-with-github`, Azure automatically creates a workflow file (e.g., `azure-static-web-apps-xxx.yml`). If you also have a custom `deploy-frontend.yml`, both will trigger.

**Fix:** Delete the auto-generated workflow file and keep only your custom one:

```bash
git rm .github/workflows/azure-static-web-apps-xxx.yml
```

---

## 6. SWA Deploy Path Incorrect

**Symptom:** SWA deployment succeeds in GitHub Actions but the site shows a blank page or default page.

**Cause:** The `app_location` in the SWA deploy action pointed to the source directory instead of the build output.

**Fix:** When using `skip_app_build: true` (we build ourselves), `app_location` should point to the **built output**:

```yaml
- name: Deploy to Azure Static Web Apps
  uses: Azure/static-web-apps-deploy@v1
  with:
    azure_static_web_apps_api_token: ${{ secrets.AZURE_STATIC_WEB_APPS_API_TOKEN }}
    action: 'upload'
    app_location: 'flowboard-web/dist/flowboard-web/browser'  # Built output, NOT source
    skip_app_build: true
```

---

## 7. CORS Errors from Frontend

**Symptom:** Browser console shows CORS errors when the frontend tries to call the backend API:

```
Access to XMLHttpRequest at 'https://flowboard-api-xxx.azurewebsites.net/api/...'
from origin 'https://xxx.azurestaticapps.net' has been blocked by CORS policy
```

**Cause:** The backend's CORS configuration doesn't include the SWA domain.

**Fix:**

```bash
az webapp config appsettings set \
  --resource-group rg-flowboard \
  --name flowboard-api-<suffix> \
  --settings "Cors__AllowedOrigins__0=https://xxx.azurestaticapps.net"

# Restart the app service
az webapp restart \
  --resource-group rg-flowboard \
  --name flowboard-api-<suffix>
```

**Important:** Do NOT include a trailing slash in the origin URL.

---

## Quick Diagnostic Commands

```bash
# Check backend health
curl https://flowboard-api-<suffix>.azurewebsites.net/health/ready

# View backend logs
az webapp log tail \
  --resource-group rg-flowboard \
  --name flowboard-api-<suffix>

# Check App Service settings
az webapp config appsettings list \
  --resource-group rg-flowboard \
  --name flowboard-api-<suffix> \
  --output table

# Restart backend
az webapp restart \
  --resource-group rg-flowboard \
  --name flowboard-api-<suffix>
```
