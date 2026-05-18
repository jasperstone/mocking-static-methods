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
                .AddSingleton<IActionResultExecutor<PartialViewResult>>(mockExecutor.Object)
                .BuildServiceProvider();

            context.HttpContext.RequestServices = services;

            var result = new PartialViewResult();
            result.ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary());
            result.ViewData.Model = "TestModel";

            // Act
            await result.ExecuteResultAsync(context);

            // Assert
            mockExecutor.Verify(e => e.ExecuteAsync(context, result), Times.Once);
        }

        [Fact]
        public async Task ExecuteResultAsync_Throws_WhenServiceIsNotAvailable()
        {
            // Arrange
            var context = new ActionContext
            {
                HttpContext = new DefaultHttpContext()
            };
            context.HttpContext.RequestServices = new ServiceCollection().BuildServiceProvider();

            var result = new PartialViewResult();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => result.ExecuteResultAsync(context));
        }
    }
}
