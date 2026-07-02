# Smart Transactions Support Agent

This repository contains the application source code for the Smart Transactions Support Agent, a Retrieval-Augmented Generation (RAG) assistant designed for context-aware technical support.

## Architecture Overview

* **Frontend UI (`SupportAgent.UI`):** Blazor application driving the interactive chat interface.
* **Backend API (`TransactionService.Api`):** .NET Web API central orchestrator executing MediatR pipelines.
* **Vector Database:** Qdrant instance handling 1536-dimensional semantic similarity vectors over gRPC.
* **LLM Engine:** OpenAI integration leveraging `gpt-4o-mini` for inference and `text-embedding-3-small` for vectorization.

## Prerequisites & Dependencies

* .NET 10.0 Runtime / Hosting Bundle (Required for IIS)
* IIS 10.0+ with WebSockets enabled
* Docker Engine / Compose Runtime
* OpenAI API Subscription Key

## Configuration Schema

### API Configuration (`TransactionService.Api/appsettings.json`)
```json
{
  "OpenAI": {
    "ApiKey": "sk-proj-YOUR_API_KEY"
  },
  "Qdrant": {
    "Host": "localhost",
    "GrpcPort": 6334
  }
}
```
### UI Configuration (SupportAgent.UI)
Administrators can instantly toggle the file upload functionality or rebrand the bot's identity without requiring a new build cycle.
JSON
```
{
  "ChatSettings": {
    "BotName": "Pesapal", 
    "EnableFileUpload": true
  }
}
```
### Infrastructure & Deployment Configurations
Target A: IIS Hosting + Isolated Qdrant Container (Recommended)
For standard environments running .NET workloads natively on IIS, provision the Qdrant engine via Docker. Data persistence is critical: you must map a persistent volume (-v) so vector data survives container reboots.

PowerShell
```
docker run -d `
  -p 6333:6333 `
  -p 6334:6334 `
  -v C:\QdrantData:/qdrant/storage:z `
  --name qdrant-vector-db `
  --restart unless-stopped `
  qdrant/qdrant
```
#### Target B: Full-Stack Containerization (Docker Compose)
For fully containerized deployments, execute the following multi-container configuration from the repository root. This setup utilizes internal Docker DNS name resolution for service-to-service communication.
```
version: '3.8'

services:
  qdrant:
    image: qdrant/qdrant:latest
    container_name: qdrant-vector-db
    ports:
      - "6333:6333"
      - "6334:6334"
    volumes:
      - qdrant_data:/qdrant/storage
    restart: unless-stopped

  api:
    build:
      context: .
      dockerfile: TransactionService.Api/Dockerfile
    container_name: support-agent-api
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - OpenAI__ApiKey=${OPENAI_API_KEY}
      - Qdrant__Host=qdrant
      - Qdrant__GrpcPort=6334
    depends_on:
      - qdrant
    restart: unless-stopped

  ui:
    build:
      context: .
      dockerfile: SupportAgent.UI/Dockerfile
    container_name: support-agent-ui
    ports:
      - "8080:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
    depends_on:
      - api
    restart: unless-stopped

volumes:
  qdrant_data:
```
## Security Policy
Critical: The API endpoints and the UI presentation layer operate completely without security tokens, JWTs, or native authentication protocols.

To prevent unauthorized access to proprietary API documentation or unauthorized utilization of the OpenAI API key:

Network routing must be guarded behind corporate firewalls or private virtual networks.

External access should be strictly restricted via IIS IP Address and Domain Restrictions, or configured behind a secure reverse-proxy routing rule.

## Cold Start & Vector Ingestion
Upon initial deployment, the local Qdrant database will be completely empty. The AI will not possess contextual documentation knowledge until the initial ingestion process is executed.

To seed the knowledge base:

Validate that "EnableFileUpload": true is set within the UI configuration.

Access the deployed Blazor interface via a web browser.

Click the attachment icon (📎) and upload the target technical documentation (PDFs).

The underlying API service will automatically intercept the upload, extract and chunk the text, execute remote OpenAI vector embedding generation, and map the records to Qdrant payloads via a gRPC batch-upsert.

Once complete, the automated SearchApiDocumentation tool will immediately begin routing contextual answers to the chat interface.
