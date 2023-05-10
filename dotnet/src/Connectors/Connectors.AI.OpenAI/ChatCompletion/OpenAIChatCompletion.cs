﻿// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.AI.TextCompletion;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.AzureSdk;

namespace Microsoft.SemanticKernel.Connectors.AI.OpenAI.ChatCompletion;

/// <summary>
/// OpenAI chat completion client.
/// TODO: forward ETW logging to ILogger, see https://learn.microsoft.com/en-us/dotnet/azure/sdk/logging
/// </summary>
public sealed class OpenAIChatCompletion : OpenAIClientBase, IChatCompletion, ITextCompletion
{
    /// <summary>
    /// Create an instance of the OpenAI chat completion connector
    /// </summary>
    /// <param name="modelId">Model name</param>
    /// <param name="apiKey">OpenAI API Key</param>
    /// <param name="organization">OpenAI Organization Id (usually optional)</param>
    /// <param name="httpClient">Custom <see cref="HttpClient"/> for HTTP requests.</param>
    /// <param name="logger">Application logger</param>
    public OpenAIChatCompletion(
        string modelId,
        string apiKey,
        string? organization = null,
        HttpClient? httpClient = null,
        ILogger? logger = null
    ) : base(modelId, apiKey, organization, httpClient, logger)
    {
    }

    /// <inheritdoc/>
    public Task<string> GenerateMessageAsync(
        ChatHistory chat,
        ChatRequestSettings? requestSettings = null,
        CancellationToken cancellationToken = default)
    {
        return this.InternalGenerateChatMessageAsync(chat, requestSettings ?? new(), cancellationToken);
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<string> GenerateMessageStreamAsync(
        ChatHistory chat,
        ChatRequestSettings? requestSettings = null,
        CancellationToken cancellationToken = default)
    {
        return this.InternalGenerateChatMessageStreamAsync(chat, requestSettings ?? new(), cancellationToken);
    }

    /// <inheritdoc/>
    public ChatHistory CreateNewChat(string instructions = "")
    {
        return InternalCreateNewChat(instructions);
    }

    /// <inheritdoc/>
    public Task<string> CompleteAsync(
        string text,
        CompleteRequestSettings requestSettings,
        CancellationToken cancellationToken = default)
    {
        return this.InternalCompleteTextUsingChatAsync(text, requestSettings, cancellationToken);
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<string> CompleteStreamAsync(
        string text,
        CompleteRequestSettings requestSettings,
        CancellationToken cancellationToken = default)
    {
        return this.InternalCompleteTextUsingChatStreamAsync(text, requestSettings, cancellationToken);
    }
}
