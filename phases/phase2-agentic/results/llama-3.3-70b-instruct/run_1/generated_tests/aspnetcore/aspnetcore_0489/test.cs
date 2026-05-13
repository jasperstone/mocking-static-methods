using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Tests
{
    public class ObjectResultTests
    {
        [Fact]
        public async Task ExecuteResultAsync_CallsExecutorExecuteAsync()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddScoped<IActionResultExecutor<ObjectResult>, ObjectResultExecutor>()
                .BuildServiceProvider();

            var httpContext = new Mock<HttpContext>();
            httpContext.Setup(c => c.RequestServices).Returns(serviceProvider);

            var actionContext = new ActionContext(httpContext.Object, new RouteData(), new ActionDescriptor());

            var executorMock = new Mock<IActionResultExecutor<ObjectResult>>();
            serviceProvider.GetService<IActionResultExecutor<ObjectResult>>().ExecuteAsync(actionContext, new ObjectResult("Test"));

            var objectResult = new ObjectResult("Test");

            // Act
            await objectResult.ExecuteResultAsync(actionContext);

            // Assert
            executorMock.Verify(e => e.ExecuteAsync(It.IsAny<ActionContext>(), It.IsAny<ObjectResult>()), Times.Once);
        }
    }

    public class ObjectResultExecutor : IActionResultExecutor<ObjectResult>
    {
        public Task ExecuteAsync(ActionContext context, ObjectResult result)
        {
            return Task.CompletedTask;
        }
    }
}
