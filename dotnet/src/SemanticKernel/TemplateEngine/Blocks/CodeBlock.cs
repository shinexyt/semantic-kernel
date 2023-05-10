﻿// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;

namespace Microsoft.SemanticKernel.TemplateEngine.Blocks;

#pragma warning disable CA2254 // error strings are used also internally, not just for logging
// ReSharper disable TemplateIsNotCompileTimeConstantProblem
internal sealed class CodeBlock : Block, ICodeRendering
{
    internal override BlockTypes Type => BlockTypes.Code;

    public CodeBlock(string? content, ILogger log)
        : this(new CodeTokenizer(log).Tokenize(content), content?.Trim(), log)
    {
    }

    public CodeBlock(List<Block> tokens, string? content, ILogger log)
        : base(content?.Trim(), log)
    {
        this._tokens = tokens;
    }

    public override bool IsValid(out string errorMsg)
    {
        errorMsg = "";

        foreach (Block token in this._tokens)
        {
            if (!token.IsValid(out errorMsg))
            {
                this.Log.LogError(errorMsg);
                return false;
            }
        }

        if (this._tokens.Count > 1)
        {
            if (this._tokens[0].Type != BlockTypes.FunctionId)
            {
                errorMsg = $"Unexpected second token found: {this._tokens[1].Content}";
                this.Log.LogError(errorMsg);
                return false;
            }

            if (this._tokens[1].Type is not BlockTypes.Value and not BlockTypes.Variable)
            {
                errorMsg = "Functions support only one parameter";
                this.Log.LogError(errorMsg);
                return false;
            }
        }

        if (this._tokens.Count > 2)
        {
            errorMsg = $"Unexpected second token found: {this._tokens[1].Content}";
            this.Log.LogError(errorMsg);
            return false;
        }

        this._validated = true;

        return true;
    }

    public async Task<string> RenderCodeAsync(SKContext context)
    {
        if (!this._validated && !this.IsValid(out var error))
        {
            throw new TemplateException(TemplateException.ErrorCodes.SyntaxError, error);
        }

        this.Log.LogTrace("Rendering code: `{0}`", this.Content);

        switch (this._tokens[0].Type)
        {
            case BlockTypes.Value:
            case BlockTypes.Variable:
                return ((ITextRendering)this._tokens[0]).Render(context.Variables);

            case BlockTypes.FunctionId:
                return await this.RenderFunctionCallAsync((FunctionIdBlock)this._tokens[0], context).ConfigureAwait(false);
        }

        throw new TemplateException(TemplateException.ErrorCodes.UnexpectedBlockType,
            $"Unexpected first token type: {this._tokens[0].Type:G}");
    }

    #region private ================================================================================

    private bool _validated;
    private readonly List<Block> _tokens;

    private async Task<string> RenderFunctionCallAsync(FunctionIdBlock fBlock, SKContext context)
    {
        if (context.Skills == null)
        {
            throw new KernelException(
                KernelException.ErrorCodes.SkillCollectionNotSet,
                "Skill collection not found in the context");
        }

        if (!this.GetFunctionFromSkillCollection(context.Skills!, fBlock, out ISKFunction? function))
        {
            var errorMsg = $"Function `{fBlock.Content}` not found";
            this.Log.LogError(errorMsg);
            throw new TemplateException(TemplateException.ErrorCodes.FunctionNotFound, errorMsg);
        }

        ContextVariables variablesClone = context.Variables.Clone();

        // If the code syntax is {{functionName $varName}} use $varName instead of $input
        // If the code syntax is {{functionName 'value'}} use "value" instead of $input
        if (this._tokens.Count > 1)
        {
            // TODO: PII
            this.Log.LogTrace("Passing variable/value: `{0}`", this._tokens[1].Content);
            string input = ((ITextRendering)this._tokens[1]).Render(variablesClone);
            variablesClone.Update(input);
        }

        SKContext result = await function.InvokeWithCustomInputAsync(
            variablesClone,
            context.Memory,
            context.Skills,
            this.Log,
            context.CancellationToken).ConfigureAwait(false);

        if (result.ErrorOccurred)
        {
            var errorMsg = $"Function `{fBlock.Content}` execution failed. {result.LastException?.GetType().FullName}: {result.LastErrorDescription}";
            this.Log.LogError(errorMsg);
            throw new TemplateException(TemplateException.ErrorCodes.RuntimeError, errorMsg, result.LastException);
        }

        return result.Result;
    }

    private bool GetFunctionFromSkillCollection(
        IReadOnlySkillCollection skills,
        FunctionIdBlock fBlock,
        [NotNullWhen(true)] out ISKFunction? function)
    {
        if (string.IsNullOrEmpty(fBlock.SkillName))
        {
            // Function in the global skill
            return skills.TryGetFunction(fBlock.FunctionName, out function);
        }
        else
        {
            // Function within a specific skill
            return skills.TryGetFunction(fBlock.SkillName, fBlock.FunctionName, out function);
        }
    }

    #endregion
}
// ReSharper restore TemplateIsNotCompileTimeConstantProblem
#pragma warning restore CA2254
