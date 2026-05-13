using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures
{
    public class PartialViewResultTests
    {
        [Fact]
        public async Task ExecuteResultAsync_ThrowsIfExecutorNotRegistered()
        {
            // Arrange
            var services = new ServiceCollection().BuildServiceProvider();
            var httpContext = new DefaultHttpContext
            {
                RequestServices = services
            };
            var actionContext = new ActionContext
            {
                HttpContext = httpContext
            };
            var partialViewResult = new PartialViewResult
            {
                ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()),
                TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>())
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => partialViewResult.ExecuteResultAsync(actionContext));
            Assert.Contains("AddControllersWithViews()", ex.Message);
        }

        [Fact]
        public async Task ExecuteResultAsync_CallsExecutorExecuteAsync()
        {
            // Arrange
            var mockExecutor = new Mock<IActionResultExecutor<PartialViewResult>>();
            mockExecutor
                .Setup(e => e.ExecuteAsync(It.IsAny<ActionContext>(), It.IsAny<PartialViewResult>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var services = new ServiceCollection()
                .AddSingleton(mockExecutor.Object)
                .BuildServiceProvider();

            var httpContext = new DefaultHttpContext
            {
                RequestServices = services
            };
            var actionContext = new ActionContext
            {
                HttpContext = httpContext
            };
            var partialViewResult = new PartialViewResult
            {
                ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()),
                TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>())
            };

            // Act
            await partialViewResult.ExecuteResultAsync(actionContext);

            // Assert
            mockExecutor.Verify(e => e.ExecuteAsync(actionContext, partialViewResult), Times.Once);
        }
    }
}
