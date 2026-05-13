using System;
using System.IO.Pipelines;
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
        public void ResolveSerializerOptions_WhenOptionsProvided_ReturnsProvidedOptions()
        {
            // Arrange
            var options = new JsonOptions { SerializerOptions = new JsonSerializerOptions() };
            var optionsAccessorMock = new Mock<IOptions<JsonOptions>>();
            optionsAccessorMock.Setup(o => o.Value).Returns(options);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(s => s.GetService(typeof(IOptions<JsonOptions>)))
                .Returns(optionsAccessorMock.Object);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(c => c.RequestServices).Returns(serviceProviderMock.Object);

            // Act
            var result = HttpResponseJsonExtensions.ResolveSerializerOptions(httpContextMock.Object);

            // Assert
            Assert.Same(options.SerializerOptions, result);
        }

        [Fact]
        public void ResolveSerializerOptions_WhenOptionsNotProvided_ReturnsDefaultOptions()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(s => s.GetService(typeof(IOptions<JsonOptions>)))
                .Returns((IOptions<JsonOptions>)null);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(c => c.RequestServices).Returns(serviceProviderMock.Object);

            // Act
            var result = HttpResponseJsonExtensions.ResolveSerializerOptions(httpContextMock.Object);

            // Assert
            Assert.Same(JsonOptions.DefaultSerializerOptions, result);
        }
    }
}
