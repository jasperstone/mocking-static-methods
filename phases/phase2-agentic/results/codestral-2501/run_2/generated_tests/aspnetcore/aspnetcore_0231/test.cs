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

            mockHttpContext.Setup(c => c.RequestServices).Returns(mockServiceProvider.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IOptions<JsonOptions>))).Returns(mockOptions.Object);
            mockOptions.Setup(o => o.Value).Returns(new JsonOptions { SerializerOptions = new JsonSerializerOptions() });

            mockResponse.Setup(r => r.HttpContext).Returns(mockHttpContext.Object);
            mockResponse.Setup(r => r.BodyWriter).Returns(new PipeWriter(new PipeOptions()));

            var value = new { Name = "Test" };

            // Act
            await HttpResponseJsonExtensions.WriteAsJsonAsync(mockResponse.Object, value);

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

            mockHttpContext.Setup(c => c.RequestServices).Returns(mockServiceProvider.Object);
            mockResponse.Setup(r => r.HttpContext).Returns(mockHttpContext.Object);
            mockResponse.Setup(r => r.BodyWriter).Returns(new PipeWriter(new PipeOptions()));

            var value = new { Name = "Test" };
            var options = new JsonSerializerOptions();

            // Act
            await HttpResponseJsonExtensions.WriteAsJsonAsync(mockResponse.Object, value, options);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetService(typeof(IOptions<JsonOptions>)), Times.Never);
        }

        [Fact]
        public async Task WriteAsJsonAsync_ShouldUseRequestAbortedToken_WhenNoCancellationTokenProvided()
        {
            // Arrange
            var mockResponse = new Mock<HttpResponse>();
            var mockHttpContext = new Mock<HttpContext>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockOptions = new Mock<IOptions<JsonOptions>>();

            mockHttpContext.Setup(c => c.RequestServices).Returns(mockServiceProvider.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IOptions<JsonOptions>))).Returns(mockOptions.Object);
            mockOptions.Setup(o => o.Value).Returns(new JsonOptions { SerializerOptions = new JsonSerializerOptions() });

            mockResponse.Setup(r => r.HttpContext).Returns(mockHttpContext.Object);
            mockResponse.Setup(r => r.BodyWriter).Returns(new PipeWriter(new PipeOptions()));

            var value = new { Name = "Test" };
            var requestAbortedToken = new CancellationTokenSource().Token;
            mockHttpContext.Setup(c => c.RequestAborted).Returns(requestAbortedToken);

            // Act
            await HttpResponseJsonExtensions.WriteAsJsonAsync(mockResponse.Object, value);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetService(typeof(IOptions<JsonOptions>)), Times.Once);
        }
    }
}
