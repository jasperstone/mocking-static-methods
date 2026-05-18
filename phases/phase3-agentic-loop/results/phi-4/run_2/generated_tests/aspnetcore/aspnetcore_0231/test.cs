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
    public class HttpResponseJsonExtensionsTests
    {
        [Fact]
        public void ResolveSerializerOptions_WhenJsonOptionsIsRegistered_ReturnsRegisteredOptions()
        {
            // Arrange
            var jsonOptions = new JsonOptions();
            var serviceProvider = new Mock<IServiceProvider>();
            var optionsAccessor = Options.Create(jsonOptions);
            serviceProvider.Setup(s => s.GetService(typeof(IOptions<JsonOptions>)))
                .Returns(optionsAccessor);

            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = serviceProvider.Object;

            // Act
            var result = HttpResponseJsonExtensions.ResolveSerializerOptions(httpContext);

            // Assert
            Assert.Same(jsonOptions.SerializerOptions, result);
        }

        [Fact]
        public void ResolveSerializerOptions_WhenJsonOptionsIsNotRegistered_ReturnsDefaultOptions()
        {
            // Arrange
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(s => s.GetService(typeof(IOptions<JsonOptions>)))
                .Returns((IOptions<JsonOptions>)null);

            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = serviceProvider.Object;

            // Act
            var result = HttpResponseJsonExtensions.ResolveSerializerOptions(httpContext);

            // Assert
            Assert.Same(JsonOptions.DefaultSerializerOptions, result);
        }
    }
}
