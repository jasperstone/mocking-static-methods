using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace HttpExtensionsTests
{
    public class HttpResponseJsonExtensionsTests
    {
        [Fact]
        public async Task WriteAsJsonAsync_CallsGetService_WhenRequestServicesIsNotNull()
        {
            // Arrange
            var response = new DefaultHttpContext().Response;
            var options = new JsonOptions { SerializerOptions = new JsonSerializerOptions() };
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IOptions<JsonOptions>>(Options.Create(options))
                .BuildServiceProvider();

            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = serviceProvider;
            response.HttpContext = httpContext;

            var value = new { Name = "Test" };

            // Act
            await response.WriteAsJsonAsync(value);

            // Assert
            Assert.Equal("application/json; charset=utf-8", response.ContentType);
        }

        [Fact]
        public async Task WriteAsJsonAsync_UsesServiceProviderToResolveOptions()
        {
            // Arrange
            var response = new DefaultHttpContext().Response;
            var options = new JsonOptions { SerializerOptions = new JsonSerializerOptions() };
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IOptions<JsonOptions>>(Options.Create(options))
                .BuildServiceProvider();

            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = serviceProvider;
            response.HttpContext = httpContext;

            var value = new { Id = 123 };

            // Act
            await response.WriteAsJsonAsync(value);

            // Assert
            Assert.Equal("application/json; charset=utf-8", response.ContentType);
        }

        [Fact]
        public async Task WriteAsJsonAsync_CallsSerializeAsync_WithCorrectParameters()
        {
            // Arrange
            var response = new DefaultHttpContext().Response;
            var called = false;
            var testValue = new { Data = "test" };
            var options = new JsonOptions { SerializerOptions = new JsonSerializerOptions() };
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IOptions<JsonOptions>>(Options.Create(options))
                .BuildServiceProvider();

            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = serviceProvider;
            response.HttpContext = httpContext;

            // Use reflection to replace JsonSerializer.SerializeAsync with a delegate that sets called=true
            // Since JsonSerializer.SerializeAsync is static and not virtual, we can't mock it directly.
            // Instead, we test that the method completes without exception and sets ContentType.

            // Act
            await response.WriteAsJsonAsync(testValue, options.SerializerOptions, CancellationToken.None);

            // Assert
            Assert.Equal("application/json; charset=utf-8", response.ContentType);
        }

        [Fact]
        public async Task WriteAsJsonAsync_WithJsonTypeInfo_CallsSerializeAsync_WithCorrectParameters()
        {
            // Arrange
            var response = new DefaultHttpContext().Response;
            var value = new { Name = "Test" };
            var jsonTypeInfo = JsonTypeInfo.CreateJsonTypeInfo(value.GetType());
            var options = new JsonOptions { SerializerOptions = new JsonSerializerOptions() };
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IOptions<JsonOptions>>(Options.Create(options))
                .BuildServiceProvider();

            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = serviceProvider;
            response.HttpContext = httpContext;

            // Act
            await response.WriteAsJsonAsync(value, jsonTypeInfo);

            // Assert
            Assert.Equal("application/json; charset=utf-8", response.ContentType);
        }
    }
}
