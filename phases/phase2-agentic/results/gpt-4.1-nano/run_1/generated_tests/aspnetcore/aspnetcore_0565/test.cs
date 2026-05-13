using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.AspNetCore.Mvc
{
    public class PartialViewResultTests
    {
        [Fact]
        public async Task ExecuteResultAsync_CallsExecutor_WhenServiceExists()
        {
            // Arrange
            var context = new ActionContext
            {
                HttpContext = new DefaultHttpContext()
            };
            var services = new ServiceCollection().BuildServiceProvider();
            var mockExecutor = new MockActionResultExecutor();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IActionResultExecutor<PartialViewResult>>(mockExecutor);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            context.HttpContext.RequestServices = serviceProvider;

            var result = new PartialViewResult();
            result.ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary());
            result.ViewData.Model = "TestModel";

            // Act
            await result.ExecuteResultAsync(context);

            // Assert
            Assert.True(mockExecutor.WasCalled);
            Assert.Equal(context, mockExecutor.CapturedContext);
            Assert.Equal(result, mockExecutor.CapturedResult);
        }

        [Fact]
        public async Task ExecuteResultAsync_Throws_WhenServiceNotFound()
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

        // Mock implementation of IActionResultExecutor<PartialViewResult>
        private class MockActionResultExecutor : IActionResultExecutor<PartialViewResult>
        {
            public bool WasCalled { get; private set; } = false;
            public ActionContext? CapturedContext { get; private set; }
            public PartialViewResult? CapturedResult { get; private set; }

            public Task ExecuteAsync(ActionContext context, PartialViewResult result)
            {
                WasCalled = true;
                CapturedContext = context;
                CapturedResult = result;
                return Task.CompletedTask;
            }
        }
    }
}
