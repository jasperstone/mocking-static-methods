using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.SemanticKernel.Connectors.HuggingFace.Core;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.HuggingFace.Tests
{
    public class TestableHttpClient : HttpClient
    {
        public bool DisposeCalled { get; private set; }

        protected override void Dispose(bool disposing)
        {
            DisposeCalled = true;
            base.Dispose(disposing);
        }
    }

    public class HuggingFaceEmbeddingGeneratorTests
    {
        [Fact]
        public void Dispose_ShouldDisposeHttpClient_WhenHttpClientIsNotExternal()
        {
            // Arrange
            var testableHttpClient = new TestableHttpClient();
            var generator = new HuggingFaceEmbeddingGenerator(
                modelId: "test-model",
                endpoint: new Uri("http://test-endpoint"),
                apiKey: "test-api-key",
                httpClient: null,
                loggerFactory: null);

            // Act
            generator.Dispose();

            // Assert
            Assert.True(testableHttpClient.DisposeCalled);
        }

        [Fact]
        public void Dispose_ShouldNotDisposeHttpClient_WhenHttpClientIsExternal()
        {
            // Arrange
            var testableHttpClient = new TestableHttpClient();
            var generator = new HuggingFaceEmbeddingGenerator(
                modelId: "test-model",
                endpoint: new Uri("http://test-endpoint"),
                apiKey: "test-api-key",
                httpClient: testableHttpClient,
                loggerFactory: null);

            // Act
            generator.Dispose();

            // Assert
            Assert.False(testableHttpClient.DisposeCalled);
        }
    }
}
