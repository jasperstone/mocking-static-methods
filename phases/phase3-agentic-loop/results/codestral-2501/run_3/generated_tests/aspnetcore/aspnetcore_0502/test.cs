using Xunit;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

public class AttributeRoutingTests
{
    [Fact]
    public void CreateAttributeMegaRoute_ShouldReturnAttributeRoute()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockActionDescriptorCollectionProvider = new Mock<IActionDescriptorCollectionProvider>();
        var mockMvcAttributeRouteHandler = new Mock<MvcAttributeRouteHandler>();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        var mockDiagnosticListener = new Mock<DiagnosticListener>();
        var mockActionInvokerFactory = new Mock<IActionInvokerFactory>();
        var mockActionSelector = new Mock<IActionSelector>();

        mockServiceProvider
            .Setup(x => x.GetRequiredService<IActionDescriptorCollectionProvider>())
            .Returns(mockActionDescriptorCollectionProvider.Object);

        mockServiceProvider
            .Setup(x => x.GetRequiredService<MvcAttributeRouteHandler>())
            .Returns(mockMvcAttributeRouteHandler.Object);

        mockServiceProvider
            .Setup(x => x.GetRequiredService<IActionInvokerFactory>())
            .Returns(mockActionInvokerFactory.Object);

        mockServiceProvider
            .Setup(x => x.GetRequiredService<IActionSelector>())
            .Returns(mockActionSelector.Object);

        mockServiceProvider
            .Setup(x => x.GetRequiredService<DiagnosticListener>())
            .Returns(mockDiagnosticListener.Object);

        mockServiceProvider
            .Setup(x => x.GetRequiredService<ILoggerFactory>())
            .Returns(mockLoggerFactory.Object);

        // Act
        var result = AttributeRouting.CreateAttributeMegaRoute(mockServiceProvider.Object);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<AttributeRoute>(result);
    }

    [Fact]
    public void CreateAttributeMegaRoute_ShouldThrowIfServiceProviderIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => AttributeRouting.CreateAttributeMegaRoute(null));
    }
}
