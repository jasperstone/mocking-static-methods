using System;
using System.IO.Pipelines;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Http.Tests
{
    public class HttpResponseJsonExtensionsTests
    {
        [Fact]
        public async Task WriteAsJsonAsync_ShouldSetContentTypeAndSerialize()
        {
            // Arrange
            var mockResponse = new Mock<HttpResponse>();
            var mockHttpContext = new Mock<HttpContext>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockOptions = new Mock<IOptions<JsonOptions>>();

            mockResponse.Setup(r => r.HttpContext).Returns(mockHttpContext.Object);
            mockHttpContext.Setup(c => c.RequestServices).Returns(mockServiceProvider.Object);
            mockServiceProvider.Setup(s => s.GetService(typeof(IOptions<JsonOptions>))).Returns(mockOptions.Object);
            mockOptions.Setup(o => o.Value).Returns(new JsonOptions { SerializerOptions = new JsonSerializerOptions() });

            var value = new { Name = "Test" };
            var contentType = "application/json";
            var cancellationToken = new CancellationToken();

            // Act
            await HttpResponseJsonExtensions.WriteAsJsonAsync(mockResponse.Object, value, contentType, cancellationToken);

            // Assert
            mockResponse.VerifySet(r => r.ContentType = contentType);
            mockResponse.Verify(r => r.BodyWriter.WriteAsync(It.IsAny<ReadOnlyMemory<byte>>(), cancellationToken));
        }

        [Fact]
        public void ResolveSerializerOptions_ShouldReturnDefaultOptionsWhenServiceNotAvailable()
        {
            // Arrange
            var mockHttpContext = new Mock<HttpContext>();
            var mockServiceProvider = new Mock<IServiceProvider>();

            mockHttpContext.Setup(c => c.RequestServices).Returns(mockServiceProvider.Object);
            mockServiceProvider.Setup(s => s.GetService(typeof(IOptions<JsonOptions>))).Returns((IOptions<JsonOptions>)null);

            // Act
            var options = HttpResponseJsonExtensions.ResolveSerializerOptions(mockHttpContext.Object);

            // Assert
            Assert.Equal(JsonOptions.DefaultSerializerOptions, options);
        }

        [Fact]
        public void ResolveSerializerOptions_ShouldReturnServiceOptionsWhenAvailable()
        {
            // Arrange
            var mockHttpContext = new Mock<HttpContext>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockOptions = new Mock<IOptions<JsonOptions>>();

            mockHttpContext.Setup(c => c.RequestServices).Returns(mockServiceProvider.Object);
            mockServiceProvider.Setup(s => s.GetService(typeof(IOptions<JsonOptions>))).Returns(mockOptions.Object);
            mockOptions.Setup(o => o.Value).Returns(new JsonOptions { SerializerOptions = new JsonSerializerOptions() });

            // Act
            var options = HttpResponseJsonExtensions.ResolveSerializerOptions(mockHttpContext.Object);

            // Assert
            Assert.NotNull(options);
            Assert.Equal(mockOptions.Object.Value.SerializerOptions, options);
        }
    }
}
