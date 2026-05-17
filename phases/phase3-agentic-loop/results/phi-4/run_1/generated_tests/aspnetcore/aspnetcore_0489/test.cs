using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Tests
{
    public class ObjectResultTests
    {
        [Fact]
        public async Task ExecuteResultAsync_CallsExecutor()
        {
            // Arrange
            var actionContext = new ActionContext
            {
                HttpContext = new DefaultHttpContext(),
                RouteData = new RouteData(),
                ActionDescriptor = new ActionDescriptor()
            };

            var executorMock = new Mock<IActionResultExecutor<ObjectResult>>();
            executorMock
                .Setup(e => e.ExecuteAsync(It.IsAny<ActionContext>(), It.IsAny<ObjectResult>()))
                .Returns(Task.CompletedTask);

            var services = new ServiceCollection();
            services.AddSingleton(executorMock.Object);
            var serviceProvider = services.BuildServiceProvider();

            var objectResult = new ObjectResult("test")
            {
                HttpContext = actionContext.HttpContext
            };

            // Act
            await objectResult.ExecuteResultAsync(actionContext);

            // Assert
            executorMock.Verify(e => e.ExecuteAsync(actionContext, objectResult), Times.Once);
        }
    }
}
