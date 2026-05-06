# Property Support RAG API - Key Based

Backend-only .NET 8 Web API for a property search support chatbot.

## Setup

Open terminal inside `PropertySupport.RagApi`:

```bash
dotnet user-secrets init

dotnet user-secrets set "AzureOpenAI:Endpoint" "https://YOUR-OPENAI-RESOURCE.openai.azure.com/"
dotnet user-secrets set "AzureOpenAI:DeploymentName" "gpt-4o"
dotnet user-secrets set "AzureOpenAI:ApiKey" "YOUR-AZURE-OPENAI-KEY"

dotnet user-secrets set "AzureAiSearch:Endpoint" "https://YOUR-SEARCH-SERVICE.search.windows.net"
dotnet user-secrets set "AzureAiSearch:ApiKey" "YOUR-AZURE-AI-SEARCH-KEY"
dotnet user-secrets set "AzureAiSearch:IndexName" "property-support-index"
dotnet user-secrets set "AzureAiSearch:ContentFieldName" "content"
dotnet user-secrets set "AzureAiSearch:TitleFieldName" "metadata_storage_name"
dotnet user-secrets set "AzureAiSearch:UrlFieldName" "metadata_storage_path"
```

## Run

```bash
dotnet restore
dotnet run
```

Open:

```text
https://localhost:7288/swagger
```

## Test search first

```text
GET /api/knowledge/search?question=postcode validation error
```

## Test chat

```json
{
  "message": "I get a validation error when searching by postcode. How do I fix it?",
  "history": []
}
```

## Later React integration

React can call:

```text
POST https://localhost:7288/api/chat
```

CORS is already enabled for:

```text
http://localhost:5173
https://localhost:5173
```
