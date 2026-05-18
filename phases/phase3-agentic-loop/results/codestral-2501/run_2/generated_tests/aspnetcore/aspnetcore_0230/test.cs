using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;

public class HttpRequestJsonExtensionsTests
{
    [Fact]
    public async Task ReadFromJsonAsync_WithValidJsonContentType_ShouldReturnDeserializedObject()
    {
        // Arrange
        var mockRequest = new Mock<HttpRequest>();
        var mockHttpContext = new Mock<HttpContext>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockOptions = new Mock<IOptions<JsonOptions>>();

        mockOptions.Setup(o => o.Value).Returns(new JsonOptions { SerializerOptions = new JsonSerializerOptions() });
        mockServiceProvider.Setup(sp => sp.GetService(typeof(IOptions<JsonOptions>))).Returns(mockOptions.Object);
        mockHttpContext.Setup(c => c.RequestServices).Returns(mockServiceProvider.Object);

        mockRequest.Setup(r => r.HttpContext).Returns(mockHttpContext.Object);
        mockRequest.Setup(r => r.ContentType).Returns("application/json");
        mockRequest.Setup(r => r.Body).Returns(new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes("{\"key\":\"value\"}")));

        var expected = new { key = "value" };

        // Act
        var result = await mockRequest.Object.ReadFromJsonAsync<dynamic>(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expected.key, result.key);
    }

    [Fact]
    public async Task ReadFromJsonAsync_WithInvalidJsonContentType_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var mockRequest = new Mock<HttpRequest>();
        var mockHttpContext = new Mock<HttpContext>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockOptions = new Mock<IOptions<JsonOptions>>();

        mockOptions.Setup(o => o.Value).Returns(new JsonOptions { SerializerOptions = new JsonSerializerOptions() });
        mockServiceProvider.Setup(sp => sp.GetService(typeof(IOptions<JsonOptions>))).Returns(mockOptions.Object);
        mockHttpContext.Setup(c => c.RequestServices).Returns(mockServiceProvider.Object);

        mockRequest.Setup(r => r.HttpContext).Returns(mockHttpContext.Object);
        mockRequest.Setup(r => r.ContentType).Returns("text/plain");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => mockRequest.Object.ReadFromJsonAsync<dynamic>(CancellationToken.None));
    }

    [Fact]
    public void ResolveSerializerOptions_WithValidServiceProvider_ShouldReturnSerializerOptions()
    {
        // Arrange
        var mockHttpContext = new Mock<HttpContext>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockOptions = new Mock<IOptions<JsonOptions>>();

        mockOptions.Setup(o => o.Value).Returns(new JsonOptions { SerializerOptions = new JsonSerializerOptions() });
        mockServiceProvider.Setup(sp => sp.GetService(typeof(IOptions<JsonOptions>))).Returns(mockOptions.Object);
        mockHttpContext.Setup(c => c.RequestServices).Returns(mockServiceProvider.Object);

        // Act
        var result = HttpRequestJsonExtensions.ResolveSerializerOptions(mockHttpContext.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(JsonOptions.DefaultSerializerOptions, result);
    }

    [Fact]
    public void ResolveSerializerOptions_WithNullServiceProvider_ShouldReturnDefaultSerializerOptions()
    {
        // Arrange
        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(c => c.RequestServices).Returns((IServiceProvider)null);

        // Act
        var result = HttpRequestJsonExtensions.ResolveSerializerOptions(mockHttpContext.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(JsonOptions.DefaultSerializerOptions, result);
    }

    [Fact]
    public void ResolveSerializerOptions_WithNullOptions_ShouldReturnDefaultSerializerOptions()
    {
        // Arrange
        var mockHttpContext = new Mock<HttpContext>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(sp => sp.GetService(typeof(IOptions<JsonOptions>))).Returns((IOptions<JsonOptions>)null);
        mockHttpContext.Setup(c => c.RequestServices).Returns(mockServiceProvider.Object);

        // Act
        var result = HttpRequestJsonExtensions.ResolveSerializerOptions(mockHttpContext.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(JsonOptions.DefaultSerializerOptions, result);
    }
}
