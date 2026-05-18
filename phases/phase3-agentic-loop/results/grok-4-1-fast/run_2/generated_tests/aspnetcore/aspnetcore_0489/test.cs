using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Tests
{
    public class ObjectResultTests
    {
        [Fact]
        public async Task ExecuteResultAsync_CallsGetRequiredService_OnRequestServices()
        {
            // Arrange
            var mockExecutor = Substitute.For<IActionResultExecutor<ObjectResult>>();
            mockExecutor.ExecuteAsync(Arg.Any<ActionContext>(), Arg.Any<ObjectResult>())
                       .Returns(Task.CompletedTask);

            var services = new ServiceCollection();
            services.AddSingleton<IActionResultExecutor<ObjectResult>>(mockExecutor);
            var serviceProvider = services.BuildServiceProvider();

            var httpContext = new DefaultHttpContext()
            {
                RequestServices = serviceProvider
            };

            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            var result = new ObjectResult("test");

            // Act
            await result.ExecuteResultAsync(actionContext);

            // Assert
            await mockExecutor.Received(1).ExecuteAsync(actionContext, result);
        }

        [Fact]
        public async Task ExecuteResultAsync_ThrowsInvalidOperationException_WhenServiceNotRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            var httpContext = new DefaultHttpContext()
            {
                RequestServices = serviceProvider
            };

            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            var result = new ObjectResult("test");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => result.ExecuteResultAsync(actionContext));
            Assert.Contains("GetRequiredService", exception.Message);
        }
    }
}
