using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Http.Tests;

public class HttpResponseJsonExtensionsTests
{
    [Fact]
    public void ResolveSerializerOptions_NullRequestServices_ReturnsDefaultOptions()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = null;

        // Act
        var options = HttpResponseJsonExtensions.ResolveSerializerOptions(httpContext);

        // Assert
        Assert.Equal(JsonOptions.DefaultSerializerOptions, options);
        Assert.Same(JavaScriptEncoder.UnsafeRelaxedJsonEscaping, options.Encoder);
    }

    [Fact]
    public void ResolveSerializerOptions_NoJsonOptionsService_ReturnsDefaultOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = services.BuildServiceProvider();

        // Act
        var options = HttpResponseJsonExtensions.ResolveSerializerOptions(httpContext);

        // Assert
        Assert.Equal(JsonOptions.DefaultSerializerOptions, options);
    }

    [Fact]
    public void ResolveSerializerOptions_NullJsonOptionsValue_ReturnsDefaultOptions()
    {
        // Arrange
        var mockOptions = new Mock<IOptions<JsonOptions>>();
        mockOptions.Setup(o => o.Value).Returns((JsonOptions)null);

        var services = new ServiceCollection();
        services.AddSingleton(mockOptions.Object);
        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = services.BuildServiceProvider();

        // Act
        var options = HttpResponseJsonExtensions.ResolveSerializerOptions(httpContext);

        // Assert
        Assert.Equal(JsonOptions.DefaultSerializerOptions, options);
    }

    [Fact]
    public void ResolveSerializerOptions_NullSerializerOptions_ReturnsDefaultOptions()
    {
        // Arrange
        var jsonOptions = new JsonOptions();
        jsonOptions.SerializerOptions = null;
        var services = new ServiceCollection();
        services.AddSingleton<IOptions<JsonOptions>>(Options.Create(jsonOptions));
        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = services.BuildServiceProvider();

        // Act
        var options = HttpResponseJsonExtensions.ResolveSerializerOptions(httpContext);

        // Assert
        Assert.Equal(JsonOptions.DefaultSerializerOptions, options);
    }

    [Fact]
    public void ResolveSerializerOptions_ValidJsonOptions_ReturnsConfiguredOptions()
    {
        // Arrange
        var customOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var jsonOptions = new JsonOptions { SerializerOptions = customOptions };
        var services = new ServiceCollection();
        services.AddSingleton<IOptions<JsonOptions>>(Options.Create(jsonOptions));
        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = services.BuildServiceProvider();

        // Act
        var result = HttpResponseJsonExtensions.ResolveSerializerOptions(httpContext);

        // Assert
        Assert.Same(customOptions, result);
        Assert.Equal(JsonNamingPolicy.CamelCase, result.PropertyNamingPolicy);
    }
}
