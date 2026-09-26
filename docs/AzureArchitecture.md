# Healthcare Document AI - Azure Architecture

## Current Local Architecture

PDF Upload
    |
ASP.NET Core API
    |
Local File Storage
    |
Document Processing Queue
    |
Background Worker
    |
PDF Text Extraction
    |
Classification + Structured Extraction
    |
Validation
    |
Chunking
    |
Ollama Embeddings
    |
Semantic Retrieval
    |
Ollama LLM
    |
RAG Answer

## Azure Production Mapping

ASP.NET Core API
    |
    +-- Azure Blob Storage
    |      Stores uploaded healthcare documents
    |
    +-- Azure Service Bus
    |      Provides durable asynchronous processing
    |
    +-- Azure Function / Worker Service
    |      Processes queued documents
    |
    +-- Azure AI Document Intelligence
    |      OCR and structured document extraction
    |
    +-- Azure OpenAI Embeddings
    |      Generates vector embeddings
    |
    +-- Azure AI Search
    |      Stores and retrieves document chunks
    |
    +-- Azure OpenAI
    |      Generates grounded RAG responses
    |
    +-- Azure SQL / PostgreSQL
    |      Stores document metadata and processing status
    |
    +-- Application Insights
    |      Logs, traces, metrics and correlation IDs
    |
    +-- Azure Key Vault
           Stores secrets and credentials

## Component Mapping

| Local Implementation | Azure Production Service |
|---|---|
| LocalFileStorageService | Azure Blob Storage |
| PdfPig | Azure AI Document Intelligence |
| DocumentProcessingQueue | Azure Service Bus |
| DocumentProcessingWorker | Azure Function / Worker |
| OllamaService | Azure OpenAI |
| EmbeddingService | Azure OpenAI Embeddings |
| DocumentRetrievalService | Azure AI Search |
| SQLite | Azure SQL / PostgreSQL |
| Console OpenTelemetry | Application Insights |
| .env | Key Vault + Managed Identity |

## Production Processing Flow

1. Client uploads a PDF to the ASP.NET Core API.
2. API validates file type, size and PDF signature.
3. File is stored in Azure Blob Storage.
4. API creates document metadata in the database.
5. A processing message is sent to Azure Service Bus.
6. Worker receives the message.
7. Azure AI Document Intelligence extracts document text.
8. Document type is classified.
9. Structured healthcare fields are extracted.
10. Business validation rules are applied.
11. Text is divided into overlapping chunks.
12. Azure OpenAI generates embeddings.
13. Chunks and vectors are indexed in Azure AI Search.
14. User submits a question.
15. The API generates a query embedding.
16. Azure AI Search retrieves relevant chunks.
17. Retrieved context is sent to Azure OpenAI.
18. Azure OpenAI generates a grounded response.
19. OpenTelemetry sends traces and metrics to Application Insights.

## Security

- Managed Identity for Azure service authentication
- Azure Key Vault for secrets
- HTTPS for all API communication
- RBAC for resource access
- No healthcare document contents written to application logs
- Correlation IDs for distributed tracing
- Private endpoints can be enabled for production services
