using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Tests
{
    public class ObjectResultTests
    {
        [Fact]
        public async Task ExecuteResultAsync_CallsGetRequiredService_OnRequestServices()
        {
            // Arrange
            var mockExecutor = new Mock<IActionResultExecutor<ObjectResult>>();
            mockExecutor.Setup(e => e.ExecuteAsync(It.IsAny<ActionContext>(), It.IsAny<ObjectResult>()))
                       .Returns(Task.CompletedTask);

            var mockServices = new Mock<IServiceProvider>();
            mockServices.Setup(s => s.GetRequiredService<IActionResultExecutor<ObjectResult>>())
                       .Returns(mockExecutor.Object);

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.RequestServices = mockServices.Object;

            var routeData = new RouteData();
            var actionDescriptor = new ActionDescriptor();
            var actionContext = new ActionContext(mockHttpContext, routeData, actionDescriptor);

            var objectResult = new ObjectResult("test value");

            // Act
            await objectResult.ExecuteResultAsync(actionContext);

            // Assert
            mockServices.Verify(s => s.GetRequiredService<IActionResultExecutor<ObjectResult>>(), Times.Once());
            mockExecutor.Verify(e => e.ExecuteAsync(actionContext, objectResult), Times.Once());
        }

        [Fact]
        public void ExecuteResultAsync_ThrowsInvalidOperationException_WhenServiceNotRegistered()
        {
            // Arrange
            var mockServices = new Mock<IServiceProvider>();
            mockServices.Setup(s => s.GetRequiredService<IActionResultExecutor<ObjectResult>>())
                       .Throws(new InvalidOperationException("No service for type 'IActionResultExecutor<ObjectResult>' has been registered."));

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.RequestServices = mockServices.Object;

            var routeData = new RouteData();
            var actionDescriptor = new ActionDescriptor();
            var actionContext = new ActionContext(mockHttpContext, routeData, actionDescriptor);

            var objectResult = new ObjectResult("test value");

            // Act & Assert
            var exception = Assert.ThrowsAsync<InvalidOperationException>(
                async () => await objectResult.ExecuteResultAsync(actionContext));
            Assert.Equal("No service for type 'IActionResultExecutor<ObjectResult>' has been registered.", exception.Result.Message);
        }
    }
}
