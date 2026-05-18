using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures
{
    public class PartialViewResultTests
    {
        [Fact]
        public async Task ExecuteResultAsync_ThrowsIfExecutorServiceNotRegistered()
        {
            // Arrange
            var partialViewResult = new PartialViewResult
            {
                ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()),
                TempData = new Mock<ITempDataDictionary>().Object
            };

            var httpContext = new DefaultHttpContext();
            // No registration of IActionResultExecutor<PartialViewResult>
            httpContext.RequestServices = new ServiceCollection().BuildServiceProvider();

            var actionContext = new ActionContext
            {
                HttpContext = httpContext,
                RouteData = new RouteData(),
                ActionDescriptor = new ActionDescriptor()
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => partialViewResult.ExecuteResultAsync(actionContext));
            Assert.Contains("AddControllersWithViews()", ex.Message);
        }

        [Fact]
        public async Task ExecuteResultAsync_CallsExecutorExecuteAsync()
        {
            // Arrange
            var partialViewResult = new PartialViewResult
            {
                ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()),
                TempData = new Mock<ITempDataDictionary>().Object
            };

            var mockExecutor = new Mock<IActionResultExecutor<PartialViewResult>>();
            mockExecutor
                .Setup(e => e.ExecuteAsync(It.IsAny<ActionContext>(), partialViewResult))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var services = new ServiceCollection();
            services.AddSingleton(mockExecutor.Object);
            var serviceProvider = services.BuildServiceProvider();

            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = serviceProvider;

            var actionContext = new ActionContext
            {
                HttpContext = httpContext,
                RouteData = new RouteData(),
                ActionDescriptor = new ActionDescriptor()
            };

            // Act
            await partialViewResult.ExecuteResultAsync(actionContext);

            // Assert
            mockExecutor.Verify();
        }
    }
}
