# AI Document Classifier

.NET 8 REST service that classifies document text as `Invoice`, `Contract`, `CV`, `Complaint`, `Report`, or `Other`. The default provider is deterministic, local, explainable, and uses no external or paid model API.

This is project 4 in the local document pipeline:

`Document Ingestion (1) -> Semantic Search (2) -> RAG Answer (3) -> Document Classifier (4)`

## Run

Requirements: .NET 8 SDK. No secrets or external services are required.

```powershell
dotnet run --project src/AI.DocumentClassifier.Api --urls http://localhost:5104
```

Optional settings can be copied from `config.example.json` to the ignored `config.json`, supplied via `DOCUMENT_CLASSIFIER_CONFIG_PATH`, or overridden with standard .NET configuration environment variables.

Endpoints:

- `POST /classifications`
- `GET /health/live`
- `GET /health/ready`
- `GET /swagger`

Example:

```powershell
$body = @{ documentId = 'c0870330-2163-44cf-82a6-bb9ff49059ee'; fileName = 'invoice.txt'; text = 'Invoice number 42. Bill to Acme. Amount due: 120 EUR.' } | ConvertTo-Json
Invoke-RestMethod http://localhost:5104/classifications -Method Post -ContentType application/json -Body $body
```

Response includes the selected `type`, a bounded `confidence`, matched `evidence` with excerpts, and the provider identifier. Unknown JSON members and invalid payloads are rejected with Problem Details.

## Verify

```powershell
dotnet build AI.DocumentClassifier.sln --configuration Release
dotnet test AI.DocumentClassifier.sln --configuration Release --no-build
```

## Limits

- The baseline recognizes explicit English document terminology; it is not a semantic model.
- Confidence is a deterministic heuristic based on winner score and competing score, not a calibrated probability.
- Text extraction remains the responsibility of `AI.DocumentIngestionService`.
- No external model access is assumed. A future local ONNX or explicitly configured AI implementation can replace `IClassificationProvider`.

See [architecture](docs/architecture.md) for boundaries and extension guidance.
