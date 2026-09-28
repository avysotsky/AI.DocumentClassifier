# Architecture

## Components

- **API** owns HTTP contracts, validation, Problem Details, JSON policy, OpenAPI, and health endpoints.
- **Application** defines the classification use case and the replaceable `IClassificationProvider` boundary.
- **Domain** contains document types and provider-independent classification results.
- **Infrastructure** contains the current local rule-based provider and validated options.

Dependencies point inward: `Api -> Application`, `Api -> Infrastructure`, `Infrastructure -> Application/Domain`, and `Application -> Domain`.

## Classification flow

1. `POST /classifications` accepts extracted text and optional document metadata.
2. Data Annotations plus `IValidatableObject` reject missing, whitespace-only, oversized, or malformed input.
3. The local provider normalizes whitespace and applies case-insensitive whole-term rules.
4. Scores are ranked deterministically; ties use the `DocumentType` order.
5. A score below `Classifier:MinimumScore` returns `Other`; otherwise the response includes the winning evidence and heuristic confidence.

Rules count each configured term at most once. This prevents repeated boilerplate from inflating a type indefinitely. Evidence is bounded by `Classifier:MaxEvidenceItems`.

## Provider extension

Implement `IClassificationProvider` for a future local ONNX model or another explicitly configured service, register it in dependency injection, and preserve the provider-independent domain result. Provider startup/model checks should be added to readiness. Model files, credentials, and endpoints must be external configuration and must not be committed.

The API does not claim calibrated probabilities. If a future provider emits calibrated confidence, document its model/version and threshold semantics under a new provider identifier.

## Operational behavior

- `/health/live` only confirms the process can answer HTTP.
- `/health/ready` confirms a named classification provider is registered.
- Unhandled exceptions are logged and returned as RFC-style Problem Details without internal exception data.
- Configuration contains no secrets; `config.json` is ignored.
- The service is stateless and safe to scale horizontally.
