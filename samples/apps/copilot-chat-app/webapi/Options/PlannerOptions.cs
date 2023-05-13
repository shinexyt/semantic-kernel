﻿// Copyright (c) Microsoft. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace SemanticKernel.Service.Config;

/// <summary>
/// Configuration options for the planner.
/// </summary>
public class PlannerOptions
{
    public const string PropertyName = "Planner";

    /// <summary>
    /// The AI service to use for planning.
    /// </summary>
    [Required]
    public AIServiceOptions? AIService { get; set; }

    /// <summary>
    /// Whether to enable the planner.
    /// </summary>
    public bool Enabled { get; set; } = false;
}
