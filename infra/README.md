# Infrastructure

Subscription Manager is deployed to Microsoft Azure using Terraform.

## Environment

The project uses a single Azure environment:

- Production

Local development remains the development environment.

## Region

Azure resources are deployed to:

- West Europe

## Azure Resources

The production environment consists of:

- Resource Group
- Linux App Service Plan
- API App Service
- Web App Service
- Azure SQL Server
- Azure SQL Database
- Log Analytics Workspace
- Application Insights
- Azure Communication Services Email

## Application Hosting

The API and Web applications run as separate Azure App Services on the same Linux App Service Plan.

The Web application communicates with the API over HTTPS.

## Database

The application uses Azure SQL Database.

This matches the existing EF Core SQL Server persistence implementation.

## Monitoring

Application Insights is used for application telemetry.

Application Insights is connected to a Log Analytics Workspace.

## Email

Azure Communication Services Email is used to send application emails.

The email service is dedicated to Subscription Manager and is managed as part of its Azure infrastructure.

## Configuration

Environment-specific application settings are supplied through the Azure App Service configuration.

Sensitive values are not committed to source control.

Examples include:

- database connection string
- JWT signing key
- OpenAI API key
- Stripe secret key
- Stripe webhook secret

## Terraform

Terraform is used to define and manage the Azure infrastructure.

Terraform state is stored remotely in Azure Storage.

The infrastructure follows the standard Terraform workflow:

1. `terraform init`
2. `terraform plan`
3. `terraform apply`

## Scope

The initial infrastructure intentionally avoids services that are not currently required by the application, including:

- Azure Kubernetes Service
- Azure Container Registry
- Azure API Management
- Azure Virtual Network
- Private Endpoints
- Azure Key Vault
