using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Tests
{
    public class ReActEngineTests
    {
        [Fact]
        public async Task GetNextStepAsync_LogsDebugMessage_WhenLlmResponseIsReceived()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kernelMock = new Mock<Kernel>();
            var kernelArgumentsMock = new Mock<KernelArguments>();
            var reActEngine = new ReActEngine(kernelMock.Object, loggerMock.Object, null);

            // Act
            var llmResponse = new object(); // replaced KernelResponse with object
            var result = await reActEngine.GetNextStepAsync(kernelMock.Object, kernelArgumentsMock.Object, "Test question", new List<ReActStep>());

            // Assert
            loggerMock.Verify(l => l.LogDebug("Response : {ActionText}", It.IsAny<object>()), Times.Once);
        }
    }
}
