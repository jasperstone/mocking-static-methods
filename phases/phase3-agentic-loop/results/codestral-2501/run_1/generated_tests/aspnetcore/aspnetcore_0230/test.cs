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
    public async Task ReadFromJsonAsync_ValidJsonContentType_ReturnsDeserializedObject()
    {
        // Arrange
        var mockRequest = new Mock<HttpRequest>();
        var mockHttpContext = new Mock<HttpContext>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockOptions = new Mock<IOptions<JsonOptions>>();

        mockRequest.Setup(r => r.ContentType).Returns("application/json");
        mockRequest.Setup(r => r.Body).Returns(new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes("{\"key\":\"value\"}")));
        mockRequest.Setup(r => r.HttpContext).Returns(mockHttpContext.Object);
        mockHttpContext.Setup(c => c.RequestServices).Returns(mockServiceProvider.Object);
        mockServiceProvider.Setup(sp => sp.GetService(typeof(IOptions<JsonOptions>))).Returns(mockOptions.Object);
        mockOptions.Setup(o => o.Value).Returns(new JsonOptions());

        // Act
        var result = await mockRequest.Object.ReadFromJsonAsync<Dictionary<string, string>>(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("value", result["key"]);
    }

    [Fact]
    public async Task ReadFromJsonAsync_InvalidJsonContentType_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockRequest = new Mock<HttpRequest>();
        mockRequest.Setup(r => r.ContentType).Returns("text/plain");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => mockRequest.Object.ReadFromJsonAsync<Dictionary<string, string>>(CancellationToken.None));
    }

    [Fact]
    public void ResolveSerializerOptions_ServiceProviderReturnsNull_ReturnsDefaultSerializerOptions()
    {
        // Arrange
        var mockHttpContext = new Mock<HttpContext>();
        var mockServiceProvider = new Mock<IServiceProvider>();

        mockHttpContext.Setup(c => c.RequestServices).Returns(mockServiceProvider.Object);
        mockServiceProvider.Setup(sp => sp.GetService(typeof(IOptions<JsonOptions>))).Returns((IOptions<JsonOptions>)null);

        // Act
        var result = HttpRequestJsonExtensions.ResolveSerializerOptions(mockHttpContext.Object);

        // Assert
        Assert.Equal(JsonOptions.DefaultSerializerOptions, result);
    }

    [Fact]
    public void ResolveSerializerOptions_ServiceProviderReturnsOptions_ReturnsSerializerOptions()
    {
        // Arrange
        var mockHttpContext = new Mock<HttpContext>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockOptions = new Mock<IOptions<JsonOptions>>();

        mockHttpContext.Setup(c => c.RequestServices).Returns(mockServiceProvider.Object);
        mockServiceProvider.Setup(sp => sp.GetService(typeof(IOptions<JsonOptions>))).Returns(mockOptions.Object);
        mockOptions.Setup(o => o.Value).Returns(new JsonOptions());

        // Act
        var result = HttpRequestJsonExtensions.ResolveSerializerOptions(mockHttpContext.Object);

        // Assert
        Assert.NotNull(result);
    }
}
