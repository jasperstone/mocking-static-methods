using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Http;

public class HttpRequestJsonExtensionsTests : IDisposable
{
    private const string JsonContent = "{\"Value\":42}";
    private MemoryStream? _stream;

    public void Dispose()
    {
        _stream?.Dispose();
    }

    [Fact]
    public async Task ReadFromJsonAsync_RequestServicesNull_UsesDefaultOptions()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = null;
        httpContext.Request.ContentType = "application/json";
        httpContext.Request.EnableBuffering();
        _stream = new MemoryStream(Encoding.UTF8.GetBytes(JsonContent));
        httpContext.Request.Body = _stream;

        // Act
        var result = await httpContext.Request.ReadFromJsonAsync<int?>();

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public async Task ReadFromJsonAsync_NoJsonOptionsRegistered_UsesDefaultOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = serviceProvider;
        httpContext.Request.ContentType = "application/json";
        httpContext.Request.EnableBuffering();
        _stream = new MemoryStream(Encoding.UTF8.GetBytes(JsonContent));
        httpContext.Request.Body = _stream;

        // Act
        var result = await httpContext.Request.ReadFromJsonAsync<int?>();

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public async Task ReadFromJsonAsync_JsonOptionsValueNull_UsesDefaultOptions()
    {
        // Arrange
        var mockOptions = new Mock<IOptions<JsonOptions>>();
        mockOptions.Setup(o => o.Value).Returns((JsonOptions)null);

        var services = new ServiceCollection();
        services.AddSingleton<IOptions<JsonOptions>>(mockOptions.Object);
        var serviceProvider = services.BuildServiceProvider();
        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = serviceProvider;
        httpContext.Request.ContentType = "application/json";
        httpContext.Request.EnableBuffering();
        _stream = new MemoryStream(Encoding.UTF8.GetBytes(JsonContent));
        httpContext.Request.Body = _stream;

        // Act
        var result = await httpContext.Request.ReadFromJsonAsync<int?>();

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public async Task ReadFromJsonAsync_CustomJsonOptionsRegistered_UsesCustomOptions()
    {
        // Arrange
        var customOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        var jsonOptions = new JsonOptions { SerializerOptions = customOptions };
        var mockOptions = new Mock<IOptions<JsonOptions>>();
        mockOptions.Setup(o => o.Value).Returns(jsonOptions);

        var services = new ServiceCollection();
        services.AddSingleton<IOptions<JsonOptions>>(mockOptions.Object);
        var serviceProvider = services.BuildServiceProvider();
        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = serviceProvider;
        httpContext.Request.ContentType = "application/json";
        httpContext.Request.EnableBuffering();
        _stream = new MemoryStream(Encoding.UTF8.GetBytes(JsonContent));
        httpContext.Request.Body = _stream;

        // Act
        var result = await httpContext.Request.ReadFromJsonAsync<int?>();

        // Assert
        Assert.Equal(42, result);
    }
}
