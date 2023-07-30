# Semantic Kernel syntax examples

This project contains a collection of semi-random examples about various scenarios
using SK components. 

The examples are ordered by number, starting with very basic examples.

Most of the examples will require secrets and credentials, to access OpenAI, Azure OpenAI,
Bing and other resources. We suggest using .NET 
[Secret Manager](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)
to avoid the risk of leaking secrets into the repository, branches and pull requests.
You can also use environment variables if you prefer.

To set your secrets with Secret Manager:
```
cd dotnet/samples/KernelSyntaxExamples

dotnet user-secrets init

dotnet user-secrets set "OpenAI:ModelId" "..."
dotnet user-secrets set "OpenAI:ChatModelId" "..."
dotnet user-secrets set "OpenAI:EmbeddingModelId" "..."
dotnet user-secrets set "OpenAI:ApiKey" "..."

dotnet user-secrets set "AzureOpenAI:ServiceId" "..."
dotnet user-secrets set "AzureOpenAI:DeploymentName" "..."
dotnet user-secrets set "AzureOpenAI:ChatDeploymentName" "..."
dotnet user-secrets set "AzureOpenAI:Endpoint" "https://... .openai.azure.com/"
dotnet user-secrets set "AzureOpenAI:ApiKey" "..."

dotnet user-secrets set "AzureOpenAIEmbeddings:DeploymentName" "..."
dotnet user-secrets set "AzureOpenAIEmbeddings:Endpoint" "https://... .openai.azure.com/"
dotnet user-secrets set "AzureOpenAIEmbeddings:ApiKey" "..."

dotnet user-secrets set "ACS:Endpoint" "https://... .search.windows.net"
dotnet user-secrets set "ACS:ApiKey" "..."

dotnet user-secrets set "Qdrant:Endpoint" "..."
dotnet user-secrets set "Qdrant:Port" "..."

dotnet user-secrets set "Weaviate:Scheme" "..."
dotnet user-secrets set "Weaviate:Endpoint" "..."
dotnet user-secrets set "Weaviate:Port" "..."
dotnet user-secrets set "Weaviate:ApiKey" "..."

dotnet user-secrets set "KeyVault:Endpoint" "..."
dotnet user-secrets set "KeyVault:ClientId" "..."
dotnet user-secrets set "KeyVault:TenantId" "..."

dotnet user-secrets set "HuggingFace:ApiKey" "..."
dotnet user-secrets set "HuggingFace:ModelId" "..."

dotnet user-secrets set "Pinecone:ApiKey" "..."
dotnet user-secrets set "Pinecone:Environment" "..."

dotnet user-secrets set "Jira:ApiKey" "..."
dotnet user-secrets set "Jira:Email" "..."
dotnet user-secrets set "Jira:Domain" "..."

dotnet user-secrets set "Bing:ApiKey" "..."

dotnet user-secrets set "Google:ApiKey" "..."
dotnet user-secrets set "Google:SearchEngineId" "..."

dotnet user-secrets set "Github:PAT" "github_pat_..."

dotnet user-secrets set "Apim:Endpoint" "https://apim...azure-api.net/"
dotnet user-secrets set "Apim:SubscriptionKey" "..."

dotnet user-secrets set "Postgres:ConnectionString" "..."
dotnet user-secrets set "Redis:Configuration" "..."
```

To set your secrets with environment variables, use these names:
```
# OpenAI
OpenAI__ModelId
OpenAI__ChatModelId
OpenAI__EmbeddingModelId
OpenAI__ApiKey

# Azure OpenAI
AzureOpenAI__ServiceId
AzureOpenAI__DeploymentName
AzureOpenAI__ChatDeploymentName
AzureOpenAI__Endpoint
AzureOpenAI__ApiKey

AzureOpenAIEmbeddings__DeploymentName
AzureOpenAIEmbeddings__Endpoint
AzureOpenAIEmbeddings__ApiKey

# Azure Cognitive Search
ACS__Endpoint
ACS__ApiKey

# Qdrant
Qdrant__Endpoint
Qdrant__Port

# Weaviate
Weaviate__Scheme
Weaviate__Endpoint
Weaviate__Port
Weaviate__ApiKey

# Azure Key Vault
KeyVault__Endpoint
KeyVault__ClientId
KeyVault__TenantId

# Hugging Face
HuggingFace__ApiKey
HuggingFace__ModelId

# Pinecone
Pinecone__ApiKey
Pinecone__Environment

# Jira
Jira__ApiKey
Jira__Email
Jira__Domain

# Bing
Bing__ApiKey

# Google
Google__ApiKey
Google__SearchEngineId

# Github
Github__PAT

# Azure API Management (APIM)
Apim__Endpoint
Apim__SubscriptionKey

# Other
Postgres__ConnectionString
Redis__Configuration
```
