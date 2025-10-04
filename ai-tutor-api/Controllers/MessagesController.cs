namespace Ai.Tutor.Api.Controllers;

using Ai.Tutor.Api.DTOs;
using Ai.Tutor.Api.Services;
using Ai.Tutor.Services.Mediation;
using Contracts.DTOs;
using Contracts.Enums;
using Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

[ApiController]
[Route("api/orgs/{orgId:guid}/threads/{threadId:guid}/messages")]
public sealed class MessagesController(
    IMediator mediator,
    IValidator<ListMessagesQueryParams> listValidator,
    IValidator<Contracts.DTOs.CreateMessageRequest> createValidator,
    ISignalRBroadcastService signalRBroadcastService,
    ILogger<MessagesController> logger) : ControllerBase
{
    [HttpGet(Name = "ListMessages")]
    public async Task<ActionResult<PagedMessagesResponse>> ListAsync(
        [FromRoute] Guid orgId,
        [FromRoute] Guid threadId,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? cursor = null,
        [FromQuery] bool includeMetadata = false,
        CancellationToken ct = default)
    {
        // Validate request parameters using FluentValidation
        var queryParams = new ListMessagesQueryParams { PageSize = pageSize, Cursor = cursor };
        var validationResult = await listValidator.ValidateAsync(queryParams, ct);
        
        if (!validationResult.IsValid)
        {
            var problemDetails = new ValidationProblemDetails();
            foreach (var error in validationResult.Errors)
            {
                problemDetails.Errors.Add(error.PropertyName, [error.ErrorMessage]);
                problemDetails.Extensions.Add($"errorCode_{error.PropertyName}", error.ErrorCode);
            }
            problemDetails.Title = "Validation failed";
            problemDetails.Status = StatusCodes.Status400BadRequest;
            return this.BadRequest(problemDetails);
        }

        var result = await mediator.Send(
            new Ai.Tutor.Services.Features.Messages.ListMessagesRequest
            {
                OrgId = orgId,
                ThreadId = threadId,
                PageSize = pageSize,
                Cursor = cursor,
            },
            ct);

        var response = new PagedMessagesResponse
        {
            Items = result.Items.Select(x => this.MapToDto(x, includeMetadata)).ToList(),
            NextCursor = result.NextCursor,
            HasMore = result.NextCursor != null,
        };

        return this.Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<MessageDto>> CreateAsync(
        [FromRoute] Guid orgId,
        [FromRoute] Guid threadId,
        [FromBody] Contracts.DTOs.CreateMessageRequest req,
        CancellationToken ct)
    {
        // Validate request using FluentValidation
        var validationResult = await createValidator.ValidateAsync(req, ct);
        
        if (!validationResult.IsValid)
        {
            var problemDetails = new ValidationProblemDetails();
            foreach (var error in validationResult.Errors)
            {
                problemDetails.Errors.Add(error.PropertyName, [error.ErrorMessage]);
                problemDetails.Extensions.Add($"errorCode_{error.PropertyName}", error.ErrorCode);
            }
            problemDetails.Title = "Validation failed";
            problemDetails.Status = StatusCodes.Status400BadRequest;
            return this.BadRequest(problemDetails);
        }

        // Extract idempotency key from headers if provided
        string? idempotencyKey = null;
        if (this.Request.Headers.TryGetValue("Idempotency-Key", out var headerValues))
        {
            idempotencyKey = headerValues.FirstOrDefault();
        }

        var created = await mediator.Send(
            new Ai.Tutor.Services.Features.Messages.CreateMessageRequest
            {
                OrgId = orgId,
                ThreadId = threadId,
                Content = req.Content,
                SenderType = (Domain.Enums.SenderType)req.SenderType,
                SenderId = req.SenderId,
                MetadataJson = req.MetadataJson,
                IdempotencyKey = idempotencyKey ?? req.IdempotencyKey,
            },
            ct);

        // Fire-and-forget SignalR broadcast, do not impact HTTP response on failure
        try
        {
            await signalRBroadcastService.BroadcastMessageCreatedAsync(created, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SignalR broadcast failed for created message. OrgId={OrgId}, ThreadId={ThreadId}, MessageId={MessageId}", orgId, threadId, created.Id);
        }

        var dto = this.MapToDto(created, includeMetadata: true);
        return this.CreatedAtRoute("ListMessages", new { orgId, threadId }, dto);
    }

    private MessageDto MapToDto(ChatMessage x, bool includeMetadata = false) => new()
    {
        Id = x.Id,
        ThreadId = x.ThreadId,
        SenderType = (SenderType)x.SenderType,
        SenderId = x.SenderId,
        Status = (MessageStatus)x.Status,
        Content = x.Content,
        MetadataJson = includeMetadata ? x.MetadataJson : null,
        CreatedAt = x.CreatedAt,
        UpdatedAt = x.UpdatedAt,
    };
}

