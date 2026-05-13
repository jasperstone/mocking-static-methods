using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Tests
{
    public class PartialViewResultTests
    {
        [Fact]
        public async Task ExecuteResultAsync_ThrowsInvalidOperationException_WhenExecutorNotFound()
        {
            // Arrange
            var context = new ActionContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var services = new Mock<IServiceProvider>();
            services.Setup(s => s.GetService(typeof(IActionResultExecutor<PartialViewResult>)))
                    .Returns(null);

            context.HttpContext.RequestServices = services.Object;

            var result = new PartialViewResult();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => result.ExecuteResultAsync(context));
        }

        [Fact]
        public async Task ExecuteResultAsync_CallsExecutorExecuteAsync_WhenExecutorFound()
        {
            // Arrange
            var context = new ActionContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var executorMock = new Mock<IActionResultExecutor<PartialViewResult>>();
            executorMock.Setup(e => e.ExecuteAsync(It.IsAny<ActionContext>(), It.IsAny<PartialViewResult>()))
                        .Returns(Task.CompletedTask)
                        .Verifiable();

            var services = new Mock<IServiceProvider>();
            services.Setup(s => s.GetService(typeof(IActionResultExecutor<PartialViewResult>)))
                    .Returns(executorMock.Object);

            context.HttpContext.RequestServices = services.Object;

            var result = new PartialViewResult();

            // Act
            await result.ExecuteResultAsync(context);

            // Assert
            executorMock.Verify(e => e.ExecuteAsync(It.IsAny<ActionContext>(), It.IsAny<PartialViewResult>()), Times.Once);
        }
    }
}
