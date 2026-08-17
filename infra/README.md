# Infrastructure

Subscription Manager is deployed to Microsoft Azure using Terraform.

## Azure resources

The infrastructure consists of the following Azure resources:

```text id="s39c82"
Resource Group
│
├── Linux App Service Plan ───────────────► Hosts the API and Web applications
│   ├── API App Service ──────────────────► Hosts the ASP.NET Core API
│   └── Web App Service ──────────────────► Hosts the Blazor web application
│
├── Azure SQL Server ─────────────────────► Hosts the application database
│   └── Azure SQL Database ───────────────► Stores application data
│
├── Azure Key Vault ──────────────────────► Stores application secrets
├── Azure Communication Services Email ───► Sends application emails
├── Application Insights ─────────────────► Provides application monitoring
└── Log Analytics Workspace ──────────────► Stores application telemetry
```

**Environment:** Production · **Region:** West Europe

## Terraform

Terraform manages the Azure infrastructure using a bootstrap configuration and remote state.

```text id="9z9c93"
bootstrap
   │
   └──► Resource Group + Storage Account + Blob Container
                              │
                              ▼
                    Main Terraform state
                              │
                              ▼
                       Azure resources
```

## Configuration

Application settings are configured through Azure App Service.

### Runtime configuration

Production secrets are stored in Azure Key Vault. Non-sensitive settings are configured through Terraform.

```text id="rrb7m7"
GitHub Environment Secrets ──► Deploy workflow ──► Azure Key Vault      ──► API
Terraform configuration    ──────────────────────► App Service settings ──► API
```

### Deployment configuration

GitHub Environment Variables are used by GitHub Actions for Azure authentication and SQL identity.

```text id="ad5vcw"
GitHub Environment Variables ──► GitHub Actions ──► Azure authentication and SQL identity
```
