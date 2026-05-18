using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;

namespace Mvc.ViewFeatures.Tests
{
    public class PartialViewResultTests
    {
        [Fact]
        public async Task ExecuteResultAsync_CallsExecutor_WhenServiceIsAvailable()
        {
            // Arrange
            var context = new ActionContext
            {
                HttpContext = new DefaultHttpContext()
            };
            var mockExecutor = new Mock<IActionResultExecutor<PartialViewResult>>();
            var services = new ServiceCollection()
                .AddSingleton(mockExecutor.Object)
                .BuildServiceProvider();

            context.HttpContext.RequestServices = services;

            var result = new PartialViewResult();
            var called = false;
            mockExecutor.Setup(e => e.ExecuteAsync(It.IsAny<ActionContext>(), result))
                .Returns<ActionContext, PartialViewResult>((ctx, res) =>
                {
                    called = true;
                    return Task.CompletedTask;
                });

            // Act
            await result.ExecuteResultAsync(context);

            // Assert
            Assert.True(called);
        }

        [Fact]
        public async Task ExecuteResultAsync_Throws_WhenServiceIsNotFound()
        {
            // Arrange
            var context = new ActionContext
            {
                HttpContext = new DefaultHttpContext()
            };
            var services = new ServiceCollection().BuildServiceProvider();
            context.HttpContext.RequestServices = services;

            var result = new PartialViewResult();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => result.ExecuteResultAsync(context));
        }
    }
}
