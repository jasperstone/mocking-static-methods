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
        public async Task WriteAsJsonAsync_ShouldUseDefaultSerializerOptions_WhenOptionsNotProvided()
        {
            // Arrange
            var mockResponse = new Mock<HttpResponse>();
            var mockHttpContext = new Mock<HttpContext>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockOptions = new Mock<IOptions<JsonOptions>>();

            mockResponse.Setup(r => r.HttpContext).Returns(mockHttpContext.Object);
            mockHttpContext.Setup(c => c.RequestServices).Returns(mockServiceProvider.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IOptions<JsonOptions>))).Returns(mockOptions.Object);
            mockOptions.Setup(o => o.Value).Returns(new JsonOptions { SerializerOptions = new JsonSerializerOptions() });

            var value = new { Name = "Test" };
            var pipeWriter = PipeWriter.Create(new MemoryStream());

            mockResponse.Setup(r => r.BodyWriter).Returns(pipeWriter);

            // Act
            await HttpResponseJsonExtensions.WriteAsJsonAsync(mockResponse.Object, value, cancellationToken: default);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetService(typeof(IOptions<JsonOptions>)), Times.Once);
        }

        [Fact]
        public async Task WriteAsJsonAsync_ShouldUseProvidedSerializerOptions_WhenOptionsProvided()
        {
            // Arrange
            var mockResponse = new Mock<HttpResponse>();
            var mockHttpContext = new Mock<HttpContext>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockOptions = new Mock<IOptions<JsonOptions>>();

            mockResponse.Setup(r => r.HttpContext).Returns(mockHttpContext.Object);
            mockHttpContext.Setup(c => c.RequestServices).Returns(mockServiceProvider.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IOptions<JsonOptions>))).Returns(mockOptions.Object);
            mockOptions.Setup(o => o.Value).Returns(new JsonOptions { SerializerOptions = new JsonSerializerOptions() });

            var value = new { Name = "Test" };
            var pipeWriter = PipeWriter.Create(new MemoryStream());

            mockResponse.Setup(r => r.BodyWriter).Returns(pipeWriter);

            var providedOptions = new JsonSerializerOptions();

            // Act
            await HttpResponseJsonExtensions.WriteAsJsonAsync(mockResponse.Object, value, providedOptions, cancellationToken: default);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetService(typeof(IOptions<JsonOptions>)), Times.Never);
        }

        [Fact]
        public async Task WriteAsJsonAsync_ShouldUseDefaultSerializerOptions_WhenOptionsServiceNotAvailable()
        {
            // Arrange
            var mockResponse = new Mock<HttpResponse>();
            var mockHttpContext = new Mock<HttpContext>();
            var mockServiceProvider = new Mock<IServiceProvider>();

            mockResponse.Setup(r => r.HttpContext).Returns(mockHttpContext.Object);
            mockHttpContext.Setup(c => c.RequestServices).Returns(mockServiceProvider.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IOptions<JsonOptions>))).Returns(null);

            var value = new { Name = "Test" };
            var pipeWriter = PipeWriter.Create(new MemoryStream());

            mockResponse.Setup(r => r.BodyWriter).Returns(pipeWriter);

            // Act
            await HttpResponseJsonExtensions.WriteAsJsonAsync(mockResponse.Object, value, cancellationToken: default);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetService(typeof(IOptions<JsonOptions>)), Times.Once);
        }
    }
}
