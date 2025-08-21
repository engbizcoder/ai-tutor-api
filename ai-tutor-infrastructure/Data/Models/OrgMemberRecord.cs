// <copyright file="OrgMemberRecord.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ai.Tutor.Infrastructure.Data.Models;

using Ai.Tutor.Domain.Enums;

public sealed class OrgMemberRecord
{
    public Guid OrgId { get; set; }

    public Guid UserId { get; set; }

    public OrgRole Role { get; set; }

    public DateTime JoinedAt { get; set; }
}