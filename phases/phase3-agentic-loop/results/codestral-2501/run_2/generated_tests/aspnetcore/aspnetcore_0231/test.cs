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
        public async Task WriteAsJsonAsync_ShouldSetContentTypeAndSerializeValue()
        {
            // Arrange
            var mockResponse = new Mock<HttpResponse>();
            var mockHttpContext = new Mock<HttpContext>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockOptions = new Mock<IOptions<JsonOptions>>();
            var mockPipeWriter = new Mock<PipeWriter>();

            mockHttpContext.Setup(c => c.RequestServices).Returns(mockServiceProvider.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IOptions<JsonOptions>))).Returns(mockOptions.Object);
            mockOptions.Setup(o => o.Value).Returns(new JsonOptions { SerializerOptions = new JsonSerializerOptions() });
            mockResponse.Setup(r => r.HttpContext).Returns(mockHttpContext.Object);
            mockResponse.Setup(r => r.BodyWriter).Returns(mockPipeWriter.Object);

            var value = new { Name = "Test" };

            // Act
            await HttpResponseJsonExtensions.WriteAsJsonAsync(mockResponse.Object, value);

            // Assert
            mockResponse.VerifySet(r => r.ContentType = "application/json; charset=utf-8");
            mockPipeWriter.Verify(w => w.WriteAsync(It.IsAny<ReadOnlyMemory<byte>>(), It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task WriteAsJsonAsync_WithCancellationToken_ShouldSetContentTypeAndSerializeValue()
        {
            // Arrange
            var mockResponse = new Mock<HttpResponse>();
            var mockHttpContext = new Mock<HttpContext>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockOptions = new Mock<IOptions<JsonOptions>>();
            var mockPipeWriter = new Mock<PipeWriter>();

            mockHttpContext.Setup(c => c.RequestServices).Returns(mockServiceProvider.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IOptions<JsonOptions>))).Returns(mockOptions.Object);
            mockOptions.Setup(o => o.Value).Returns(new JsonOptions { SerializerOptions = new JsonSerializerOptions() });
            mockResponse.Setup(r => r.HttpContext).Returns(mockHttpContext.Object);
            mockResponse.Setup(r => r.BodyWriter).Returns(mockPipeWriter.Object);

            var value = new { Name = "Test" };
            var cancellationToken = new CancellationTokenSource().Token;

            // Act
            await HttpResponseJsonExtensions.WriteAsJsonAsync(mockResponse.Object, value, cancellationToken: cancellationToken);

            // Assert
            mockResponse.VerifySet(r => r.ContentType = "application/json; charset=utf-8");
            mockPipeWriter.Verify(w => w.WriteAsync(It.IsAny<ReadOnlyMemory<byte>>(), cancellationToken));
        }

        [Fact]
        public async Task WriteAsJsonAsync_WithOptions_ShouldSetContentTypeAndSerializeValue()
        {
            // Arrange
            var mockResponse = new Mock<HttpResponse>();
            var mockHttpContext = new Mock<HttpContext>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockOptions = new Mock<IOptions<JsonOptions>>();
            var mockPipeWriter = new Mock<PipeWriter>();

            mockHttpContext.Setup(c => c.RequestServices).Returns(mockServiceProvider.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IOptions<JsonOptions>))).Returns(mockOptions.Object);
            mockOptions.Setup(o => o.Value).Returns(new JsonOptions { SerializerOptions = new JsonSerializerOptions() });
            mockResponse.Setup(r => r.HttpContext).Returns(mockHttpContext.Object);
            mockResponse.Setup(r => r.BodyWriter).Returns(mockPipeWriter.Object);

            var value = new { Name = "Test" };
            var options = new JsonSerializerOptions();

            // Act
            await HttpResponseJsonExtensions.WriteAsJsonAsync(mockResponse.Object, value, options);

            // Assert
            mockResponse.VerifySet(r => r.ContentType = "application/json; charset=utf-8");
            mockPipeWriter.Verify(w => w.WriteAsync(It.IsAny<ReadOnlyMemory<byte>>(), It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task WriteAsJsonAsync_WithContentType_ShouldSetContentTypeAndSerializeValue()
        {
            // Arrange
            var mockResponse = new Mock<HttpResponse>();
            var mockHttpContext = new Mock<HttpContext>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockOptions = new Mock<IOptions<JsonOptions>>();
            var mockPipeWriter = new Mock<PipeWriter>();

            mockHttpContext.Setup(c => c.RequestServices).Returns(mockServiceProvider.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IOptions<JsonOptions>))).Returns(mockOptions.Object);
            mockOptions.Setup(o => o.Value).Returns(new JsonOptions { SerializerOptions = new JsonSerializerOptions() });
            mockResponse.Setup(r => r.HttpContext).Returns(mockHttpContext.Object);
            mockResponse.Setup(r => r.BodyWriter).Returns(mockPipeWriter.Object);

            var value = new { Name = "Test" };
            var contentType = "application/custom+json; charset=utf-8";

            // Act
            await HttpResponseJsonExtensions.WriteAsJsonAsync(mockResponse.Object, value, contentType: contentType);

            // Assert
            mockResponse.VerifySet(r => r.ContentType = contentType);
            mockPipeWriter.Verify(w => w.WriteAsync(It.IsAny<ReadOnlyMemory<byte>>(), It.IsAny<CancellationToken>()));
        }

        [Fact]
        public void ResolveSerializerOptions_ShouldReturnDefaultOptionsWhenServiceProviderIsNull()
        {
            // Arrange
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(c => c.RequestServices).Returns((IServiceProvider)null);

            // Act
            var options = HttpResponseJsonExtensions.ResolveSerializerOptions(mockHttpContext.Object);

            // Assert
            Assert.Equal(JsonOptions.DefaultSerializerOptions, options);
        }

        [Fact]
        public void ResolveSerializerOptions_ShouldReturnDefaultOptionsWhenOptionsServiceIsNull()
        {
            // Arrange
            var mockHttpContext = new Mock<HttpContext>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockHttpContext.Setup(c => c.RequestServices).Returns(mockServiceProvider.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IOptions<JsonOptions>))).Returns(null);

            // Act
            var options = HttpResponseJsonExtensions.ResolveSerializerOptions(mockHttpContext.Object);

            // Assert
            Assert.Equal(JsonOptions.DefaultSerializerOptions, options);
        }

        [Fact]
        public void ResolveSerializerOptions_ShouldReturnDefaultOptionsWhenOptionsValueIsNull()
        {
            // Arrange
            var mockHttpContext = new Mock<HttpContext>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockOptions = new Mock<IOptions<JsonOptions>>();
            mockHttpContext.Setup(c => c.RequestServices).Returns(mockServiceProvider.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IOptions<JsonOptions>))).Returns(mockOptions.Object);
            mockOptions.Setup(o => o.Value).Returns((JsonOptions)null);

            // Act
            var options = HttpResponseJsonExtensions.ResolveSerializerOptions(mockHttpContext.Object);

            // Assert
            Assert.Equal(JsonOptions.DefaultSerializerOptions, options);
        }

        [Fact]
        public void ResolveSerializerOptions_ShouldReturnSerializerOptionsFromServiceProvider()
        {
            // Arrange
            var mockHttpContext = new Mock<HttpContext>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockOptions = new Mock<IOptions<JsonOptions>>();
            var expectedOptions = new JsonOptions { SerializerOptions = new JsonSerializerOptions() };
            mockHttpContext.Setup(c => c.RequestServices).Returns(mockServiceProvider.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IOptions<JsonOptions>))).Returns(mockOptions.Object);
            mockOptions.Setup(o => o.Value).Returns(expectedOptions);

            // Act
            var options = HttpResponseJsonExtensions.ResolveSerializerOptions(mockHttpContext.Object);

            // Assert
            Assert.Equal(expectedOptions.SerializerOptions, options);
        }
    }
}
