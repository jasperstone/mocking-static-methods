using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.SemanticKernel.TemplateEngine;
using System;

namespace SemanticKernel.Tests
{
    public class VarBlockTests
    {
        [Fact]
        public void Constructor_ShouldLogError_WhenContentIsTooShort()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<VarBlock>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger<VarBlock>()).Returns(loggerMock.Object);

            // Act
            var varBlock = new VarBlock(null, loggerFactoryMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.LogError("The variable name is empty"),
                Times.Once);
        }
    }
}
