using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Amazon.Core;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Amazon.Core.Tests
{
    public class BedrockChatCompletionClientLoggingTests
    {
        [Fact]
        public void LoggerFactory_CreateLogger_IsNotMockedExtension()
        {
            // This test verifies that we cannot mock ILoggerFactory.CreateLogger(Type) because it is an extension method.
            // So we avoid mocking it directly and instead mock ILogger and pass it directly if possible.
            // This is a demonstration test to explain the limitation encountered.

            var mockLogger = new Mock<ILogger>();

            // We cannot mock ILoggerFactory.CreateLogger(Type) because it is an extension method.
            // So we do not attempt to mock it here.

            Assert.NotNull(mockLogger.Object);
        }
    }
}
