using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;

public class HttpRequestJsonExtensionsTests
{
    [Fact]
    public async Task ReadFromJsonAsync_WithValidJsonContentType_ReturnsDeserializedObject()
    {
        // Arrange
        var mockRequest = new Mock<HttpRequest>();
        var mockHttpContext = new Mock<HttpContext>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockOptions = new Mock<IOptions<JsonOptions>>();

        var jsonOptions = new JsonOptions
        {
            SerializerOptions = new JsonSerializerOptions()
        };

        mockOptions.Setup(o => o.Value).Returns(jsonOptions);
        mockServiceProvider.Setup(sp => sp.GetService(typeof(IOptions<JsonOptions>))).Returns(mockOptions.Object);
        mockHttpContext.Setup(c => c.RequestServices).Returns(mockServiceProvider.Object);

        mockRequest.Setup(r => r.HttpContext).Returns(mockHttpContext.Object);
        mockRequest.Setup(r => r.ContentType).Returns("application/json");
        mockRequest.Setup(r => r.Body).Returns(new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes("{\"key\":\"value\"}")));

        var jsonTypeInfo = JsonSerializerOptions.Default.GetTypeInfo(typeof(Dictionary<string, string>));

        // Act
        var result = await mockRequest.Object.ReadFromJsonAsync<Dictionary<string, string>>(jsonTypeInfo, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("value", result["key"]);
    }

    [Fact]
    public async Task ReadFromJsonAsync_WithInvalidJsonContentType_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockRequest = new Mock<HttpRequest>();
        var mockHttpContext = new Mock<HttpContext>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockOptions = new Mock<IOptions<JsonOptions>>();

        var jsonOptions = new JsonOptions
        {
            SerializerOptions = new JsonSerializerOptions()
        };

        mockOptions.Setup(o => o.Value).Returns(jsonOptions);
        mockServiceProvider.Setup(sp => sp.GetService(typeof(IOptions<JsonOptions>))).Returns(mockOptions.Object);
        mockHttpContext.Setup(c => c.RequestServices).Returns(mockServiceProvider.Object);

        mockRequest.Setup(r => r.HttpContext).Returns(mockHttpContext.Object);
        mockRequest.Setup(r => r.ContentType).Returns("text/plain");

        var jsonTypeInfo = JsonSerializerOptions.Default.GetTypeInfo(typeof(Dictionary<string, string>));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => mockRequest.Object.ReadFromJsonAsync<Dictionary<string, string>>(jsonTypeInfo, CancellationToken.None));
    }

    [Fact]
    public void ResolveSerializerOptions_WithServiceProvider_ReturnsSerializerOptions()
    {
        // Arrange
        var mockHttpContext = new Mock<HttpContext>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockOptions = new Mock<IOptions<JsonOptions>>();

        var jsonOptions = new JsonOptions
        {
            SerializerOptions = new JsonSerializerOptions()
        };

        mockOptions.Setup(o => o.Value).Returns(jsonOptions);
        mockServiceProvider.Setup(sp => sp.GetService(typeof(IOptions<JsonOptions>))).Returns(mockOptions.Object);
        mockHttpContext.Setup(c => c.RequestServices).Returns(mockServiceProvider.Object);

        // Act
        var result = HttpRequestJsonExtensions.ResolveSerializerOptions(mockHttpContext.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Same(jsonOptions.SerializerOptions, result);
    }

    [Fact]
    public void ResolveSerializerOptions_WithoutServiceProvider_ReturnsDefaultSerializerOptions()
    {
        // Arrange
        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(c => c.RequestServices).Returns((IServiceProvider)null);

        // Act
        var result = HttpRequestJsonExtensions.ResolveSerializerOptions(mockHttpContext.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Same(JsonOptions.DefaultSerializerOptions, result);
    }
}
