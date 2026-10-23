using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;

namespace ReActEngineTests
{
    public class ReActEngineUnitTests
    {
        [Fact]
        public async Task GetNextStepAsync_Should_LogDebug_When_SingleParameterlessFunctionExists()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(x => x.IsEnabled(LogLevel.Debug)).Returns(true);
            var kernelMock = new Mock<Kernel>();
            var reActEngine = new TestReActEngine(loggerMock.Object);

            // Setup a dummy function with no parameters
            var function = new Mock<KernelFunction>();
            function.Setup(f => f.Parameters).Returns(new List<KernelFunctionParameter>());
            function.Setup(f => f.PluginName).Returns("TestPlugin");
            function.Setup(f => f.Name).Returns("TestFunction");
            var functions = new List<KernelFunction> { function.Object };

            // Inject the available functions
            reActEngine.AvailableFunctions = functions;

            // Act
            var result = await reActEngine.GetNextStepAsync(kernelMock.Object, new KernelArguments(), "What is the weather?", new List<ReActStep>());

            // Assert
            loggerMock.Verify(x => x.Log(LogLevel.Debug, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Auto selecting")), null, It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
            Assert.NotNull(result);
            Assert.IsType<ReActStep>(result);
            Assert.Equal("TestPlugin.TestFunction", result.Action);
        }

        // Helper subclass to expose internal methods and allow setting available functions
        private class TestReActEngine : ReActEngine
        {
            public List<KernelFunction> AvailableFunctions { get; set; } = new List<KernelFunction>();

            public TestReActEngine(ILogger logger) : base(null, logger, new FlowOrchestratorConfig()) { }

            protected override IEnumerable<KernelFunction> GetAvailableFunctions(Kernel kernel)
            {
                return AvailableFunctions;
            }
        }
    }
}
