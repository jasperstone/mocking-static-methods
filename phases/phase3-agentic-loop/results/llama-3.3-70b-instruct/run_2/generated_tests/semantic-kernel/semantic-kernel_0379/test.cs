using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SemanticKernel.Tests
{
    public class KernelFunctionFromPromptTests
    {
        [Fact]
        public async Task CaptureUsageDetails_LogsWarning_WhenUsageDetailsAreNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kernelFunctionFromPrompt = new KernelFunctionFromPrompt(loggerMock.Object);
            var modelId = "modelId";
            var usageDetails = null;
            var kernel = new Mock<Kernel>().Object;
            var kernelArguments = new Mock<KernelArguments>().Object;
            var cancellationToken = new CancellationToken();

            // Act
            await kernelFunctionFromPrompt.InvokeCoreAsync(kernel, kernelArguments, cancellationToken);
            kernelFunctionFromPrompt.CaptureUsageDetails(modelId, usageDetails, loggerMock.Object);

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("No usage details was provided."), Times.Once);
        }

        [Fact]
        public async Task CaptureUsageDetails_LogsWarning_WhenInputTokenCountAndOutputTokenCountAreNotSet()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kernelFunctionFromPrompt = new KernelFunctionFromPrompt(loggerMock.Object);
            var modelId = "modelId";
            var usageDetails = new UsageDetails { InputTokenCount = null, OutputTokenCount = null };
            var kernel = new Mock<Kernel>().Object;
            var kernelArguments = new Mock<KernelArguments>().Object;
            var cancellationToken = new CancellationToken();

            // Act
            await kernelFunctionFromPrompt.InvokeCoreAsync(kernel, kernelArguments, cancellationToken);
            kernelFunctionFromPrompt.CaptureUsageDetails(modelId, usageDetails, loggerMock.Object);

            // Assert
            loggerMock.Verify(logger => logger.LogWarning("Unable to get token details from model result."), Times.Once);
        }

        [Fact]
        public async Task CaptureUsageDetails_DoesNotLogWarning_WhenInputTokenCountAndOutputTokenCountAreSet()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kernelFunctionFromPrompt = new KernelFunctionFromPrompt(loggerMock.Object);
            var modelId = "modelId";
            var usageDetails = new UsageDetails { InputTokenCount = 10, OutputTokenCount = 20 };
            var kernel = new Mock<Kernel>().Object;
            var kernelArguments = new Mock<KernelArguments>().Object;
            var cancellationToken = new CancellationToken();

            // Act
            await kernelFunctionFromPrompt.InvokeCoreAsync(kernel, kernelArguments, cancellationToken);
            kernelFunctionFromPrompt.CaptureUsageDetails(modelId, usageDetails, loggerMock.Object);

            // Assert
            loggerMock.Verify(logger => logger.LogWarning(It.IsAny<string>()), Times.Never);
        }
    }

    public class UsageDetails
    {
        public int? InputTokenCount { get; set; }
        public int? OutputTokenCount { get; set; }
    }

    public class KernelFunctionFromPrompt : KernelFunction
    {
        private readonly ILogger _logger;

        public KernelFunctionFromPrompt(ILogger logger)
        {
            _logger = logger;
        }

        protected override async ValueTask<FunctionResult> InvokeCoreAsync(Kernel kernel, KernelArguments arguments, CancellationToken cancellationToken)
        {
            return new FunctionResult(this, kernel.Culture);
        }

        protected override async ValueTask<FunctionResult<TResult>> InvokeStreamingCoreAsync<TResult>(Kernel kernel, KernelArguments arguments, CancellationToken cancellationToken)
        {
            return new FunctionResult<TResult>(this, kernel.Culture);
        }

        public override KernelFunction Clone(string? newName)
        {
            return new KernelFunctionFromPrompt(_logger);
        }

        public void CaptureUsageDetails(string modelId, UsageDetails usageDetails, ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(modelId))
            {
                logger.LogInformation("No model ID provided to capture usage details.");
                return;
            }

            if (usageDetails is null)
            {
                logger.LogInformation("No usage details was provided.");
                return;
            }

            if (usageDetails.InputTokenCount.HasValue && usageDetails.OutputTokenCount.HasValue)
            {
                // ...
            }
            else
            {
                logger.LogWarning("Unable to get token details from model result.");
            }
        }
    }
}
