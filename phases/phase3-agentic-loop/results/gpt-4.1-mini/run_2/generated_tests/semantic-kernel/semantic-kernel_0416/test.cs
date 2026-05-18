using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace SemanticKernel.UnitTests.TemplateEngine.Blocks
{
    public class VarBlockTests
    {
        private readonly Type _varBlockType;
        private readonly ConstructorInfo _varBlockCtor;

        public VarBlockTests()
        {
            var assembly = Assembly.Load("Microsoft.SemanticKernel.Core");
            _varBlockType = assembly.GetType("Microsoft.SemanticKernel.TemplateEngine.VarBlock") 
                ?? throw new InvalidOperationException("VarBlock type not found");
            _varBlockCtor = _varBlockType.GetConstructor(new Type[] { typeof(string), typeof(ILoggerFactory) }) 
                ?? throw new InvalidOperationException("VarBlock constructor not found");
        }

        [Fact]
        public void Constructor_LogsError_WhenContentLengthLessThan2()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            // Act
            var block = _varBlockCtor.Invoke(new object?[] { "a", loggerFactoryMock.Object });

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "The variable name is empty"),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void Constructor_SetsName_WhenContentLengthAtLeast2()
        {
            // Arrange
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(Mock.Of<ILogger>());

            // Act
            var block = _varBlockCtor.Invoke(new object?[] { "$name", loggerFactoryMock.Object });

            // Use reflection to get the Name property
            var nameProp = _varBlockType.GetProperty("Name", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                ?? throw new InvalidOperationException("Name property not found");

            var nameValue = nameProp.GetValue(block) as string;

            // Assert
            Assert.Equal("name", nameValue);
        }
    }
}
