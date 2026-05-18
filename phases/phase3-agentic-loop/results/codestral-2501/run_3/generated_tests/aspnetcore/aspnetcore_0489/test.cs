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
        public async Task ExecuteResultAsync_ShouldCallExecutorExecuteAsync()
        {
            // Arrange
            var mockExecutor = new Mock<IActionResultExecutor<ObjectResult>>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockHttpContext = new Mock<HttpContext>();
            var mockActionContext = new Mock<ActionContext>();

            mockServiceProvider
                .Setup(sp => sp.GetRequiredService<IActionResultExecutor<ObjectResult>>())
                .Returns(mockExecutor.Object);

            mockHttpContext.Setup(hc => hc.RequestServices).Returns(mockServiceProvider.Object);
            mockActionContext.Setup(ac => ac.HttpContext).Returns(mockHttpContext.Object);

            var objectResult = new ObjectResult(null);

            // Act
            await objectResult.ExecuteResultAsync(mockActionContext.Object);

            // Assert
            mockExecutor.Verify(executor => executor.ExecuteAsync(mockActionContext.Object, objectResult), Times.Once);
        }
    }
}
