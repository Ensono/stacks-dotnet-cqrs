@(
  @{
    displayName = "AwsDeployment"
    template = "deploy/k8s/aws/base_api-deploy.yml"
    vars = @{
        dns_pointer = "`${ENV_NAME}-`${DOMAIN}.`${DNS_BASE_DOMAIN}"
        environment = "`${ENV_NAME}"
        tls_domain = "`${DNS_BASE_DOMAIN}"
        k8s_app_route = "`${K8S_APP_ROUTE}"
        log_level = "Information"
        k8s_image = "`${DOCKER_REGISTRY}/`${DOCKER_IMAGE_NAME}:`${DOCKER_IMAGE_TAG}"
        cloudwatch_log_group_name = "`${CLOUDWATCH_LOG_GROUP}"
        cloudwatch_log_prefix ="`${CLOUDWATCH_STREAM_PREFIX}"
        cloudwatch_region = "`${REGION}"
        version = "`${DOCKER_IMAGE_TAG}"
        rewrite_target = '/$([char]0x0024)2' # Using UniCode to prevent substitution
        k8s_worker_image = "`${DOCKER_REGISTRY}/`${DOCKER_IMAGE_NAME_BG_WORKER}:`${DOCKER_IMAGE_TAG}"
    }
  }

    @{
    displayName = "AzureHelm"
    template = "deploy/helm/stacks-dotnet/values.yaml"
    vars = @{
        host = "`${ENV_NAME}-`${DOMAIN}.`${DNS_BASE_DOMAIN}"
        environment = "`${ENV_NAME}"
        apppath = "`${K8S_APP_ROUTE}"
        loglevel = "Debug"
        k8s_image = "`${DOCKER_REGISTRY}/`${DOCKER_IMAGE_NAME}:`${DOCKER_IMAGE_TAG}"
        aadpodidentitybinding = "stacks-webapp-identity"
        app_insights_key = "`${APP_INSIGHTS_INSTRUMENTATION_KEY}"
        version = "`${DOCKER_IMAGE_TAG}"
        rewrite_target = '/$([char]0x0024)2' # Using UniCode to prevent substitution
        cosmosdb_key = "`${COSMOSDB_PRIMARY_MASTER_KEY}"
        cosmosdb_endpoint = "`${COSMOSDB_ENDPOINT}"
        cosmosdb_name = "`${COSMOSDB_DATABASE_NAME}"
        servicebus_topic_name = "`${SERVICEBUS_TOPIC_NAME}"
        servicebus_subscription_name = "`${SERVICEBUS_SUBSCRIPTION_NAME}"
        servicebus_connectionstring = "`${SERVICEBUS_CONNECTIONSTRING}"
        worker_image = "`${DOCKER_REGISTRY}/`${DOCKER_IMAGE_NAME_BG_WORKER}:`${DOCKER_IMAGE_TAG}"
    }
}
)

# # AWS is expecting env vars or TF outputs for:
# ENV_NAME (i.e. nonprod/prod)
# DOMAIN (i.e. the business domain prefix in DNS)
# DNS_BASE_DOMAIN (i.e. the base DNS domain)
# K8S_APP_ROUTE (i.e. the route after the domain, to the particular service)
# DOCKER_REGISTRY, DOCKER_IMAGE_NAME and DOCKER_IMAGE_TAG to construct the image being used
# CLOUDWATCH_LOG_GROUP, CLOUDWATCH_STREAM_PREFIX and CLOUDWATCH_REGION for logging
# VERSION (for K8S resource versioning)
# TBC: additional vars used in the file which map straight to env-vars
# SNS_TOPIC_ARN for Eventing / SNS integration Queue
# REGION for AWS config
