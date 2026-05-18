using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Core.Test
{
    public class ObjectResultTests
    {
        [Fact]
        public async Task ExecuteResultAsync_CallsExecutorFromRequestServices()
        {
            // Arrange
            var mockExecutor = new Mock<IActionResultExecutor<ObjectResult>>();
            var objectResult = new ObjectResult("test-value");

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
                .Setup(sp => sp.GetService(typeof(IActionResultExecutor<ObjectResult>)))
                .Returns(mockExecutor.Object);

            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = serviceProvider.Object;

            var actionContext = new ActionContext
            {
                HttpContext = httpContext
            };

            mockExecutor
                .Setup(executor => executor.ExecuteAsync(actionContext, objectResult))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            await objectResult.ExecuteResultAsync(actionContext);

            // Assert
            mockExecutor.Verify();
        }
    }
}
