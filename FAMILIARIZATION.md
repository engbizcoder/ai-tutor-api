# AI Tutor API Familiarization

## Current Implementation Snapshot
- **API layer**: Controllers under `ai-tutor-api/Controllers/` expose endpoints for threads, messages, files, attachments, references, organizations, and folder deletion, using mediator requests and FluentValidation (`MessagesController.cs`).
- **SignalR groundwork**: Real-time hub scaffolding in `ai-tutor-api/Hubs/` and broadcaster service (`ai-tutor-api/Services/SignalRBroadcastService.cs`) integrate with `MessagesController` for message-created events.
- **Service layer**: Vertical slices in `ai-tutor-services/Features/` implement request/handler pairs for chat threads, messages (including pagination/idempotency), file uploads, attachments, references, and folder deletion, relying on repository abstractions and `IUnitOfWork`.
- **Domain layer**: Entities, enums, and repository interfaces live in `ai-tutor-domain/`, modeling tenancy-aware data such as `ChatThread`, `ChatMessage`, `StoredFile`, `Attachment`, and `Reference`.
- **Infrastructure layer**: `ai-tutor-infrastructure/` provides `AiTutorDbContext`, EF Core configurations, repository implementations, PostgreSQL enum mappings, migrations (`20250907053030_InitialMigration`), and a storage adapter stub.
- **Contracts**: Shared DTOs and enums reside in `ai-tutor-contracts/DTOs/`, aligning API payloads with front-end expectations.

## Testing Coverage
- **Integration tests** in `Ai.Tutor.Api.IntegrationTests/` cover happy and error paths for threads, messages, attachments, references, files (upload/download/list), folder deletion cascades, and organization lifecycle workflows, executed sequentially to avoid database deadlocks.

## Outstanding Work vs. Roadmap
- **Folders**: Implement create, rename/status updates, move, and tree listing endpoints/handlers beyond the current delete flow.
- **Tenancy guard & validation**: Add middleware/filters to enforce org membership per request, localize ProblemDetails responses, and finish StyleCop/CA analyzer cleanup.
- **Files & references**: Enhance file storage integration (checksum dedupe + real storage provider) and include enriched file snippets in attachment/reference DTOs if required by contracts.
- **SignalR outbox**: Complete the background dispatcher that reads `message_events` and broadcasts via SignalR, fulfilling the outbox pattern described in roadmap docs.
- **OpenAPI & documentation**: Publish ProblemDetails schemas and DTO examples in Swagger, ensuring the API contract stays synchronized with `ai-tutor-contracts/`.
- **Testing infrastructure polish**: Adopt the Respawn-based reset helpers and shared fixtures outlined in `Ai.Tutor.Api.IntegrationTests/INTEGRATION_TESTS_PLAN.md`.

## Quick Reference Documents
- `ai-tutor-contracts/purpose.txt` — product vision, architecture, and data model overview.
- `ai-tutor-contracts/Guidelines.txt` — engineering conventions (one type per file, mediator usage, logging style, validation expectations).
- `ai-tutor-contracts/NextSteps.txt` — detailed Phase 2/3 scope, including tables, enums, endpoints, and acceptance criteria.
- `ai-tutor-contracts/execution_next_steps_current_status.txt` — snapshot of completed work (ProblemDetails, mediator wiring, thread endpoints) and immediate next tasks.
- `ai-tutor-contracts/INTEGRATION_TESTS_SETUP.md` & `Ai.Tutor.Api.IntegrationTests/INTEGRATION_TESTS_PLAN.md` — integration test architecture blueprint (Testcontainers, Respawn, xUnit collections).
