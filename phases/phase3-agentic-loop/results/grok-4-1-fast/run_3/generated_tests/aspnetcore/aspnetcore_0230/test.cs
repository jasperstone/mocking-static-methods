using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Microsoft.AspNetCore.Http.Extensions.Tests
{
    public class HttpRequestJsonExtensionsTests
    {
        [Fact]
        public void ResolveSerializerOptions_NullRequestServices_ReturnsDefaultOptions()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = null;

            // Act
            var options = Microsoft.AspNetCore.Http.HttpRequestJsonExtensions.ResolveSerializerOptions(httpContext);

            // Assert
            Assert.NotNull(options);
            Assert.Same(Microsoft.AspNetCore.Http.Json.JsonOptions.DefaultSerializerOptions, options);
        }

        [Fact]
        public void ResolveSerializerOptions_NoIOptionsRegistered_ReturnsDefaultOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = serviceProvider;

            // Act
            var options = Microsoft.AspNetCore.Http.HttpRequestJsonExtensions.ResolveSerializerOptions(httpContext);

            // Assert
            Assert.NotNull(options);
            Assert.Same(Microsoft.AspNetCore.Http.Json.JsonOptions.DefaultSerializerOptions, options);
        }

        [Fact]
        public void ResolveSerializerOptions_NullSerializerOptions_ReturnsDefaultOptions()
        {
            // Arrange
            var jsonOptions = new Microsoft.AspNetCore.Http.Json.JsonOptions { SerializerOptions = null };
            var services = new ServiceCollection();
            services.AddSingleton<IOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>>(Options.Create(jsonOptions));
            var serviceProvider = services.BuildServiceProvider();
            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = serviceProvider;

            // Act
            var options = Microsoft.AspNetCore.Http.HttpRequestJsonExtensions.ResolveSerializerOptions(httpContext);

            // Assert
            Assert.NotNull(options);
            Assert.Same(Microsoft.AspNetCore.Http.Json.JsonOptions.DefaultSerializerOptions, options);
        }

        [Fact]
        public void ResolveSerializerOptions_ValidConfiguredOptions_ReturnsConfiguredOptions()
        {
            // Arrange
            var expectedOptions = new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            };
            var jsonOptions = new Microsoft.AspNetCore.Http.Json.JsonOptions { SerializerOptions = expectedOptions };
            var services = new ServiceCollection();
            services.AddSingleton<IOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>>(Options.Create(jsonOptions));
            var serviceProvider = services.BuildServiceProvider();
            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = serviceProvider;

            // Act
            var options = Microsoft.AspNetCore.Http.HttpRequestJsonExtensions.ResolveSerializerOptions(httpContext);

            // Assert
            Assert.Same(expectedOptions, options);
            Assert.Equal(JsonNamingPolicy.CamelCase, options.PropertyNamingPolicy);
        }
    }
}
