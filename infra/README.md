# Infrastructure

Subscription Manager is deployed to Microsoft Azure using Terraform.

## Azure resources

| Resource                           | Purpose                                      |
|------------------------------------|----------------------------------------------|
| Resource Group                     | Groups the application resources             |
| Linux App Service Plan             | Hosts the API and Web applications           |
| API App Service                    | Hosts the ASP.NET Core API                   |
| Web App Service                    | Hosts the Blazor web application             |
| Azure SQL Server                   | Hosts the application database               |
| Azure SQL Database                 | Stores application data                      |
| Log Analytics Workspace            | Stores application telemetry                 |
| Application Insights               | Provides application monitoring              |
| Azure Communication Services Email | Sends application emails                     |

- Environment: Production
- Region: West Europe

## Terraform

Terraform is used to define and manage the Azure infrastructure.

| Configuration             | Value                |
|---------------------------|----------------------|
| Main infrastructure state | Azure Storage        |
| Authentication            | Microsoft Entra ID   |
| Shared Key                | Disabled             |
| Bootstrap state           | Local                |

The `bootstrap` configuration creates the Resource Group, Storage Account and private Blob Container required for the main infrastructure remote state.

## Configuration

Environment-specific settings are supplied through Azure App Service configuration.

Sensitive values are not committed to source control, including:

- database connection string
- JWT signing key
- OpenAI API key
- Stripe secret key
- Stripe webhook secret
