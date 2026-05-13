using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Text;

namespace Microsoft.AspNetCore.Http.Tests
{
    public class HttpRequestJsonExtensionsTests
    {
        [Fact]
        public async Task ReadFromJsonAsync_ValidJsonContentType_ReturnsDeserializedObject()
        {
            // Arrange
            var json = "{\"name\":\"John\"}";
            var request = new Mock<HttpRequest>();
            var httpContext = new Mock<HttpContext>();
            var serviceProvider = new Mock<IServiceProvider>();
            var options = new JsonSerializerOptions();
            var jsonTypeInfo = JsonSerializerOptions.Default.GetTypeInfo(typeof(object));

            request.Setup(r => r.ContentType).Returns("application/json");
            request.Setup(r => r.Body).Returns(new MemoryStream(Encoding.UTF8.GetBytes(json)));
            request.Setup(r => r.HttpContext).Returns(httpContext.Object);
            httpContext.Setup(c => c.RequestServices).Returns(serviceProvider.Object);
            serviceProvider.Setup(sp => sp.GetService(typeof(IOptions<JsonOptions>))).Returns(new OptionsWrapper<JsonOptions>(new JsonOptions { SerializerOptions = options }));

            // Act
            var result = await request.Object.ReadFromJsonAsync(jsonTypeInfo, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<JsonElement>(result);
            var jsonElement = (JsonElement)result;
            Assert.Equal("John", jsonElement.GetProperty("name").GetString());
        }

        [Fact]
        public async Task ReadFromJsonAsync_InvalidJsonContentType_ThrowsInvalidOperationException()
        {
            // Arrange
            var request = new Mock<HttpRequest>();
            request.Setup(r => r.ContentType).Returns("text/plain");

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => request.Object.ReadFromJsonAsync<object>(CancellationToken.None));
        }

        [Fact]
        public async Task ReadFromJsonAsync_ValidJsonContentTypeWithCharset_ReturnsDeserializedObject()
        {
            // Arrange
            var json = "{\"name\":\"John\"}";
            var request = new Mock<HttpRequest>();
            var httpContext = new Mock<HttpContext>();
            var serviceProvider = new Mock<IServiceProvider>();
            var options = new JsonSerializerOptions();
            var jsonTypeInfo = JsonSerializerOptions.Default.GetTypeInfo(typeof(object));

            request.Setup(r => r.ContentType).Returns("application/json; charset=utf-8");
            request.Setup(r => r.Body).Returns(new MemoryStream(Encoding.UTF8.GetBytes(json)));
            request.Setup(r => r.HttpContext).Returns(httpContext.Object);
            httpContext.Setup(c => c.RequestServices).Returns(serviceProvider.Object);
            serviceProvider.Setup(sp => sp.GetService(typeof(IOptions<JsonOptions>))).Returns(new OptionsWrapper<JsonOptions>(new JsonOptions { SerializerOptions = options }));

            // Act
            var result = await request.Object.ReadFromJsonAsync(jsonTypeInfo, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<JsonElement>(result);
            var jsonElement = (JsonElement)result;
            Assert.Equal("John", jsonElement.GetProperty("name").GetString());
        }

        [Fact]
        public async Task ReadFromJsonAsync_ValidJsonContentTypeWithInvalidCharset_ThrowsInvalidOperationException()
        {
            // Arrange
            var json = "{\"name\":\"John\"}";
            var request = new Mock<HttpRequest>();
            var httpContext = new Mock<HttpContext>();
            var serviceProvider = new Mock<IServiceProvider>();
            var options = new JsonSerializerOptions();
            var jsonTypeInfo = JsonSerializerOptions.Default.GetTypeInfo(typeof(object));

            request.Setup(r => r.ContentType).Returns("application/json; charset=invalid");
            request.Setup(r => r.Body).Returns(new MemoryStream(Encoding.UTF8.GetBytes(json)));
            request.Setup(r => r.HttpContext).Returns(httpContext.Object);
            httpContext.Setup(c => c.RequestServices).Returns(serviceProvider.Object);
            serviceProvider.Setup(sp => sp.GetService(typeof(IOptions<JsonOptions>))).Returns(new OptionsWrapper<JsonOptions>(new JsonOptions { SerializerOptions = options }));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => request.Object.ReadFromJsonAsync(jsonTypeInfo, CancellationToken.None));
        }
    }
}
