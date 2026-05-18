using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Tests
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

            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = serviceProvider;

            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            var objectResult = new ObjectResult("Test");

            var executorMock = new Mock<IActionResultExecutor<ObjectResult>>();
            executorMock
                .Setup(e => e.ExecuteAsync(It.IsAny<ActionContext>(), It.IsAny<ObjectResult>()))
                .Returns(Task.CompletedTask);

            serviceProvider.GetService<IActionResultExecutor<ObjectResult>>().ExecuteAsync(actionContext, objectResult);

            // Act
            await objectResult.ExecuteResultAsync(actionContext);

            // Assert
            executorMock.Verify(e => e.ExecuteAsync(It.IsAny<ActionContext>(), It.IsAny<ObjectResult>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteResultAsync_ThrowsArgumentNullException_WhenContextIsNull()
        {
            // Arrange
            var objectResult = new ObjectResult("Test");

            // Act and Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => objectResult.ExecuteResultAsync(null));
        }

        [Fact]
        public async Task OnFormatting_SetsStatusCode_WhenValueIsProblemDetails()
        {
            // Arrange
            var objectResult = new ObjectResult(new ProblemDetails { Status = 404 });
            var httpContext = new DefaultHttpContext();
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            // Act
            objectResult.OnFormatting(actionContext);

            // Assert
            Assert.Equal(404, httpContext.Response.StatusCode);
        }
    }
}
