﻿// Copyright (c) Microsoft. All rights reserved.

using System.Text.Json.Serialization;

namespace Microsoft.SemanticKernel.Connectors.AI.Oobabooga.TextCompletion;

/// <summary>
/// HTTP Schema for streaming completion response. Adapted from <see href="https://github.com/oobabooga/text-generation-webui/blob/main/extensions/api/streaming_api.py"/>
/// </summary>
public sealed class TextCompletionStreamingResponse
{
    public const string ResponseObjectTextStreamEvent = "text_stream";
    public const string ResponseObjectStreamEndEvent = "stream_end";

    /// <summary>
    /// A field used by Oobabooga to signal the type of websocket message sent, e.g. "text_stream" or "stream_end".
    /// </summary>
    [JsonPropertyName("event")]
    public string Event { get; set; } = string.Empty;

    /// <summary>
    /// A field used by Oobabooga to signal the number of messages sent, starting with 0 and incremented on each message.
    /// </summary>
    [JsonPropertyName("message_num")]
    public int MessageNum { get; set; }

    /// <summary>
    /// A field used by Oobabooga with the text chunk sent in the websocket message.
    /// </summary>
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}
