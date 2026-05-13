using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Tests
{
    public class PartialViewResultTests
    {
        [Fact]
        public async Task ExecuteResultAsync_ExecutorFound_ExecutesAsync()
        {
            // Arrange
            var executorMock = new Mock<IActionResultExecutor<PartialViewResult>>();
            executorMock.Setup(e => e.ExecuteAsync(It.IsAny<ActionContext>(), It.IsAny<PartialViewResult>()))
                .Returns(Task.CompletedTask);

            var servicesMock = new Mock<IServiceProvider>();
            servicesMock.Setup(s => s.GetService(It.IsAny<Type>()))
                .Returns(executorMock.Object);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(h => h.RequestServices)
                .Returns(servicesMock.Object);

            var actionContext = new ActionContext(httpContextMock.Object, new RouteData(), new ActionDescriptor());

            var partialViewResult = new PartialViewResult();

            // Act
            await partialViewResult.ExecuteResultAsync(actionContext);

            // Assert
            executorMock.Verify(e => e.ExecuteAsync(actionContext, partialViewResult), Times.Once);
        }

        [Fact]
        public async Task ExecuteResultAsync_ExecutorNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var servicesMock = new Mock<IServiceProvider>();
            servicesMock.Setup(s => s.GetService(It.IsAny<Type>()))
                .Returns(null);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(h => h.RequestServices)
                .Returns(servicesMock.Object);

            var actionContext = new ActionContext(httpContextMock.Object, new RouteData(), new ActionDescriptor());

            var partialViewResult = new PartialViewResult();

            // Act and Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => partialViewResult.ExecuteResultAsync(actionContext));
        }
    }
}
