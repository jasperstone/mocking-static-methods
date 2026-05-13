using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.IO.Pipelines;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class HttpResponseJsonExtensionsTests
    {
        [Fact]
        public async Task ResolveSerializerOptions_ReturnsDefaultSerializerOptions_WhenNoOptionsAreConfigured()
        {
            // Arrange
            var httpContext = new Mock<HttpContext>();
            var serviceProvider = new Mock<IServiceProvider>();
            httpContext.Setup(h => h.RequestServices).Returns(serviceProvider.Object);

            // Act
            var serializerOptions = HttpResponseJsonExtensions.ResolveSerializerOptions(httpContext.Object);

            // Assert
            Assert.NotNull(serializerOptions);
            Assert.Equal(JsonOptions.DefaultSerializerOptions.Encoder, serializerOptions.Encoder);
        }

        [Fact]
        public async Task ResolveSerializerOptions_ReturnsConfiguredSerializerOptions_WhenOptionsAreConfigured()
        {
            // Arrange
            var httpContext = new Mock<HttpContext>();
            var serviceProvider = new Mock<IServiceProvider>();
            var jsonOptions = new JsonOptions();
            jsonOptions.SerializerOptions.PropertyNameCaseInsensitive = true;
            serviceProvider.Setup(s => s.GetService<IOptions<JsonOptions>>()).Returns(Options.Create(jsonOptions));
            httpContext.Setup(h => h.RequestServices).Returns(serviceProvider.Object);

            // Act
            var serializerOptions = HttpResponseJsonExtensions.ResolveSerializerOptions(httpContext.Object);

            // Assert
            Assert.NotNull(serializerOptions);
            Assert.True(serializerOptions.PropertyNameCaseInsensitive);
        }

        [Fact]
        public async Task WriteAsJsonAsync_WritesJsonValueToResponseStream()
        {
            // Arrange
            var response = new Mock<HttpResponse>();
            var bodyWriter = new Mock<PipeWriter>();
            response.Setup(r => r.BodyWriter).Returns(bodyWriter.Object);
            var value = new { Foo = "bar" };
            var cancellationToken = new CancellationToken();

            // Act
            await response.Object.WriteAsJsonAsync(value, cancellationToken: cancellationToken);

            // Assert
            bodyWriter.Verify(b => b.WriteAsync(It.IsAny<ReadOnlySequence<byte>>(), cancellationToken), Times.Once);
        }

        [Fact]
        public async Task WriteAsJsonAsync_WritesJsonValueToResponseStream_WithSpecifiedSerializerOptions()
        {
            // Arrange
            var response = new Mock<HttpResponse>();
            var bodyWriter = new Mock<PipeWriter>();
            response.Setup(r => r.BodyWriter).Returns(bodyWriter.Object);
            var value = new { Foo = "bar" };
            var serializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var cancellationToken = new CancellationToken();

            // Act
            await response.Object.WriteAsJsonAsync(value, serializerOptions, cancellationToken: cancellationToken);

            // Assert
            bodyWriter.Verify(b => b.WriteAsync(It.IsAny<ReadOnlySequence<byte>>(), cancellationToken), Times.Once);
        }

        [Fact]
        public async Task WriteAsJsonAsync_WritesJsonValueToResponseStream_WithSpecifiedContentType()
        {
            // Arrange
            var response = new Mock<HttpResponse>();
            var bodyWriter = new Mock<PipeWriter>();
            response.Setup(r => r.BodyWriter).Returns(bodyWriter.Object);
            var value = new { Foo = "bar" };
            var contentType = "application/json; charset=utf-16";
            var cancellationToken = new CancellationToken();

            // Act
            await response.Object.WriteAsJsonAsync(value, contentType: contentType, cancellationToken: cancellationToken);

            // Assert
            response.Verify(r => r.ContentType = contentType, Times.Once);
            bodyWriter.Verify(b => b.WriteAsync(It.IsAny<ReadOnlySequence<byte>>(), cancellationToken), Times.Once);
        }
    }
}
