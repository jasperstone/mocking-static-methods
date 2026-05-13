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
    public class PartialViewResultTest
    {
        [Fact]
        public async Task ExecuteResultAsync_ThrowsIfExecutorNotRegistered()
        {
            // Arrange
            var services = new ServiceCollection().BuildServiceProvider();
            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = services;
            var actionContext = new ActionContext(httpContext, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());

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
            var services = new ServiceCollection();
            services.AddSingleton(mockExecutor.Object);
            var serviceProvider = services.BuildServiceProvider();

            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = serviceProvider;
            var actionContext = new ActionContext(httpContext, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());

            var partialViewResult = new PartialViewResult
            {
                ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()),
                TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>())
            };

            mockExecutor
                .Setup(e => e.ExecuteAsync(actionContext, partialViewResult))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            await partialViewResult.ExecuteResultAsync(actionContext);

            // Assert
            mockExecutor.Verify();
        }
    }
}
