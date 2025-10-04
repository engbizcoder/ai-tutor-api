namespace Ai.Tutor.Api.Hubs;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Ai.Tutor.Domain.Repositories;
using Contracts.DTOs;
using Domain.Exceptions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

public sealed class ThreadsHub(
    ILogger<ThreadsHub> logger,
    IOrgRepository orgs)
    : Hub<IThreadsHubClient>
{
    public override async Task OnConnectedAsync()
    {
        try
        {
            var httpContext = this.Context.GetHttpContext();
            var orgIdStr = httpContext?.Request.Query["orgId"].ToString();
            var userIdStr = httpContext?.Request.Query["userId"].ToString();

            if (!Guid.TryParse(orgIdStr, out var orgId))
            {
                await this.Clients.Caller.ErrorOccurred(
                    new SignalRErrorDto { Code = "INVALID_THREAD", Message = "Missing or invalid orgId.", Retryable = false, Metadata = new { orgId = orgIdStr } });
                throw new SignalRConnectionException("Missing or invalid orgId.");
            }

            // Validate org exists and user membership
            if (!string.IsNullOrWhiteSpace(userIdStr) && !Guid.TryParse(userIdStr, out _))
            {
                await this.Clients.Caller.ErrorOccurred(
                    new SignalRErrorDto { Code = "THREAD_ACCESS_DENIED", Message = "Invalid userId.", Retryable = false, Metadata = new { userId = userIdStr } });
                throw new ThreadAccessDeniedException("Invalid userId.");
            }

            // Validate org exists
            var org = await orgs.GetByIdAsync(orgId);
            if (org is null)
            {
                await this.Clients.Caller.ErrorOccurred(
                    new SignalRErrorDto { Code = "INVALID_THREAD", Message = "Organization not found.", Retryable = false, Metadata = new { orgId } });
                throw new InvalidThreadException($"Organization {orgId} not found.");
            }

            // Validate user membership
            var userOrgIds = GetUserOrgIds();
            if (!userOrgIds.Contains(orgId))
            {
                await this.Clients.Caller.ErrorOccurred(
                    new SignalRErrorDto { Code = "THREAD_ACCESS_DENIED", Message = "User does not belong to the organization.", Retryable = false, Metadata = new { orgId } });
                throw new ThreadAccessDeniedException($"User does not have access to org {orgId}.");
            }

            logger.LogInformation("SignalR connected. ConnectionId={ConnectionId}, OrgId={OrgId}", this.Context.ConnectionId, orgId);
            await base.OnConnectedAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during SignalR connection initialization. ConnectionId={ConnectionId}", this.Context.ConnectionId);
            throw;
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        logger.LogInformation("SignalR disconnected. ConnectionId={ConnectionId}", this.Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinThreadAsync(Guid threadId)
    {
        try
        {
            var group = GetThreadGroup(threadId);
            await this.Groups.AddToGroupAsync(this.Context.ConnectionId, group);
            logger.LogInformation("Joined group {Group} ConnectionId={ConnectionId}", group, this.Context.ConnectionId);
        }
        catch (Exception ex)
        {
            await this.Clients.Caller.ErrorOccurred(new SignalRErrorDto { Code = "SIGNALR_CONNECTION_ERROR", Message = "Failed to join thread group.", Retryable = true, Metadata = new { threadId } });
            throw new SignalRConnectionException("Failed to join thread group.", ex);
        }
    }

    public async Task LeaveThreadAsync(Guid threadId)
    {
        try
        {
            var group = GetThreadGroup(threadId);
            await this.Groups.RemoveFromGroupAsync(this.Context.ConnectionId, group);
            logger.LogInformation("Left group {Group} ConnectionId={ConnectionId}", group, this.Context.ConnectionId);
        }
        catch (Exception ex)
        {
            await this.Clients.Caller.ErrorOccurred(new SignalRErrorDto { Code = "SIGNALR_CONNECTION_ERROR", Message = "Failed to leave thread group.", Retryable = true, Metadata = new { threadId } });
            throw new SignalRConnectionException("Failed to leave thread group.", ex);
        }
    }

    public async Task SendTypingIndicatorAsync(Guid threadId, bool isTyping)
    {
        try
        {
            var userId = GetUserId();
            var userName = GetUserName();
            await this.BroadcastTypingIndicator(threadId, userId, userName, isTyping);
        }
        catch (Exception ex)
        {
            await this.Clients.Caller.ErrorOccurred(new SignalRErrorDto { Code = "SIGNALR_CONNECTION_ERROR", Message = "Failed to send typing indicator.", Retryable = true, Metadata = new { threadId, isTyping } });
            throw new SignalRConnectionException("Failed to send typing indicator.", ex);
        }
    }

    public async Task BroadcastTypingIndicator(Guid threadId, Guid? userId, string? userName, bool isTyping)
    {
        try
        {
            var group = GetThreadGroup(threadId);
            await this.Clients.Group(group).TypingIndicator(threadId, userId, userName, isTyping);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to broadcast typing indicator. ThreadId={ThreadId}, UserId={UserId}, IsTyping={IsTyping}", threadId, userId, isTyping);
            throw new SignalRConnectionException("Failed to broadcast typing indicator.", ex);
        }
    }

    private static string GetThreadGroup(Guid threadId) => $"thread:{threadId}";

    private Guid? GetUserId()
    {
        var id = this.Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(id, out var guid) ? guid : null;
    }

    private string? GetUserName()
    {
        return this.Context.User?.Identity?.Name;
    }

    private List<Guid> GetUserOrgIds()
    {
        var orgsClaim = this.Context.User?.FindFirst("orgs")?.Value;
        if (string.IsNullOrWhiteSpace(orgsClaim))
        {
            return new List<Guid>();
        }

        return orgsClaim.Split(',')
            .Select(s => Guid.TryParse(s.Trim(), out var g) ? g : Guid.Empty)
            .Where(g => g != Guid.Empty)
            .ToList();
    }
}
