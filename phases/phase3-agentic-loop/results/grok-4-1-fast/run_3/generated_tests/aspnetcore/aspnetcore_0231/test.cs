using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Microsoft.AspNetCore.Http.Extensions.Tests
{
    public class HttpResponseJsonExtensionsTests
    {
        [Fact]
        public void ResolveSerializerOptions_RequestServicesNull_ReturnsDefaultOptions()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = null;

            // Act
            var options = Microsoft.AspNetCore.Http.HttpResponseJsonExtensions.ResolveSerializerOptions(httpContext);

            // Assert
            Assert.Equal(JsonOptions.DefaultSerializerOptions, options);
        }

        [Fact]
        public void ResolveSerializerOptions_NoIOptionsService_ReturnsDefaultOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = services.BuildServiceProvider();

            // Act
            var options = Microsoft.AspNetCore.Http.HttpResponseJsonExtensions.ResolveSerializerOptions(httpContext);

            // Assert
            Assert.Equal(JsonOptions.DefaultSerializerOptions, options);
        }

        [Fact]
        public void ResolveSerializerOptions_IOptionsValueNull_ReturnsDefaultOptions()
        {
            // Arrange
            var jsonOptions = new JsonOptions();
            jsonOptions.SerializerOptions = null;
            var services = new ServiceCollection();
            services.AddSingleton<IOptions<JsonOptions>>(Options.Create(jsonOptions));
            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = services.BuildServiceProvider();

            // Act
            var result = Microsoft.AspNetCore.Http.HttpResponseJsonExtensions.ResolveSerializerOptions(httpContext);

            // Assert
            Assert.Equal(JsonOptions.DefaultSerializerOptions, result);
        }

        [Fact]
        public void ResolveSerializerOptions_ValidIOptions_ReturnsSerializerOptions()
        {
            // Arrange
            var expectedOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var jsonOptions = new JsonOptions();
            jsonOptions.SerializerOptions = expectedOptions;
            var services = new ServiceCollection();
            services.AddSingleton<IOptions<JsonOptions>>(Options.Create(jsonOptions));
            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = services.BuildServiceProvider();

            // Act
            var result = Microsoft.AspNetCore.Http.HttpResponseJsonExtensions.ResolveSerializerOptions(httpContext);

            // Assert
            Assert.Same(expectedOptions, result);
            Assert.Equal(JsonNamingPolicy.CamelCase, result.PropertyNamingPolicy);
        }
    }
}
