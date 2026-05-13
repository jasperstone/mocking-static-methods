using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;

namespace Microsoft.AspNetCore.Mvc
{
    public class ObjectResultTests
    {
        [Fact]
        public async Task ExecuteResultAsync_CallsExecutorExecuteAsync()
        {
            // Arrange
            var mockExecutor = new Mock<IActionResultExecutor<ObjectResult>>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockHttpContext = new DefaultHttpContext();
            var mockRequestServices = new Mock<IServiceProvider>();
            mockHttpContext.RequestServices = mockRequestServices.Object;

            var mockExecutorObj = mockExecutor.Object;

            // Setup the service provider to return the mock executor
            mockRequestServices.Setup(s => s.GetRequiredService<IActionResultExecutor<ObjectResult>>())
                .Returns(mockExecutorObj);

            var context = new ActionContext
            {
                HttpContext = mockHttpContext,
                RouteData = new Microsoft.AspNetCore.Routing.RouteData(),
                ActionDescriptor = new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()
            };

            var objectResult = new ObjectResult("test");
            // Act
            await objectResult.ExecuteResultAsync(context);

            // Assert
            mockRequestServices.Verify(s => s.GetRequiredService<IActionResultExecutor<ObjectResult>>(), Times.Once);
            mockExecutor.Verify(e => e.ExecuteAsync(context, objectResult), Times.Once);
        }
    }
}
