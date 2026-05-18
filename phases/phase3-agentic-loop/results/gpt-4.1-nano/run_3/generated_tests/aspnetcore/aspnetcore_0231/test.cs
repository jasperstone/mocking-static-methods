using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace HttpResponseJsonExtensionsTests
{
    public class WriteAsJsonAsyncTests
    {
        [Fact]
        public async Task WriteAsJsonAsync_CallsGetService_ReturnsExpected()
        {
            // Arrange
            var response = new DefaultHttpContext().Response;
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IOptions<JsonOptions>>(Options.Create(new JsonOptions()))
                .BuildServiceProvider();

            response.HttpContext.RequestServices = serviceProvider;

            var value = new { Name = "Test" };

            // Act
            await response.WriteAsJsonAsync(value);

            // Assert
            Assert.Equal("application/json; charset=utf-8", response.ContentType);
        }

        [Fact]
        public async Task WriteAsJsonAsync_WithCustomOptions_SetsContentType()
        {
            // Arrange
            var response = new DefaultHttpContext().Response;
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IOptions<JsonOptions>>(Options.Create(new JsonOptions()))
                .BuildServiceProvider();

            response.HttpContext.RequestServices = serviceProvider;

            var value = new { Name = "Test" };
            var customOptions = new JsonSerializerOptions { WriteIndented = true };

            // Act
            await response.WriteAsJsonAsync(value, customOptions);

            // Assert
            Assert.Equal("application/json; charset=utf-8", response.ContentType);
        }

        [Fact]
        public async Task WriteAsJsonAsync_WithContentType_SetsContentType()
        {
            // Arrange
            var response = new DefaultHttpContext().Response;
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IOptions<JsonOptions>>(Options.Create(new JsonOptions()))
                .BuildServiceProvider();

            response.HttpContext.RequestServices = serviceProvider;

            var value = new { Name = "Test" };
            var customContentType = "application/custom";

            // Act
            await response.WriteAsJsonAsync(value, null, customContentType);

            // Assert
            Assert.Equal(customContentType, response.ContentType);
        }

        [Fact]
        public async Task WriteAsJsonAsync_CancellationToken_CallsSerializeAsyncWithToken()
        {
            // Arrange
            var response = new DefaultHttpContext().Response;
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IOptions<JsonOptions>>(Options.Create(new JsonOptions()))
                .BuildServiceProvider();

            response.HttpContext.RequestServices = serviceProvider;

            var value = new { Name = "Test" };
            var cts = new CancellationTokenSource();

            // Act
            await response.WriteAsJsonAsync(value, cts.Token);

            // Assert
            Assert.Equal("application/json; charset=utf-8", response.ContentType);
        }

        [Fact]
        public async Task WriteAsJsonAsync_ResponseWithoutCancellableToken_CallsSlowMethod()
        {
            // Arrange
            var response = new DefaultHttpContext().Response;
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IOptions<JsonOptions>>(Options.Create(new JsonOptions()))
                .BuildServiceProvider();

            response.HttpContext.RequestServices = serviceProvider;

            var value = new { Name = "Test" };
            var nonCancellableToken = CancellationToken.None;

            // Act
            await response.WriteAsJsonAsync(value, nonCancellableToken);

            // Assert
            Assert.Equal("application/json; charset=utf-8", response.ContentType);
        }

        [Fact]
        public async Task WriteAsJsonAsync_WithJsonTypeInfo_SetsContentTypeAndSerializes()
        {
            // Arrange
            var response = new DefaultHttpContext().Response;
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IOptions<JsonOptions>>(Options.Create(new JsonOptions()))
                .BuildServiceProvider();

            response.HttpContext.RequestServices = serviceProvider;

            var value = new { Name = "Test" };
            var jsonTypeInfo = JsonSerializer.GetTypeInfo(value.GetType());

            // Act
            await response.WriteAsJsonAsync(value, jsonTypeInfo);

            // Assert
            Assert.Equal("application/json; charset=utf-8", response.ContentType);
        }
    }
}
