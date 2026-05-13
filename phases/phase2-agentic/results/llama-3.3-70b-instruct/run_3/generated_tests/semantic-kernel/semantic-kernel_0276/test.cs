using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.OpenApi;

namespace TestProject
{
    [TestClass]
    public class CopilotAgentPluginKernelExtensionsTests
    {
        [TestMethod]
        public async Task CreatePluginFromCopilotAgentPluginAsync_NoFunctionsInRuntimeObject_LogsWarning()
        {
            // Arrange
            var kernel = new Kernel();
            var pluginName = "TestPlugin";
            var filePath = "TestFilePath";
            var pluginParameters = new CopilotAgentPluginParameters();
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            kernel.LoggerFactory = loggerFactoryMock.Object;

            // Act
            await kernel.CreatePluginFromCopilotAgentPluginAsync(pluginName, filePath, pluginParameters).ConfigureAwait(false);

            // Assert
            loggerMock.Verify(x => x.LogWarning("No functions found in the runtime object."), Times.Once);
        }

        [TestMethod]
        public async Task CreatePluginFromCopilotAgentPluginAsync_NoApiDescriptionUrlInRuntimeObject_LogsWarning()
        {
            // Arrange
            var kernel = new Kernel();
            var pluginName = "TestPlugin";
            var filePath = "TestFilePath";
            var pluginParameters = new CopilotAgentPluginParameters();
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            kernel.LoggerFactory = loggerFactoryMock.Object;

            // Act
            await kernel.CreatePluginFromCopilotAgentPluginAsync(pluginName, filePath, pluginParameters).ConfigureAwait(false);

            // Assert
            loggerMock.Verify(x => x.LogWarning("No API description URL found in the runtime object."), Times.Once);
        }
    }
}
