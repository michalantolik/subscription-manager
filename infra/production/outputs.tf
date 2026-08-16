output "resource_group_name" {
  description = "Resource Group name containing the production infrastructure."
  value       = azurerm_resource_group.production.name
}

output "api_url" {
  description = "Production API URL."
  value       = local.api_url
}

output "web_url" {
  description = "Production Web application URL."
  value       = local.web_url
}
