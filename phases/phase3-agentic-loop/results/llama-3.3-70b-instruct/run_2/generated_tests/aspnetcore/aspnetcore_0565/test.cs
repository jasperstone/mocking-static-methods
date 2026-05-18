using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Tests
{
    public class PartialViewResultTests
    {
        [Fact]
        public async Task ExecuteResultAsync_ExecutorFound_ExecuteAsyncCalled()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddScoped<IActionResultExecutor<PartialViewResult>>(provider => new PartialViewResultExecutor())
                .BuildServiceProvider();

            var httpContext = new DefaultHttpContext { RequestServices = serviceProvider };
            var actionContext = new ActionContext(httpContext, new RouteData(), new ControllerActionDescriptor());

            var partialViewResult = new PartialViewResult();

            // Act
            await partialViewResult.ExecuteResultAsync(actionContext);

            // Assert
            // The executor should be found and ExecuteAsync should be called
        }

        [Fact]
        public async Task ExecuteResultAsync_ExecutorNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var serviceProvider = new ServiceCollection().BuildServiceProvider();

            var httpContext = new DefaultHttpContext { RequestServices = serviceProvider };
            var actionContext = new ActionContext(httpContext, new RouteData(), new ControllerActionDescriptor());

            var partialViewResult = new PartialViewResult();

            // Act and Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => partialViewResult.ExecuteResultAsync(actionContext));
        }

        private class PartialViewResultExecutor : IActionResultExecutor<PartialViewResult>
        {
            public Task ExecuteAsync(ActionContext context, PartialViewResult result)
            {
                // Mock implementation
                return Task.CompletedTask;
            }
        }
    }
}
