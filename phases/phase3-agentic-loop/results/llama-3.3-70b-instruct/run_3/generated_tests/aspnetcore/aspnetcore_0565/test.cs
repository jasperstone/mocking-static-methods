using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Tests
{
    public class PartialViewResultTests
    {
        [Fact]
        public async Task ExecuteResultAsync_ExecutorFound_ExecutesAsync()
        {
            // Arrange
            var executorMock = new Mock<IActionResultExecutor<PartialViewResult>>();
            var servicesMock = new Mock<IServiceProvider>();
            servicesMock.Setup(s => s.GetService(typeof(IActionResultExecutor<PartialViewResult>)))
                .Returns(executorMock.Object);

            var contextMock = new Mock<ActionContext>();
            contextMock.SetupGet(c => c.HttpContext.RequestServices)
                .Returns(servicesMock.Object);

            var result = new PartialViewResult();

            // Act
            await result.ExecuteResultAsync(contextMock.Object);

            // Assert
            executorMock.Verify(e => e.ExecuteAsync(contextMock.Object, result), Times.Once);
        }

        [Fact]
        public async Task ExecuteResultAsync_ExecutorNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var servicesMock = new Mock<IServiceProvider>();
            servicesMock.Setup(s => s.GetService(typeof(IActionResultExecutor<PartialViewResult>)))
                .Returns(null);

            var contextMock = new Mock<ActionContext>();
            contextMock.SetupGet(c => c.HttpContext.RequestServices)
                .Returns(servicesMock.Object);

            var result = new PartialViewResult();

            // Act and Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => result.ExecuteResultAsync(contextMock.Object));
        }
    }
}
