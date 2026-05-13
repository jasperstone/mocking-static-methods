using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Http.Json;

public class HttpResponseJsonExtensionsTests
{
    [Fact]
    public void ResolveSerializerOptions_WhenOptionsProvided_ReturnsProvidedOptions()
    {
        // Arrange
        var jsonOptions = new JsonOptions { SerializerOptions = new JsonSerializerOptions() };
        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(s => s.GetService(typeof(IOptions<JsonOptions>)))
            .Returns(new OptionsWrapper<JsonOptions>(jsonOptions));

        var httpContext = new DefaultHttpContext { RequestServices = serviceProvider.Object };

        // Act
        var result = HttpResponseJsonExtensions.ResolveSerializerOptions(httpContext);

        // Assert
        Assert.Same(jsonOptions.SerializerOptions, result);
    }

    [Fact]
    public void ResolveSerializerOptions_WhenOptionsNotProvided_ReturnsDefaultOptions()
    {
        // Arrange
        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(s => s.GetService(typeof(IOptions<JsonOptions>)))
            .Returns((IOptions<JsonOptions>)null);

        var httpContext = new DefaultHttpContext { RequestServices = serviceProvider.Object };

        // Act
        var result = HttpResponseJsonExtensions.ResolveSerializerOptions(httpContext);

        // Assert
        Assert.Same(JsonOptions.DefaultSerializerOptions, result);
    }
}
