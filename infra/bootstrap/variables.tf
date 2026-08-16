variable "subscription_id" {
  description = "Azure subscription ID used for the Terraform state bootstrap."
  type        = string
}

variable "location" {
  description = "Azure region used for the Terraform state bootstrap."
  type        = string
  default     = "westeurope"
}
