using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNetCore.Http.Tests
{
    public class HttpRequestJsonExtensionsTests
    {
        [Fact]
        public async Task ResolveSerializerOptions_GetService_ReturnsSerializerOptions()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddOptions<JsonOptions>()
                .Configure<JsonOptions>(options =>
                {
                    options.SerializerOptions = new JsonSerializerOptions();
                })
                .BuildServiceProvider();

            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = serviceProvider;

            // Act
            var serializerOptions = HttpRequestJsonExtensions.ResolveSerializerOptions(httpContext);

            // Assert
            Assert.NotNull(serializerOptions);
        }

        [Fact]
        public async Task ResolveSerializerOptions_GetService_ReturnsDefaultSerializerOptions_WhenServiceNotFound()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .BuildServiceProvider();

            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = serviceProvider;

            // Act
            var serializerOptions = HttpRequestJsonExtensions.ResolveSerializerOptions(httpContext);

            // Assert
            Assert.NotNull(serializerOptions);
            Assert.Same(JsonOptions.DefaultSerializerOptions, serializerOptions);
        }

        [Fact]
        public async Task ReadFromJsonAsync_ContentTypeJson_ReturnsDeserializedObject()
        {
            // Arrange
            var json = "{\"name\":\"John\",\"age\":30}";
            var request = new DefaultHttpRequest(new DefaultHttpContext());
            request.ContentType = "application/json";
            request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));

            // Act
            var result = await request.ReadFromJsonAsync<Person>();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("John", result.Name);
            Assert.Equal(30, result.Age);
        }

        [Fact]
        public async Task ReadFromJsonAsync_ContentTypeNotJson_ThrowsInvalidOperationException()
        {
            // Arrange
            var json = "{\"name\":\"John\",\"age\":30}";
            var request = new DefaultHttpRequest(new DefaultHttpContext());
            request.ContentType = "text/plain";
            request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));

            // Act and Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => request.ReadFromJsonAsync<Person>());
        }

        private class Person
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }
    }
}
