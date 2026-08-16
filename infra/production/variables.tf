variable "subscription_id" {
  description = "Azure subscription ID used for the production infrastructure."
  type        = string
}

variable "location" {
  description = "Azure region used for the production infrastructure."
  type        = string
  default     = "westeurope"
}

variable "sql_admin_login" {
  description = "Microsoft Entra ID login used as the Azure SQL administrator."
  type        = string
}

variable "sql_admin_object_id" {
  description = "Microsoft Entra ID object ID used as the Azure SQL administrator."
  type        = string
}
