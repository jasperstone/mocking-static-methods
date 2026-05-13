using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.AspNetCore.Mvc
{
    public class PartialViewResultTests
    {
        [Fact]
        public async Task ExecuteResultAsync_CallsExecutor_WhenExecutorIsAvailable()
        {
            // Arrange
            var context = new ActionContext
            {
                HttpContext = new DefaultHttpContext()
            };
            var services = new ServiceCollection()
                .AddTransient<PartialViewResultExecutor>()
                .BuildServiceProvider();

            context.HttpContext.RequestServices = services;

            var executor = new PartialViewResultExecutor();
            services.GetRequiredService<IServiceProvider>()
                .GetService<IActionResultExecutor<PartialViewResult>>() = executor;

            var result = new PartialViewResult();

            // Act
            await result.ExecuteResultAsync(context);

            // Assert
            Assert.True(executor.WasCalled);
        }

        [Fact]
        public async Task ExecuteResultAsync_Throws_WhenExecutorIsNull()
        {
            // Arrange
            var context = new ActionContext
            {
                HttpContext = new DefaultHttpContext()
            };
            var services = new ServiceCollection()
                .BuildServiceProvider();

            context.HttpContext.RequestServices = services;

            var result = new PartialViewResult();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => result.ExecuteResultAsync(context));
        }
    }

    // Dummy executor for testing
    public class PartialViewResultExecutor : IActionResultExecutor<PartialViewResult>
    {
        public bool WasCalled { get; private set; } = false;

        public Task ExecuteAsync(ActionContext context, PartialViewResult result)
        {
            WasCalled = true;
            return Task.CompletedTask;
        }
    }
}
