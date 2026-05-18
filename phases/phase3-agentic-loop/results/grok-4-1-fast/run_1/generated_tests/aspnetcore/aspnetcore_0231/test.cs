using System.Text.Json;
using System.Text.Json.Serialization;
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
        public void ResolveSerializerOptions_RequestServicesNull_ReturnsDefault()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = null;

            // Act
            var options = HttpResponseJsonExtensions.ResolveSerializerOptions(httpContext);

            // Assert
            Assert.Same(JsonOptions.DefaultSerializerOptions, options);
        }

        [Fact]
        public void ResolveSerializerOptions_NoIOptionsService_ReturnsDefault()
        {
            // Arrange
            var services = new ServiceCollection().BuildServiceProvider();
            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = services;

            // Act
            var options = HttpResponseJsonExtensions.ResolveSerializerOptions(httpContext);

            // Assert
            Assert.Same(JsonOptions.DefaultSerializerOptions, options);
        }

        [Fact]
        public void ResolveSerializerOptions_IOptionsNullSerializerOptions_ReturnsDefault()
        {
            // Arrange
            var jsonOptions = new JsonOptions();
            var services = new ServiceCollection();
            services.AddSingleton<IOptions<JsonOptions>>(Options.Create(jsonOptions));
            var serviceProvider = services.BuildServiceProvider();
            
            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = serviceProvider;

            // Act
            var options = HttpResponseJsonExtensions.ResolveSerializerOptions(httpContext);

            // Assert
            Assert.Same(JsonOptions.DefaultSerializerOptions, options);
        }

        [Fact]
        public void ResolveSerializerOptions_ValidCustomOptions_ReturnsCustomOptions()
        {
            // Arrange
            var customOptions = new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            };
            var jsonOptions = new JsonOptions { SerializerOptions = customOptions };
            
            var services = new ServiceCollection();
            services.AddSingleton<IOptions<JsonOptions>>(Options.Create(jsonOptions));
            var serviceProvider = services.BuildServiceProvider();
            
            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = serviceProvider;

            // Act
            var options = HttpResponseJsonExtensions.ResolveSerializerOptions(httpContext);

            // Assert
            Assert.Same(customOptions, options);
            Assert.Equal(JsonNamingPolicy.CamelCase, options.PropertyNamingPolicy);
        }
    }
}
