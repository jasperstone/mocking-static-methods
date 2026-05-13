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

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(IActionResultExecutor<PartialViewResult>)))
                .Returns(null);

            context.HttpContext.RequestServices = mockServiceProvider.Object;

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

            var mockExecutor = new Mock<IActionResultExecutor<PartialViewResult>>();
            mockExecutor
                .Setup(executor => executor.ExecuteAsync(context, It.IsAny<PartialViewResult>()))
                .Returns(Task.CompletedTask);

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(IActionResultExecutor<PartialViewResult>)))
                .Returns(mockExecutor.Object);

            context.HttpContext.RequestServices = mockServiceProvider.Object;

            var result = new PartialViewResult();

            // Act
            await result.ExecuteResultAsync(context);

            // Assert
            mockExecutor.Verify(executor => executor.ExecuteAsync(context, result), Times.Once);
        }
    }
}
