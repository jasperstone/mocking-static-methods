using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Mvc.ViewFeatures.Tests
{
    public class PartialViewResultTests
    {
        [Fact]
        public async Task ExecuteResultAsync_WhenExecutorIsFound_ShouldCallExecuteAsync()
        {
            // Arrange
            var context = new ActionContext
            {
                HttpContext = new DefaultHttpContext(),
                RouteData = new RouteData(),
                ActionDescriptor = new ActionDescriptor()
            };

            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            var executorMock = new Mock<IActionResultExecutor<PartialViewResult>>();
            services.AddSingleton(executorMock.Object);

            var partialViewResult = new PartialViewResult();

            // Act
            await partialViewResult.ExecuteResultAsync(context);

            // Assert
            executorMock.Verify(e => e.ExecuteAsync(context, partialViewResult), Times.Once);
        }

        [Fact]
        public void ExecuteResultAsync_WhenExecutorIsNotFound_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var context = new ActionContext
            {
                HttpContext = new DefaultHttpContext(),
                RouteData = new RouteData(),
                ActionDescriptor = new ActionDescriptor()
            };

            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            var partialViewResult = new PartialViewResult();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => partialViewResult.ExecuteResultAsync(context).GetAwaiter().GetResult());
        }
    }
}
