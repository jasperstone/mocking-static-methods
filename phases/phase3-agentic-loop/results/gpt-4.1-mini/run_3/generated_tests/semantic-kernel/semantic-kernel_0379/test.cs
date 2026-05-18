using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;
using Xunit;

namespace SemanticKernel.Core.Tests.Functions
{
    public class KernelFunctionFromPromptTests
    {
        // We cannot subclass KernelFunctionFromPrompt because it is internal sealed,
        // and CaptureUsageDetails is private.
        // Instead, we test the public async method GetChatCompletionResultAsync which calls CaptureUsageDetails internally.
        // We will mock IChatCompletionService to return a chat content with missing token counts to trigger the LogWarning call.

        private class TestChatMessageContent : IChatMessageContent
        {
            public string ModelId { get; set; } = string.Empty;
            public IReadOnlyDictionary<string, object>? Metadata { get; set; }
        }

        private class TestPromptRenderingResult : PromptRenderingResult
        {
            public TestPromptRenderingResult(string renderedPrompt) : base(renderedPrompt, null) { }
        }

        [Fact]
        public async Task GetChatCompletionResultAsync_LogsWarning_WhenTokenDetailsMissing()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var chatCompletionMock = new Mock<IChatCompletionService>();
            var kernelMock = new Mock<Kernel>(MockBehavior.Loose, null, null, null);

            var function = CreateKernelFunctionFromPromptInstance(loggerMock.Object);

            var chatContent = new TestChatMessageContent
            {
                ModelId = "test-model",
                Metadata = new Dictionary<string, object>
                {
                    // Missing InputTokenCount and OutputTokenCount keys to trigger warning
                    { "SomeOtherKey", 123 }
                }
            };

            chatCompletionMock.Setup(c => c.GetChatMessageContentsAsync(
                It.IsAny<string>(),
                It.IsAny<PromptSettings>(),
                It.IsAny<Kernel>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<IChatMessageContent> { chatContent });

            var promptRenderingResult = new TestPromptRenderingResult("test prompt");

            // Act
            var result = await InvokeGetChatCompletionResultAsync(function, chatCompletionMock.Object, kernelMock.Object, promptRenderingResult);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Unable to get token details from model result."),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private static KernelFunctionFromPrompt CreateKernelFunctionFromPromptInstance(ILogger logger)
        {
            // Use reflection to create an instance of internal sealed KernelFunctionFromPrompt
            // and set the private _logger field to our mock logger.
            var type = typeof(KernelFunction).Assembly.GetType("Microsoft.SemanticKernel.KernelFunctionFromPrompt");
            if (type == null) throw new InvalidOperationException("Type KernelFunctionFromPrompt not found.");

            // Create instance using non-public constructor
            var ctor = type.GetConstructor(
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null,
                new Type[] { typeof(string), typeof(string), typeof(ILogger) },
                null);
            if (ctor == null) throw new InvalidOperationException("Constructor not found.");

            var instance = (KernelFunctionFromPrompt)ctor.Invoke(new object[] { "name", "description", logger });
            return instance;
        }

        private static Task<object> InvokeGetChatCompletionResultAsync(
            KernelFunctionFromPrompt function,
            IChatCompletionService chatCompletion,
            Kernel kernel,
            PromptRenderingResult promptRenderingResult)
        {
            // Use reflection to invoke private async method GetChatCompletionResultAsync
            var method = function.GetType().GetMethod("GetChatCompletionResultAsync",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (method == null) throw new InvalidOperationException("Method GetChatCompletionResultAsync not found.");

            var task = (Task)method.Invoke(function, new object[] { chatCompletion, kernel, promptRenderingResult, CancellationToken.None })!;
            return task.ContinueWith(t =>
            {
                var resultProperty = t.GetType().GetProperty("Result");
                return resultProperty!.GetValue(t);
            });
        }
    }
}
