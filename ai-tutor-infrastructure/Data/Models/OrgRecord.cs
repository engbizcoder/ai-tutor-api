// <copyright file="OrgRecord.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ai.Tutor.Infrastructure.Data.Models;

using Domain.Enums;
using Interfaces;

public sealed class OrgRecord : ICreatedAtEntity, IUpdatedAtEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public OrgType Type { get; set; }

    public string? MetadataJson { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}