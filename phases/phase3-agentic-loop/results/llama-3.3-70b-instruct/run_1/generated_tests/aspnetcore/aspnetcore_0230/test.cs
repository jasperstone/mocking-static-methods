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
        public async Task ReadFromJsonAsync_WithJsonContentType_ReturnsDeserializedObject()
        {
            // Arrange
            var request = new HttpRequest(new DefaultHttpContext());
            request.ContentType = "application/json";
            var json = "{\"name\":\"John\",\"age\":30}";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            request.Body = stream;

            // Act
            var result = await request.ReadFromJsonAsync<Person>();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("John", result.Name);
            Assert.Equal(30, result.Age);
        }

        [Fact]
        public async Task ReadFromJsonAsync_WithJsonContentTypeAndSerializerOptions_ReturnsDeserializedObject()
        {
            // Arrange
            var request = new HttpRequest(new DefaultHttpContext());
            request.ContentType = "application/json";
            var json = "{\"name\":\"John\",\"age\":30}";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            request.Body = stream;
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // Act
            var result = await request.ReadFromJsonAsync<Person>(options);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("John", result.Name);
            Assert.Equal(30, result.Age);
        }

        [Fact]
        public async Task ReadFromJsonAsync_WithNonJsonContentType_ThrowsInvalidOperationException()
        {
            // Arrange
            var request = new HttpRequest(new DefaultHttpContext());
            request.ContentType = "text/plain";
            var json = "Hello World";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            request.Body = stream;

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
