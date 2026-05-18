using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Tests
{
    public class PartialViewResultTests
    {
        [Fact]
        public async Task ExecuteResultAsync_ShouldRetrieveExecutorFromServiceProvider()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockExecutor = new Mock<IActionResultExecutor<PartialViewResult>>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IActionResultExecutor<PartialViewResult>)))
                .Returns(mockExecutor.Object);

            var httpContext = new DefaultHttpContext
            {
                RequestServices = mockServiceProvider.Object
            };
            var actionContext = new ActionContext
            {
                HttpContext = httpContext
            };
            var partialViewResult = new PartialViewResult();

            // Act
            await partialViewResult.ExecuteResultAsync(actionContext);

            // Assert
            mockExecutor.Verify(executor => executor.ExecuteAsync(actionContext, partialViewResult), Times.Once);
        }

        [Fact]
        public async Task ExecuteResultAsync_ShouldThrowInvalidOperationException_WhenExecutorNotFound()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IActionResultExecutor<PartialViewResult>)))
                .Returns((IActionResultExecutor<PartialViewResult>)null);

            var httpContext = new DefaultHttpContext
            {
                RequestServices = mockServiceProvider.Object
            };
            var actionContext = new ActionContext
            {
                HttpContext = httpContext
            };
            var partialViewResult = new PartialViewResult();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => partialViewResult.ExecuteResultAsync(actionContext));
        }
    }
}
