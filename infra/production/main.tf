data "azurerm_client_config" "current" {}

resource "azurerm_resource_group" "production" {
  name     = local.resource_group_name
  location = var.location

  tags = local.tags
}

resource "azurerm_user_assigned_identity" "deployment" {
  name                = "id-${local.project}-deploy-${local.environment}"
  resource_group_name = azurerm_resource_group.production.name
  location            = azurerm_resource_group.production.location

  tags = local.tags
}

resource "azurerm_federated_identity_credential" "github_production" {
  name                      = "github-production"
  user_assigned_identity_id = azurerm_user_assigned_identity.deployment.id

  audience = [
    "api://AzureADTokenExchange"
  ]

  issuer  = "https://token.actions.githubusercontent.com"
  subject = "repo:michalantolik@30344910/subscription-manager@1304476763:environment:production"
}

resource "azurerm_service_plan" "production" {
  name                = "asp-${local.project}-${local.environment}"
  resource_group_name = azurerm_resource_group.production.name
  location            = azurerm_resource_group.production.location

  os_type  = "Linux"
  sku_name = "B1"

  tags = local.tags
}

resource "azurerm_linux_web_app" "api" {
  name                = local.api_app_name
  resource_group_name = azurerm_resource_group.production.name
  location            = azurerm_resource_group.production.location
  service_plan_id     = azurerm_service_plan.production.id

  https_only = true

  ftp_publish_basic_authentication_enabled       = false
  webdeploy_publish_basic_authentication_enabled = false

  identity {
    type = "SystemAssigned"
  }

  app_settings = {
    "ApplicationInsights__ConnectionString"  = azurerm_application_insights.production.connection_string
    "ConnectionStrings__SubscriptionManager" = "Server=tcp:${azurerm_mssql_server.production.fully_qualified_domain_name},1433;Database=${azurerm_mssql_database.production.name};Authentication=Active Directory Managed Identity;Encrypt=True;TrustServerCertificate=False;"
    "Email__ApplicationBaseUrl"              = local.web_url
    "AzureEmail__Endpoint"                   = "https://${azurerm_communication_service.production.hostname}"
    "AzureEmail__SenderAddress"              = "donotreply@${azurerm_email_communication_service_domain.production.mail_from_sender_domain}"
    "Jwt__SigningKey"                        = "@Microsoft.KeyVault(VaultName=${azurerm_key_vault.production.name};SecretName=jwt-signing-key)"
  }

  logs {
    application_logs {
      file_system_level = "Information"
    }

    http_logs {
      file_system {
        retention_in_days = 3
        retention_in_mb   = 100
      }
    }

    detailed_error_messages = false
    failed_request_tracing  = false
  }

  site_config {
    application_stack {
      dotnet_version = "10.0"
    }

    health_check_path                 = "/health"
    health_check_eviction_time_in_min = 5
  }

  tags = local.tags
}

resource "azurerm_linux_web_app" "web" {
  name                = local.web_app_name
  resource_group_name = azurerm_resource_group.production.name
  location            = azurerm_resource_group.production.location
  service_plan_id     = azurerm_service_plan.production.id

  https_only = true

  ftp_publish_basic_authentication_enabled       = false
  webdeploy_publish_basic_authentication_enabled = false

  app_settings = {
    "ApplicationInsights__ConnectionString" = azurerm_application_insights.production.connection_string
    "Api__BaseUrl"                          = local.api_url
  }

  site_config {
    application_stack {
      dotnet_version = "10.0"
    }

    health_check_path                 = "/health"
    health_check_eviction_time_in_min = 5
  }

  tags = local.tags
}

resource "azurerm_mssql_server" "production" {
  name                = "sql-${local.project}-${local.environment}"
  resource_group_name = azurerm_resource_group.production.name
  location            = azurerm_resource_group.production.location
  version             = "12.0"

  azuread_administrator {
    login_username              = var.sql_admin_login
    object_id                   = var.sql_admin_object_id
    azuread_authentication_only = true
  }

  tags = local.tags
}

resource "azurerm_mssql_database" "production" {
  name      = "sqldb-${local.project}-${local.environment}"
  server_id = azurerm_mssql_server.production.id

  sku_name    = "Basic"
  max_size_gb = 2

  tags = local.tags
}

resource "azurerm_mssql_firewall_rule" "allow_azure_services" {
  name      = "AllowAzureServices"
  server_id = azurerm_mssql_server.production.id

  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

resource "azurerm_key_vault" "production" {
  name                = "kv-submanager-prod"
  resource_group_name = azurerm_resource_group.production.name
  location            = azurerm_resource_group.production.location
  tenant_id           = data.azurerm_client_config.current.tenant_id

  sku_name = "standard"

  rbac_authorization_enabled = true

  soft_delete_retention_days = 7
  purge_protection_enabled   = false

  tags = local.tags
}

resource "azurerm_role_assignment" "api_key_vault_secrets" {
  scope                = azurerm_key_vault.production.id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_linux_web_app.api.identity[0].principal_id
}

resource "azurerm_log_analytics_workspace" "production" {
  name                = "log-${local.project}-${local.environment}"
  resource_group_name = azurerm_resource_group.production.name
  location            = azurerm_resource_group.production.location

  sku               = "PerGB2018"
  retention_in_days = 30

  tags = local.tags
}

resource "azurerm_application_insights" "production" {
  name                = "appi-${local.project}-${local.environment}"
  resource_group_name = azurerm_resource_group.production.name
  location            = azurerm_resource_group.production.location
  workspace_id        = azurerm_log_analytics_workspace.production.id

  application_type = "web"

  tags = local.tags
}

resource "azurerm_communication_service" "production" {
  name                = "acs-${local.project}-${local.environment}"
  resource_group_name = azurerm_resource_group.production.name
  data_location       = "Europe"

  tags = local.tags
}

resource "azurerm_email_communication_service" "production" {
  name                = "email-${local.project}-${local.environment}"
  resource_group_name = azurerm_resource_group.production.name
  data_location       = "Europe"

  tags = local.tags
}

resource "azurerm_email_communication_service_domain" "production" {
  name             = "AzureManagedDomain"
  email_service_id = azurerm_email_communication_service.production.id

  domain_management = "AzureManaged"

  tags = local.tags
}

resource "azurerm_communication_service_email_domain_association" "production" {
  communication_service_id = azurerm_communication_service.production.id
  email_service_domain_id  = azurerm_email_communication_service_domain.production.id
}

resource "azurerm_role_assignment" "api_communication_service" {
  scope                = azurerm_communication_service.production.id
  role_definition_name = "Communication and Email Service Owner"
  principal_id         = azurerm_linux_web_app.api.identity[0].principal_id
}
