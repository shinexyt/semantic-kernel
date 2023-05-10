﻿// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.AI.TextCompletion;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.AzureSdk;

namespace Microsoft.SemanticKernel.Connectors.AI.OpenAI.ChatCompletion;

/// <summary>
/// Azure OpenAI chat completion client.
/// TODO: forward ETW logging to ILogger, see https://learn.microsoft.com/en-us/dotnet/azure/sdk/logging
/// </summary>
public sealed class AzureChatCompletion : AzureOpenAIClientBase, IChatCompletion, ITextCompletion
{
    /// <summary>
    /// Create an instance of the Azure OpenAI chat completion connector with API key auth
    /// </summary>
    /// <param name="modelId">Azure OpenAI model ID or deployment name, see https://learn.microsoft.com/azure/cognitive-services/openai/how-to/create-resource</param>
    /// <param name="endpoint">Azure OpenAI deployment URL, see https://learn.microsoft.com/azure/cognitive-services/openai/quickstart</param>
    /// <param name="apiKey">Azure OpenAI API key, see https://learn.microsoft.com/azure/cognitive-services/openai/quickstart</param>
    /// <param name="httpClient">Custom <see cref="HttpClient"/> for HTTP requests.</param>
    /// <param name="logger">Application logger</param>
    public AzureChatCompletion(
        string modelId,
        string endpoint,
        string apiKey,
        HttpClient? httpClient = null,
        ILogger? logger = null) : base(modelId, endpoint, apiKey, httpClient, logger)
    {
    }

    /// <summary>
    /// Create an instance of the Azure OpenAI chat completion connector with AAD auth
    /// </summary>
    /// <param name="modelId">Azure OpenAI model ID or deployment name, see https://learn.microsoft.com/azure/cognitive-services/openai/how-to/create-resource</param>
    /// <param name="endpoint">Azure OpenAI deployment URL, see https://learn.microsoft.com/azure/cognitive-services/openai/quickstart</param>
    /// <param name="credentials">Token credentials, e.g. DefaultAzureCredential, ManagedIdentityCredential, EnvironmentCredential, etc.</param>
    /// <param name="httpClient">Custom <see cref="HttpClient"/> for HTTP requests.</param>
    /// <param name="logger">Application logger</param>
    public AzureChatCompletion(
        string modelId,
        string endpoint,
        TokenCredential credentials,
        HttpClient? httpClient = null,
        ILogger? logger = null) : base(modelId, endpoint, credentials, httpClient, logger)
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
    public IAsyncEnumerable<string> CompleteStreamAsync(string text,
        CompleteRequestSettings requestSettings,
        CancellationToken cancellationToken = default)
    {
        return this.InternalCompleteTextUsingChatStreamAsync(text, requestSettings, cancellationToken);
    }
}
