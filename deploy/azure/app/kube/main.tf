########
# Application level stuff will live here
# Each module is conditionally created within this app infra definition interface and can be re-used across app types e.g. SSR webapp, API only
########

# Naming convention
module "default_label" {
  source     = "git::https://github.com/cloudposse/terraform-null-label.git?ref=0.24.1"
  namespace  = "${var.name_company}-${var.name_project}"
  stage      = var.stage
  name       = "${lookup(var.location_name_map, var.resource_group_location, "uksouth")}-${var.name_domain}"
  attributes = var.attributes
  delimiter  = "-"
  tags       = var.tags
}

module "app" {
  source                               = "git::https://github.com/amido/stacks-terraform//azurerm/modules/azurerm-server-side-app?ref=v1.5.2"
  create_cosmosdb                      = var.create_cosmosdb
  resource_namer                       = module.default_label.id
  resource_tags                        = module.default_label.tags
  resource_group_location              = var.resource_group_location
  cosmosdb_sql_container               = var.cosmosdb_sql_container
  cosmosdb_sql_container_partition_key = var.cosmosdb_sql_container_partition_key
  cosmosdb_kind                        = var.cosmosdb_kind
  cosmosdb_offer_type                  = var.cosmosdb_offer_type
  create_cache                         = var.create_cache
  create_dns_record                    = var.create_dns_record
  dns_record                           = var.dns_record
  dns_zone_name                        = data.terraform_remote_state.core.outputs.dns_base_domain
  core_resource_group                  = data.terraform_remote_state.core.outputs.resource_group_name
  dns_zone_resource_group              = data.terraform_remote_state.core.outputs.dns_resource_group_name != "" ? data.terraform_remote_state.core.outputs.dns_resource_group_name : data.terraform_remote_state.core.outputs.resource_group_name
  dns_a_records                        = [data.terraform_remote_state.core.outputs.app_gateway_ip]
  subscription_id                      = data.azurerm_client_config.current.subscription_id
  create_cdn_endpoint                  = var.create_cdn_endpoint
  # Alternatively if you want you can pass in the IP directly and remove the need for a lookup
  # dns_a_records                        = ["0.1.23.45"]
}

module "servicebus" {
  count                      = contains(split(",", var.app_bus_type), "servicebus") ? 1 : 0
  source                     = "../servicebus"
  resource_group_name        = module.default_label.id
  resource_group_location    = var.resource_group_location
  name_company               = var.name_company
  name_project               = var.name_project
  name_domain                = var.name_domain
  stage                      = var.stage
  cosmosdb_database_name     = module.app.cosmosdb_database_name
  cosmosdb_collection_name   = var.cosmosdb_sql_container
  cosmosdb_connection_string = "AccountEndpoint=${module.app.cosmosdb_endpoint};AccountKey=${module.app.cosmosdb_primary_master_key};"
}

module "eventhub" {
  count                   = contains(split(",", var.app_bus_type), "eventhub") ? 1 : 0
  source                  = "../eventhub"
  resource_group_name     = module.default_label.id
  resource_group_location = var.resource_group_location
}
