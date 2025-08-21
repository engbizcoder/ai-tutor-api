// <copyright file="IUpdatedAtEntity.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ai.Tutor.Infrastructure.Data.Interfaces;

public interface IUpdatedAtEntity
{
    DateTime? UpdatedAt { get; set; }
}