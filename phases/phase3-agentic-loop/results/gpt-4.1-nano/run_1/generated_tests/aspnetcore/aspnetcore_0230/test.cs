using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HttpExtensionsTests
{
    public class HttpRequestJsonExtensionsTests
    {
        private class DummyService
        {
            public string Message { get; set; } = "Hello";
        }

        [Fact]
        public void ResolveSerializerOptions_ReturnsService_WhenAvailable()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions<JsonOptions>().Configure(o => o.SerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var provider = services.BuildServiceProvider();

            var context = new DefaultHttpContext();
            context.RequestServices = provider;

            // Act
            var options = HttpRequestJsonExtensions.ResolveSerializerOptions(context);

            // Assert
            Assert.NotNull(options);
            Assert.IsType<JsonSerializerOptions>(options);
        }

        [Fact]
        public void ResolveSerializerOptions_ReturnsDefault_WhenServiceNotAvailable()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.RequestServices = null; // No service provider

            // Act
            var options = HttpRequestJsonExtensions.ResolveSerializerOptions(context);

            // Assert
            Assert.NotNull(options);
            Assert.Equal(JsonOptions.DefaultSerializerOptions, options);
        }

        [Fact]
        public async Task ReadFromJsonAsync_ReturnsObject_WhenContentTypeIsJson()
        {
            // Arrange
            var json = "{\"Message\":\"Hello\"}";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var context = new DefaultHttpContext();
            context.Request.Body = stream;
            context.Request.ContentType = "application/json";

            var request = context.Request;

            // Act
            var result = await request.ReadFromJsonAsync<DummyService>();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Hello", result.Message);
        }

        [Fact]
        public async Task ReadFromJsonAsync_ReturnsDefaultSerializerOptions_WhenOptionsNull()
        {
            // Arrange
            var json = "{\"Message\":\"Hello\"}";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var context = new DefaultHttpContext();
            context.Request.Body = stream;
            context.Request.ContentType = "application/json";

            var request = context.Request;

            // Act
            var result = await request.ReadFromJsonAsync<DummyService>(null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Hello", result.Message);
        }

        [Fact]
        public async Task ReadFromJsonAsync_Throws_WhenContentTypeIsNotJson()
        {
            // Arrange
            var stream = new MemoryStream(Encoding.UTF8.GetBytes("not json"));
            var context = new DefaultHttpContext();
            context.Request.Body = stream;
            context.Request.ContentType = "text/plain";

            var request = context.Request;

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => request.ReadFromJsonAsync<DummyService>());
        }

        [Fact]
        public async Task ReadFromJsonAsync_UsesServiceProviderToGetOptions()
        {
            // Arrange
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var services = new ServiceCollection();
            services.AddOptions<JsonOptions>().Configure(o => o.SerializerOptions = options);
            var provider = services.BuildServiceProvider();

            var context = new DefaultHttpContext();
            context.RequestServices = provider;

            var json = "{\"Message\":\"Hello\"}";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            context.Request.Body = stream;
            context.Request.ContentType = "application/json";

            var request = context.Request;

            // Act
            var result = await request.ReadFromJsonAsync<DummyService>();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Hello", result.Message);
        }

        [Fact]
        public async Task ReadFromJsonAsync_WithTypeInfo_UsesCorrectDeserializeMethod()
        {
            // Arrange
            var json = "{\"Message\":\"Hello\"}";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var context = new DefaultHttpContext();
            context.Request.Body = stream;
            context.Request.ContentType = "application/json";

            var request = context.Request;

            var typeInfo = JsonTypeInfo.Create<DummyService>();

            // Act
            var result = await request.ReadFromJsonAsync(typeInfo);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Hello", result.Message);
        }
    }
}
