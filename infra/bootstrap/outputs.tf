output "resource_group_name" {
  description = "Resource Group name containing the Terraform state infrastructure."
  value       = azurerm_resource_group.terraform_state.name
}

output "storage_account_name" {
  description = "Storage Account name used for Terraform state."
  value       = azurerm_storage_account.terraform_state.name
}

output "container_name" {
  description = "Blob container name used for Terraform state."
  value       = azurerm_storage_container.terraform_state.name
}
