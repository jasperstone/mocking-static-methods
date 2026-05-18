using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Tests
{
    public class PartialViewResultTests
    {
        [Fact]
        public async Task ExecuteResultAsync_ExecutorFound_ExecutesSuccessfully()
        {
            // Arrange
            var mockExecutor = new Mock<IActionResultExecutor<PartialViewResult>>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(s => s.GetService(typeof(IActionResultExecutor<PartialViewResult>)))
                .Returns(mockExecutor.Object);

            var context = new ActionContext
            {
                HttpContext = new DefaultHttpContext { RequestServices = mockServiceProvider.Object }
            };

            var partialViewResult = new PartialViewResult();

            // Act
            await partialViewResult.ExecuteResultAsync(context);

            // Assert
            mockExecutor.Verify(e => e.ExecuteAsync(context, partialViewResult), Times.Once);
        }

        [Fact]
        public void ExecuteResultAsync_ExecutorNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(s => s.GetService(typeof(IActionResultExecutor<PartialViewResult>)))
                .Returns((IActionResultExecutor<PartialViewResult>)null);

            var context = new ActionContext
            {
                HttpContext = new DefaultHttpContext { RequestServices = mockServiceProvider.Object }
            };

            var partialViewResult = new PartialViewResult();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => partialViewResult.ExecuteResultAsync(context).GetAwaiter().GetResult());
            Assert.Contains("Unable to find services", exception.Message);
        }
    }
}
