using System;
using System.IO.Pipelines;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Http.Tests
{
    public class HttpResponseJsonExtensionsTests
    {
        [Fact]
        public void ResolveSerializerOptions_WhenServiceAvailable_ReturnsServiceOptions()
        {
            // Arrange
            var jsonOptions = new JsonOptions();
            var options = Options.Create(jsonOptions);
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(s => s.GetService(typeof(IOptions<JsonOptions>)))
                .Returns(options);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(c => c.RequestServices).Returns(serviceProviderMock.Object);

            // Act
            var result = HttpResponseJsonExtensions.ResolveSerializerOptions(httpContextMock.Object);

            // Assert
            Assert.Same(jsonOptions.SerializerOptions, result);
        }

        [Fact]
        public void ResolveSerializerOptions_WhenServiceNotAvailable_ReturnsDefaultOptions()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(s => s.GetService(typeof(IOptions<JsonOptions>)))
                .Returns((IOptions<JsonOptions>)null);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(c => c.RequestServices).Returns(serviceProviderMock.Object);

            // Act
            var result = HttpResponseJsonExtensions.ResolveSerializerOptions(httpContextMock.Object);

            // Assert
            Assert.Same(JsonOptions.DefaultSerializerOptions, result);
        }
    }
}
