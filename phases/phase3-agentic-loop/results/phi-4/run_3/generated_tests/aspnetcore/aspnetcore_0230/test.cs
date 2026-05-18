using System;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Http.Json; // Ensure this using directive is included
using Microsoft.AspNetCore.Http.Extensions; // Ensure this using directive is included if JsonOptions is in this namespace

namespace Microsoft.AspNetCore.Http.Tests
{
    public class HttpRequestJsonExtensionsTests
    {
        [Fact]
        public void ResolveSerializerOptions_WhenOptionsResolved_ReturnsResolvedOptions()
        {
            // Arrange
            var jsonOptions = new JsonOptions { SerializerOptions = new JsonSerializerOptions() };
            var optionsMock = new Mock<IOptions<JsonOptions>>();
            optionsMock.Setup(o => o.Value).Returns(jsonOptions);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(s => s.GetService(typeof(IOptions<JsonOptions>)))
                .Returns(optionsMock.Object);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(c => c.RequestServices).Returns(serviceProviderMock.Object);

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
            serviceProviderMock.Setup(s => s.GetService(typeof(IOptions<JsonOptions>)))
                .Returns((IOptions<JsonOptions>)null);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(c => c.RequestServices).Returns(serviceProviderMock.Object);

            // Act
            var result = HttpRequestJsonExtensions.ResolveSerializerOptions(httpContextMock.Object);

            // Assert
            Assert.Same(JsonOptions.DefaultSerializerOptions, result);
        }

        [Fact]
        public void ResolveSerializerOptions_WhenRequestServicesIsNull_ReturnsDefaultOptions()
        {
            // Arrange
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(c => c.RequestServices).Returns((IServiceProvider)null);

            // Act
            var result = HttpRequestJsonExtensions.ResolveSerializerOptions(httpContextMock.Object);

            // Assert
            Assert.Same(JsonOptions.DefaultSerializerOptions, result);
        }
    }
}
