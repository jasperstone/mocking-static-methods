using System;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Http.Json; // Ensure this namespace is included

namespace Microsoft.AspNetCore.Http.Tests
{
    public class HttpRequestJsonExtensionsTests
    {
        [Fact]
        public void ResolveSerializerOptions_WhenOptionsResolved_ReturnsResolvedOptions()
        {
            // Arrange
            var jsonOptions = new JsonOptions();
            var options = Options.Create(jsonOptions);
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(s => s.GetService(typeof(IOptions<JsonOptions>)))
                .Returns(options);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(c => c.RequestServices).Returns(serviceProviderMock.Object);

            // Act
            var result = HttpRequestJsonExtensions.ResolveSerializerOptions(httpContextMock.Object);

            // Assert
            Assert.Same(jsonOptions.SerializerOptions, result);
        }

        [Fact]
        public void ResolveSerializerOptions_WhenOptionsNotResolved_ReturnsDefaultOptions()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(s => s.GetService(typeof(IOptions<JsonOptions>)))
                .Returns((IOptions<JsonOptions>)null);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(c => c.RequestServices).Returns(serviceProviderMock.Object);

            // Act
            var result = HttpRequestJsonExtensions.ResolveSerializerOptions(httpContextMock.Object);

            // Assert
            Assert.Same(JsonOptions.DefaultSerializerOptions, result);
        }
    }
}
