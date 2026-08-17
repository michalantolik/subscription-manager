locals {
  project     = "subscription-manager"
  environment = "prod"

  resource_group_name = "rg-${local.project}-${local.environment}"

  api_app_name = "app-${local.project}-api-${local.environment}"
  web_app_name = "app-${local.project}-web-${local.environment}"

  api_url        = "https://${local.api_app_name}.azurewebsites.net"
  web_url        = "https://${local.web_app_name}.azurewebsites.net"
  public_web_url = "https://submanager.dev"

  tags = {
    Project     = "Subscription Manager"
    Environment = "Production"
  }
}
