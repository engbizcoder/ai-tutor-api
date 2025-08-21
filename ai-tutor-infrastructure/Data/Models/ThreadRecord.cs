// <copyright file="ThreadRecord.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ai.Tutor.Infrastructure.Data.Models;

using Ai.Tutor.Domain.Enums;
using Ai.Tutor.Infrastructure.Data.Interfaces;

public sealed class ThreadRecord : ICreatedAtEntity, IUpdatedAtEntity
{
    public Guid Id { get; set; }

    public Guid OrgId { get; set; }

    public Guid UserId { get; set; }

    public Guid? FolderId { get; set; }

    public string? Title { get; set; }

    public ChatThreadStatus Status { get; set; } = ChatThreadStatus.Active;

    public decimal SortOrder { get; set; } = 1000m;

    public string? MetadataJson { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}