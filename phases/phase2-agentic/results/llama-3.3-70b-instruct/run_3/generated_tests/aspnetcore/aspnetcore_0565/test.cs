using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Tests
{
    public class PartialViewResultTests
    {
        [Fact]
        public async Task ExecuteResultAsync_ExecutorNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var context = new ActionContext
            {
                HttpContext = new DefaultHttpContext(),
            };

            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            context.HttpContext.RequestServices = serviceProvider;

            var partialViewResult = new PartialViewResult();

            // Act and Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => partialViewResult.ExecuteResultAsync(context));
        }

        [Fact]
        public async Task ExecuteResultAsync_ExecutorFound_ExecutesAsync()
        {
            // Arrange
            var context = new ActionContext
            {
                HttpContext = new DefaultHttpContext(),
            };

            var executorMock = new Mock<IActionResultExecutor<PartialViewResult>>();
            executorMock
                .Setup(e => e.ExecuteAsync(It.IsAny<ActionContext>(), It.IsAny<PartialViewResult>()))
                .Returns(Task.CompletedTask);

            var services = new ServiceCollection();
            services.AddSingleton<IActionResultExecutor<PartialViewResult>>(executorMock.Object);
            var serviceProvider = services.BuildServiceProvider();

            context.HttpContext.RequestServices = serviceProvider;

            var partialViewResult = new PartialViewResult();

            // Act
            await partialViewResult.ExecuteResultAsync(context);

            // Assert
            executorMock.Verify(e => e.ExecuteAsync(It.IsAny<ActionContext>(), It.IsAny<PartialViewResult>()), Times.Once);
        }
    }
}
