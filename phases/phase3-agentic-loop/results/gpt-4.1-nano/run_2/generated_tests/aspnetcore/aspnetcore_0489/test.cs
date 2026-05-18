using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Tests
{
    public class ObjectResultTests
    {
        [Fact]
        public async Task ExecuteResultAsync_CallsExecutorWithCorrectParameters()
        {
            // Arrange
            var mockExecutor = new Mock<IActionResultExecutor<ObjectResult>>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockHttpContext = new DefaultHttpContext();

            // Setup the service provider to return the mock executor
            mockServiceProvider
                .Setup(sp => sp.GetRequiredService<IActionResultExecutor<ObjectResult>>())
                .Returns(mockExecutor.Object);

            // Setup HttpContext.RequestServices to return our mock service provider
            mockHttpContext.RequestServices = mockServiceProvider.Object;

            var context = new ActionContext
            {
                HttpContext = mockHttpContext,
                RouteData = new Microsoft.AspNetCore.Routing.RouteData(),
                ActionDescriptor = new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()
            };

            var objectResult = new ObjectResult("test value");

            // Act
            await objectResult.ExecuteResultAsync(context);

            // Assert
            mockExecutor.Verify(executor => executor.ExecuteAsync(context, objectResult), Times.Once);
        }
    }
}
