﻿// Copyright (c) Microsoft. All rights reserved.

using System.Net.Http;
using System.Runtime.CompilerServices;
using Azure.AI.OpenAI;
using Azure.Core;
using Azure.Core.Pipeline;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Diagnostics;

namespace Microsoft.SemanticKernel.Connectors.AI.OpenAI.AzureSdk;

public abstract class OpenAIClientBase : ClientBase
{
    /// <summary>
    /// OpenAI / Azure OpenAI Client
    /// </summary>
    private protected override OpenAIClient Client { get; }

    /// <summary>
    /// Create an instance of the OpenAI connector
    /// </summary>
    /// <param name="modelId">Model name</param>
    /// <param name="apiKey">OpenAI API Key</param>
    /// <param name="organization">OpenAI Organization Id (usually optional)</param>
    /// <param name="httpClient">Custom <see cref="HttpClient"/> for HTTP requests.</param>
    /// <param name="logger">Application logger</param>
    private protected OpenAIClientBase(
        string modelId,
        string apiKey,
        string? organization = null,
        HttpClient? httpClient = null,
        ILogger? logger = null) : base(logger)
    {
        Verify.NotNullOrWhiteSpace(modelId);
        Verify.NotNullOrWhiteSpace(apiKey);

        this.ModelId = modelId;

        var options = GetClientOptions();
        if (httpClient != null)
        {
            options.Transport = new HttpClientTransport(httpClient);
        }

        if (!string.IsNullOrWhiteSpace(organization))
        {
            options.AddPolicy(new AddHeaderRequestPolicy("OpenAI-Organization", organization!), HttpPipelinePosition.PerCall);
        }

        this.Client = new OpenAIClient(apiKey, options);
    }

    /// <summary>
    /// Logs OpenAI action details.
    /// </summary>
    /// <param name="callerMemberName">Caller member name. Populated automatically by runtime.</param>
    private protected void LogActionDetails([CallerMemberName] string? callerMemberName = default)
    {
        this.Logger.LogInformation("Action: {Action}. OpenAI Model ID: {ModelId}.", callerMemberName, this.ModelId);
    }

    /// <summary>
    /// Options used by the OpenAI client, e.g. User Agent.
    /// </summary>
    private static OpenAIClientOptions GetClientOptions()
    {
        return new OpenAIClientOptions
        {
            Diagnostics =
            {
                IsTelemetryEnabled = Telemetry.IsTelemetryEnabled,
                ApplicationId = Telemetry.HttpUserAgent,
            }
        };
    }
}
