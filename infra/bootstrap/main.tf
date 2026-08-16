resource "azurerm_resource_group" "terraform_state" {
  name     = "rg-subscription-manager-tfstate"
  location = var.location
}

resource "azurerm_storage_account" "terraform_state" {
  name                = "stsubmanagertfstate01"
  resource_group_name = azurerm_resource_group.terraform_state.name
  location            = azurerm_resource_group.terraform_state.location

  account_kind             = "StorageV2"
  account_tier             = "Standard"
  account_replication_type = "LRS"

  https_traffic_only_enabled      = true
  min_tls_version                 = "TLS1_2"
  allow_nested_items_to_be_public = false
  shared_access_key_enabled       = false

  blob_properties {
    versioning_enabled = true
  }
}

resource "azurerm_storage_container" "terraform_state" {
  name                  = "tfstate"
  storage_account_id    = azurerm_storage_account.terraform_state.id
  container_access_type = "private"
}
