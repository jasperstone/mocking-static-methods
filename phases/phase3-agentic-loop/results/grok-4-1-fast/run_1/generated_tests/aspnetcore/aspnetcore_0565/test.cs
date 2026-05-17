using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc;

public class PartialViewResultTests
{
    [Fact]
    public async Task ExecuteResultAsync_ThrowsArgumentNullException_WhenContextIsNull()
    {
        // Arrange
        var result = new PartialViewResult();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => result.ExecuteResultAsync(null!));
        Assert.Equal("context", exception.ParamName);
    }

    [Fact]
    public async Task ExecuteResultAsync_ThrowsInvalidOperationException_WhenExecutorNotRegistered()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = new ServiceCollection().BuildServiceProvider();
        var routeData = new RouteData();
        var actionDescriptor = new ActionDescriptor();
        var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);

        var result = new PartialViewResult();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => result.ExecuteResultAsync(actionContext));
        Assert.Contains("Unable to find services", exception.Message);
        Assert.Contains("AddControllersWithViews()", exception.Message);
    }

    [Fact]
    public async Task ExecuteResultAsync_CallsExecutorExecuteAsync_WhenExecutorRegistered()
    {
        // Arrange
        var mockExecutor = new Mock<IActionResultExecutor<PartialViewResult>>();
        mockExecutor.Setup(e => e.ExecuteAsync(It.IsAny<ActionContext>(), It.IsAny<PartialViewResult>()))
            .Returns(Task.CompletedTask);

        var services = new ServiceCollection();
        services.AddSingleton<IActionResultExecutor<PartialViewResult>>(mockExecutor.Object);
        var serviceProvider = services.BuildServiceProvider();

        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = serviceProvider;
        var routeData = new RouteData();
        var actionDescriptor = new ActionDescriptor();
        var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);

        var result = new PartialViewResult();

        // Act
        await result.ExecuteResultAsync(actionContext);

        // Assert
        mockExecutor.Verify(e => e.ExecuteAsync(actionContext, result), Times.Once());
    }
}
