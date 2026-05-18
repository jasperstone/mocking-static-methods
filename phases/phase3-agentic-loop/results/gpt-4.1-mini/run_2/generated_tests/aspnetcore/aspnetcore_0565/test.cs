using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace MvcViewFeaturesTests
{
    public class PartialViewResultTests
    {
        [Fact]
        public async Task ExecuteResultAsync_CallsExecutorExecuteAsync_WhenExecutorIsResolved()
        {
            // Arrange
            var mockExecutor = new Mock<IActionResultExecutor<PartialViewResult>>();
            var partialViewResult = new PartialViewResult
            {
                ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()),
                TempData = new Mock<ITempDataDictionary>().Object
            };

            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = new ServiceProviderStub(mockExecutor.Object);

            var actionContext = new ActionContext
            {
                HttpContext = httpContext
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

        [Fact]
        public async Task ExecuteResultAsync_ThrowsInvalidOperationException_WhenExecutorIsNotRegistered()
        {
            // Arrange
            var partialViewResult = new PartialViewResult
            {
                ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()),
                TempData = new Mock<ITempDataDictionary>().Object
            };

            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = new ServiceProviderStub(null);

            var actionContext = new ActionContext
            {
                HttpContext = httpContext
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => partialViewResult.ExecuteResultAsync(actionContext));
            Assert.Contains("AddControllersWithViews()", ex.Message);
        }

        private class ServiceProviderStub : IServiceProvider
        {
            private readonly object? _service;

            public ServiceProviderStub(object? service)
            {
                _service = service;
            }

            public object? GetService(Type serviceType)
            {
                if (serviceType == typeof(IActionResultExecutor<PartialViewResult>))
                {
                    return _service;
                }
                return null;
            }
        }
    }
}
