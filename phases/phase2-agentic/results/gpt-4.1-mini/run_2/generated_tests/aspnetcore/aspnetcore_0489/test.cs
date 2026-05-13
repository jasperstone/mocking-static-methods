using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc
{
    public class ObjectResultTests
    {
        [Fact]
        public async Task ExecuteResultAsync_CallsExecutorExecuteAsync()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var executorMock = new Mock<IActionResultExecutor<ObjectResult>>();
            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = serviceProviderMock.Object;
            var actionContext = new ActionContext
            {
                HttpContext = httpContext
            };
            var objectResult = new ObjectResult("test-value");

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IActionResultExecutor<ObjectResult>)))
                .Returns(executorMock.Object);

            // Setup GetRequiredService extension method behavior
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IActionResultExecutor<ObjectResult>>())
                .Returns(executorMock.Object);

            executorMock
                .Setup(e => e.ExecuteAsync(actionContext, objectResult))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            await objectResult.ExecuteResultAsync(actionContext);

            // Assert
            executorMock.Verify(e => e.ExecuteAsync(actionContext, objectResult), Times.Once);
        }
    }
}
