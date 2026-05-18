using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc;

public class PartialViewResultServiceLookupTest
{
    [Fact]
    public async Task ExecuteResultAsync_CallsGetService_OnRequestServices()
    {
        // Arrange
        var executorMock = new Mock<IActionResultExecutor<PartialViewResult>>(MockBehavior.Strict);
        executorMock.Setup(e => e.ExecuteAsync(It.IsAny<ActionContext>(), It.IsAny<PartialViewResult>()))
                   .Returns(Task.CompletedTask);

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IActionResultExecutor<PartialViewResult>)))
                          .Returns(executorMock.Object);

        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = serviceProviderMock.Object;

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

        var result = new PartialViewResult();

        // Act
        await result.ExecuteResultAsync(actionContext);

        // Assert
        serviceProviderMock.Verify(sp => sp.GetService(typeof(IActionResultExecutor<PartialViewResult>)), Times.Once);
        executorMock.Verify();
    }

    [Fact]
    public async Task ExecuteResultAsync_ThrowsInvalidOperationException_WhenExecutorNotFound()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IActionResultExecutor<PartialViewResult>)))
                          .Returns((IActionResultExecutor<PartialViewResult>?)null);

        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = serviceProviderMock.Object;

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

        var result = new PartialViewResult();

        var expectedMessage = "Unable to find the required services. Please add all the required services by calling " +
            "'IServiceCollection.AddControllersWithViews()' inside the call to 'ConfigureServices(...)' " +
            "in the application startup code.";

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => result.ExecuteResultAsync(actionContext));
        Assert.Equal(expectedMessage, ex.Message);
        serviceProviderMock.Verify(sp => sp.GetService(typeof(IActionResultExecutor<PartialViewResult>)), Times.Once);
    }
}
