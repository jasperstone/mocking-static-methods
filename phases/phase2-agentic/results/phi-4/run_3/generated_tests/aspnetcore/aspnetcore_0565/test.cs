using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Routing;
using Moq;
using Xunit;

public class PartialViewResultTests
{
    [Fact]
    public async Task ExecuteResultAsync_WhenExecutorIsRetrieved_ShouldExecuteExecutor()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var viewEngine = new Mock<ICompositeViewEngine>();
        var executor = new PartialViewResultExecutor(viewEngine.Object);
        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(s => s.GetService(typeof(IActionResultExecutor<PartialViewResult>)))
            .Returns(executor);

        var partialViewResult = new PartialViewResult
        {
            ViewEngine = viewEngine.Object,
            ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
        };

        // Act
        await partialViewResult.ExecuteResultAsync(actionContext);

        // Assert
        viewEngine.Verify(v => v.FindPartialView(It.IsAny<ActionContext>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void ExecuteResultAsync_WhenExecutorIsNull_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(s => s.GetService(typeof(IActionResultExecutor<PartialViewResult>)))
            .Returns((IActionResultExecutor<PartialViewResult>)null);

        var partialViewResult = new PartialViewResult
        {
            ViewEngine = null,
            ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => partialViewResult.ExecuteResultAsync(actionContext).GetAwaiter().GetResult());
        Assert.Contains("Unable to find services", exception.Message);
    }
}
