using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Services;
using Moq;
using Xunit;

namespace SemanticKernel.Core.Tests
{
    public class KernelFunctionFromPromptTests
    {
        [Fact]
        public void CaptureUsageDetails_LogsWarning_WhenTokenDetailsAreMissing()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kernelFunction = new KernelFunctionFromPrompt(
                "testPrompt",
                new Dictionary<string, PromptExecutionSettings>(),
                "testFunction",
                "testDescription",
                null,
                null,
                null,
                Mock.Of<ILoggerFactory>());

            var usageDetails = new UsageDetails();

            // Act
            kernelFunction.CaptureUsageDetails("modelId", usageDetails, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unable to get token details from model result.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task GetChatCompletionResultAsync_ReturnsFunctionResult_WhenChatContentsAreEmpty()
        {
            // Arrange
            var chatCompletionMock = new Mock<IChatCompletionService>();
            var kernelMock = new Mock<Kernel>();
            var promptRenderingResult = new PromptRenderingResult("testRenderedPrompt", new Dictionary<string, PromptExecutionSettings>());
            var cancellationToken = new CancellationToken();

            chatCompletionMock.Setup(x => x.GetChatMessageContentsAsync(
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, PromptExecutionSettings>>(),
                It.IsAny<Kernel>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ChatMessageContent>());

            var kernelFunction = new KernelFunctionFromPrompt(
                "testPrompt",
                new Dictionary<string, PromptExecutionSettings>(),
                "testFunction",
                "testDescription",
                null,
                null,
                null,
                Mock.Of<ILoggerFactory>());

            // Act
            var result = await kernelFunction.GetChatCompletionResultAsync(
                chatCompletionMock.Object,
                kernelMock.Object,
                promptRenderingResult,
                cancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("testRenderedPrompt", result.RenderedPrompt);
        }
    }
}
