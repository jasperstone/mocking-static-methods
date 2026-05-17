using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc
{
    public class ObjectResultTests
    {
        [Fact]
        public async Task ExecuteResultAsync_CallsGetRequiredService_Successfully()
        {
            // Arrange
            var mockExecutor = new Mock<IActionResultExecutor<ObjectResult>>();
            mockExecutor.Setup(e => e.ExecuteAsync(It.IsAny<ActionContext>(), It.IsAny<ObjectResult>()))
                       .Returns(Task.CompletedTask);

            var services = new ServiceCollection();
            services.AddSingleton<IActionResultExecutor<ObjectResult>>(mockExecutor.Object);
            var serviceProvider = services.BuildServiceProvider();

            var httpContext = new DefaultHttpContext()
            {
                RequestServices = serviceProvider
            };

            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            var result = new ObjectResult("test");

            // Act
            await result.ExecuteResultAsync(actionContext);

            // Assert - verifies GetRequiredService was called by successful executor invocation
            mockExecutor.Verify(e => e.ExecuteAsync(actionContext, result), Times.Once);
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
            var result = new ObjectResult(null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => result.ExecuteResultAsync(actionContext));
            
            Assert.Contains("No service for type", exception.Message);
        }

        [Fact]
        public void ContentTypes_ThrowsArgumentNullException_WhenSetToNull()
        {
            // Arrange
            var result = new ObjectResult(null);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => result.ContentTypes = null);
            Assert.Equal("value", exception.ParamName);
        }
    }
}
