﻿// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using KernelHttpServer.Config;
using KernelHttpServer.Utils;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;
using static KernelHttpServer.Config.Constants;

namespace KernelHttpServer;

internal static class SemanticKernelFactory
{
    internal static IKernel? CreateForRequest(
        HttpRequestData req,
        ILogger logger,
        IEnumerable<string>? skillsToLoad = null,
        IMemoryStore? memoryStore = null)
    {
        var apiConfig = req.ToApiKeyConfig();

        // must have a completion service
        if (!apiConfig.CompletionConfig.IsValid())
        {
            logger.LogError("Text completion service has not been supplied");
            return null;
        }

        // Text embedding service is optional, don't fail if we were not given the config
        if (memoryStore != null &&
            !apiConfig.EmbeddingConfig.IsValid())
        {
            logger.LogWarning("Text embedding service has not been supplied");
        }

        KernelBuilder builder = Kernel.Builder;
        builder = _ConfigureKernelBuilder(apiConfig, builder, memoryStore);
        return _CompleteKernelSetup(req, builder, logger, skillsToLoad);
    }

    private static KernelBuilder _ConfigureKernelBuilder(ApiKeyConfig config, KernelBuilder builder, IMemoryStore? memoryStore)
    {
        switch (config.CompletionConfig.AIService)
        {
            case AIService.OpenAI:
                builder.WithOpenAITextCompletionService(
                    config.CompletionConfig.DeploymentOrModelId,
                    config.CompletionConfig.Key,
                    config.CompletionConfig.ServiceId);
                break;
            case AIService.AzureOpenAI:
                builder.WithAzureTextCompletionService(
                    config.CompletionConfig.DeploymentOrModelId,
                    config.CompletionConfig.Endpoint,
                    config.CompletionConfig.Key,
                    serviceId: config.CompletionConfig.ServiceId);
                break;
            default:
                break;
        }

        if (memoryStore != null && config.EmbeddingConfig.IsValid())
        {
            switch (config.EmbeddingConfig.AIService)
            {
                case AIService.OpenAI:
                    builder.WithOpenAITextEmbeddingGenerationService(
                        config.EmbeddingConfig.DeploymentOrModelId,
                        config.EmbeddingConfig.Key,
                        serviceId: config.EmbeddingConfig.ServiceId);
                    break;
                case AIService.AzureOpenAI:
                    builder.WithAzureTextEmbeddingGenerationService(
                        config.EmbeddingConfig.DeploymentOrModelId,
                        config.EmbeddingConfig.Endpoint,
                        config.EmbeddingConfig.Key,
                        serviceId: config.EmbeddingConfig.ServiceId);
                    break;
                default:
                    break;
            }

            builder.WithMemoryStorage(memoryStore);
        }

        return builder;
    }

    private static IKernel _CompleteKernelSetup(HttpRequestData req, KernelBuilder builder, ILogger logger, IEnumerable<string>? skillsToLoad = null)
    {
        IKernel kernel = builder.Build();

        kernel.RegisterSemanticSkills(RepoFiles.SampleSkillsPath(), logger, skillsToLoad);
        kernel.RegisterNativeSkills(skillsToLoad);

        if (req.Headers.TryGetValues(SKHttpHeaders.MSGraph, out var graphToken))
        {
            kernel.RegisterNativeGraphSkills(graphToken.First());
        }

        kernel.RegisterTextMemory();

        return kernel;
    }
}
