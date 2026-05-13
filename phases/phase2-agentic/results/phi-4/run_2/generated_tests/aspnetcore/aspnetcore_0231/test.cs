using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using System.Text.Json;

namespace Microsoft.AspNetCore.Http.Tests
{
    public class HttpResponseJsonExtensionsTests
    {
        [Fact]
        public void ResolveSerializerOptions_WhenJsonOptionsRegistered_ReturnsRegisteredOptions()
        {
            // Arrange
            var jsonOptions = new JsonOptions { SerializerOptions = new JsonSerializerOptions() };
            var serviceProvider = new Mock<IServiceProvider>();
            var optionsAccessor = Options.Create(jsonOptions);
            serviceProvider.Setup(s => s.GetService(typeof(IOptions<JsonOptions>)))
                .Returns(optionsAccessor);

            var httpContext = new DefaultHttpContext { RequestServices = serviceProvider.Object };

            // Act
            var result = HttpResponseJsonExtensions.ResolveSerializerOptions(httpContext);

            // Assert
            Assert.Same(jsonOptions.SerializerOptions, result);
        }

        [Fact]
        public void ResolveSerializerOptions_WhenJsonOptionsNotRegistered_ReturnsDefaultOptions()
        {
            // Arrange
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(s => s.GetService(typeof(IOptions<JsonOptions>)))
                .Returns((IOptions<JsonOptions>)null);

            var httpContext = new DefaultHttpContext { RequestServices = serviceProvider.Object };

            // Act
            var result = HttpResponseJsonExtensions.ResolveSerializerOptions(httpContext);

            // Assert
            Assert.Same(JsonOptions.DefaultSerializerOptions, result);
        }

        [Fact]
        public void ResolveSerializerOptions_WhenHttpContextIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            HttpContext httpContext = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => HttpResponseJsonExtensions.ResolveSerializerOptions(httpContext));
        }
    }
}
