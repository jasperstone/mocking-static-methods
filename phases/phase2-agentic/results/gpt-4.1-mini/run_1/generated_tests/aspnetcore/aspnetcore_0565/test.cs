using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Test
{
    public class PartialViewResultTests
    {
        [Fact]
        public async Task ExecuteResultAsync_ThrowsArgumentNullException_WhenContextIsNull()
        {
            // Arrange
            var result = new PartialViewResult();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => result.ExecuteResultAsync(null!));
        }

        [Fact]
        public async Task ExecuteResultAsync_ThrowsInvalidOperationException_WhenExecutorServiceNotFound()
        {
            // Arrange
            var servicesMock = new Mock<IServiceProvider>();
            servicesMock.Setup(s => s.GetService(typeof(IActionResultExecutor<PartialViewResult>))).Returns(null);

            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = servicesMock.Object;

            var actionContext = new ActionContext
            {
                HttpContext = httpContext
            };

            var result = new PartialViewResult();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => result.ExecuteResultAsync(actionContext));
            Assert.Contains("AddControllersWithViews()", ex.Message);
        }

        [Fact]
        public async Task ExecuteResultAsync_CallsExecutorExecuteAsync_WhenExecutorServiceFound()
        {
            // Arrange
            var executorMock = new Mock<IActionResultExecutor<PartialViewResult>>();
            executorMock
                .Setup(e => e.ExecuteAsync(It.IsAny<ActionContext>(), It.IsAny<PartialViewResult>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var servicesMock = new Mock<IServiceProvider>();
            servicesMock.Setup(s => s.GetService(typeof(IActionResultExecutor<PartialViewResult>))).Returns(executorMock.Object);

            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = servicesMock.Object;

            var actionContext = new ActionContext
            {
                HttpContext = httpContext
            };

            var result = new PartialViewResult();

            // Act
            await result.ExecuteResultAsync(actionContext);

            // Assert
            executorMock.Verify(e => e.ExecuteAsync(actionContext, result), Times.Once);
        }
    }
}
