using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.Experimental.Orchestration.Abstractions;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;

namespace UnitTests
{
    [TestClass]
    public class FlowExecutorTests
    {
        private Mock<ILogger> _loggerMock;
        private Mock<IKernelBuilder> _kernelBuilderMock;
        private Mock<IFlowStatusProvider> _flowStatusProviderMock;
        private Mock<Dictionary<object, string?>> _globalPluginCollectionMock;

        [TestInitialize]
        public void Initialize()
        {
            _loggerMock = new Mock<ILogger>();
            _kernelBuilderMock = new Mock<IKernelBuilder>();
            _flowStatusProviderMock = new Mock<IFlowStatusProvider>();
            _globalPluginCollectionMock = new Mock<Dictionary<object, string?>>();
        }

        [TestMethod]
        public async Task ExecuteFlowAsync_LogInformation_Called()
        {
            // Arrange
            var flowExecutor = new FlowExecutor(_kernelBuilderMock.Object, _flowStatusProviderMock.Object, _globalPluginCollectionMock.Object);
            var flow = new Flow();
            var sessionId = "sessionId";
            var input = "input";
            var kernelArguments = new KernelArguments();

            _loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

            // Act
            await flowExecutor.ExecuteFlowAsync(flow, sessionId, input, kernelArguments);

            // Assert
            _loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
        }

        [TestMethod]
        public async Task ExecuteFlowAsync_LogInformation_NotCalled()
        {
            // Arrange
            var flowExecutor = new FlowExecutor(_kernelBuilderMock.Object, _flowStatusProviderMock.Object, _globalPluginCollectionMock.Object);
            var flow = new Flow();
            var sessionId = "sessionId";
            var input = "input";
            var kernelArguments = new KernelArguments();

            _loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(false);

            // Act
            await flowExecutor.ExecuteFlowAsync(flow, sessionId, input, kernelArguments);

            // Assert
            _loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }
    }
}
