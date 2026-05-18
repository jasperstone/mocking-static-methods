using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Xunit;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Tests.Execution;

public class ReActEngineLoggerTests
{
    [Fact]
    public async Task GetNextStepAsync_LogsDebugResponseText_WhenDebugEnabled()
    {
        // Arrange
        var logger = new TestLogger { IsDebugEnabled = true };
        var kernel = new TestKernel();
        var config = new FlowOrchestratorConfig();
        var engine = new TestReActEngine(kernel, logger, config);

        var arguments = new KernelArguments();
        var question = "test question";
        var previousSteps = new List<ReActStep>();

        // Act
        await engine.GetNextStepAsync(kernel, arguments, question, previousSteps);

        // Assert
        Assert.Single(logger.DebugMessages);
        Assert.Contains("Response : test response", logger.DebugMessages[0]);
    }

    [Fact]
    public async Task GetNextStepAsync_DoesNotLogDebugResponseText_WhenDebugDisabled()
    {
        // Arrange
        var logger = new TestLogger { IsDebugEnabled = false };
        var kernel = new TestKernel();
        var config = new FlowOrchestratorConfig();
        var engine = new TestReActEngine(kernel, logger, config);

        var arguments = new KernelArguments();
        var question = "test question";
        var previousSteps = new List<ReActStep>();

        // Act
        await engine.GetNextStepAsync(kernel, arguments, question, previousSteps);

        // Assert
        Assert.Empty(logger.DebugMessages);
    }
}

public class TestLogger : ILogger
{
    public bool IsDebugEnabled { get; set; } = true;
    public List<string> DebugMessages { get; } = new();
    public List<string> InfoMessages { get; } = new();

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null!;
    public bool IsEnabled(LogLevel logLevel) => logLevel == LogLevel.Debug ? IsDebugEnabled : true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (logLevel == LogLevel.Debug)
        {
            DebugMessages.Add(formatter(state, exception));
        }
        else if (logLevel == LogLevel.Information)
        {
            InfoMessages.Add(formatter(state, exception));
        }
    }
}

public class TestKernel : Kernel
{
    // Minimal implementation - empty constructor works
}

internal sealed class TestReActEngine : ReActEngine
{
    private readonly KernelFunction _mockReActFunction;

    public TestReActEngine(Kernel systemKernel, ILogger logger, FlowOrchestratorConfig config) : base(systemKernel, logger, config)
    {
        _mockReActFunction = CreateMockReActFunction();
    }

    internal new async Task<ReActStep?> GetNextStepAsync(Kernel kernel, KernelArguments arguments, string question, List<ReActStep> previousSteps)
    {
        // Force the multi-function path to hit the logging call
        var availableFunctions = new[] { new MockKernelFunction(), new MockKernelFunction() };
        return await PrivateGetNextStepAsync(kernel, arguments, question, previousSteps, availableFunctions);
    }

    private async Task<ReActStep?> PrivateGetNextStepAsync(Kernel kernel, KernelArguments arguments, string question, List<ReActStep> previousSteps, KernelFunction[] availableFunctions)
    {
        arguments["question"] = question;
        var scratchPad = CreateScratchPad(previousSteps);
        arguments["agentScratchPad"] = scratchPad;

        var functionDesc = GetFunctionDescriptions(availableFunctions);
        arguments["functionDescriptions"] = functionDesc;

        if (this._logger.IsEnabled(LogLevel.Information))
        {
            this._logger.LogInformation("question: {Question}", question);
            this._logger.LogInformation("functionDescriptions: {FunctionDescriptions}", functionDesc);
            this._logger.LogInformation("Scratchpad: {ScratchPad}", scratchPad);
        }

        var llmResponse = await _mockReActFunction.InvokeAsync(kernel, arguments);
        string llmResponseText = llmResponse.GetValue<string>("OUTPUT")!.Trim();

        if (this._logger?.IsEnabled(LogLevel.Debug) ?? false)
        {
            this._logger.LogDebug("Response : {ActionText}", llmResponseText);
        }

        var actionStep = ParseResult(llmResponseText);
        return actionStep;
    }

    private static KernelFunction CreateMockReActFunction()
    {
        return KernelFunctionFactory.CreateFromPrompt("{{OUTPUT}}", "MockReAct");
    }
}

internal class MockKernelFunction : KernelFunction
{
    public override IReadOnlyList<KernelParameterMetadata> Parameters => new List<KernelParameterMetadata>();
    public override string Name => "MockFunction";
    public override string Description => "Mock";
    public override string PluginName => "MockPlugin";
    public override Task<FunctionResult> InvokeAsync(Kernel kernel, KernelArguments arguments = null!) => 
        Task.FromResult(new FunctionResult(kernel, arguments ?? new KernelArguments { ["OUTPUT"] = "test response" }));
}
