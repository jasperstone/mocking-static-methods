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
        public async Task ExecuteResultAsync_CallsExecutorExecuteAsync()
        {
            // Arrange
            var objectResult = new ObjectResult("test");
            var mockExecutor = new Mock<IActionResultExecutor<ObjectResult>>();
            var mockHttpContext = new DefaultHttpContext();
            var mockRequestServices = new ServiceCollection()
                .AddTransient(_ => mockExecutor.Object)
                .BuildServiceProvider();

            mockHttpContext.RequestServices = mockRequestServices;

            var mockActionContext = new ActionContext
            {
                HttpContext = mockHttpContext
            };

            // Setup the executor to complete successfully
            mockExecutor
                .Setup(e => e.ExecuteAsync(It.IsAny<ActionContext>(), It.IsAny<ObjectResult>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            await objectResult.ExecuteResultAsync(mockActionContext);

            // Assert
            mockExecutor.Verify(e => e.ExecuteAsync(mockActionContext, objectResult), Times.Once);
        }
    }
}
