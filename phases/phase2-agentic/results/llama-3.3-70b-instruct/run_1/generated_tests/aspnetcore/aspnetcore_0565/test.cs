using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
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
        public async Task ExecuteResultAsync_ExecutorFound_ExecutesAsync()
        {
            // Arrange
            var context = new ActionContext();
            var services = new ServiceCollection();
            services.AddTransient<IActionResultExecutor<PartialViewResult>, PartialViewResultExecutor>();
            var serviceProvider = services.BuildServiceProvider();
            context.HttpContext = new DefaultHttpContext { RequestServices = serviceProvider };
            var partialViewResult = new PartialViewResult();

            // Act
            await partialViewResult.ExecuteResultAsync(context);

            // Assert
            // No exception thrown, executor was found and executed.
        }

        [Fact]
        public async Task ExecuteResultAsync_ExecutorNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var context = new ActionContext();
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            context.HttpContext = new DefaultHttpContext { RequestServices = serviceProvider };
            var partialViewResult = new PartialViewResult();

            // Act and Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => partialViewResult.ExecuteResultAsync(context));
        }

        private class PartialViewResultExecutor : IActionResultExecutor<PartialViewResult>
        {
            public Task ExecuteAsync(ActionContext context, PartialViewResult result)
            {
                return Task.CompletedTask;
            }
        }
    }
}
