using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Core.Test
{
    public class ObjectResultTests
    {
        [Fact]
        public async Task ExecuteResultAsync_ResolvesExecutorFromRequestServices()
        {
            // Arrange
            var objectResult = new ObjectResult("value");
            var httpContext = new DefaultHttpContext();
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            var executor = new TestObjectResultExecutor();
            var serviceProvider = new TestServiceProvider
            {
                ServiceToReturn = executor
            };
            httpContext.RequestServices = serviceProvider;

            // Act
            await objectResult.ExecuteResultAsync(actionContext);

            // Assert
            Assert.Equal(typeof(IActionResultExecutor<ObjectResult>), serviceProvider.RequestedType);
            Assert.True(executor.Executed);
            Assert.Same(actionContext, executor.CapturedActionContext);
            Assert.Same(objectResult, executor.CapturedResult);
        }

        [Fact]
        public async Task ExecuteResultAsync_ThrowsWhenExecutorNotRegistered()
        {
            // Arrange
            var objectResult = new ObjectResult("value");
            var httpContext = new DefaultHttpContext();
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            var serviceProvider = new TestServiceProvider();
            httpContext.RequestServices = serviceProvider;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => objectResult.ExecuteResultAsync(actionContext));
            Assert.Equal(typeof(IActionResultExecutor<ObjectResult>), serviceProvider.RequestedType);
            Assert.Contains(typeof(IActionResultExecutor<ObjectResult>).FullName, exception.Message);
        }

        private sealed class TestServiceProvider : IServiceProvider
        {
            public Type? RequestedType { get; private set; }
            public object? ServiceToReturn { get; set; }

            public object? GetService(Type serviceType)
            {
                RequestedType = serviceType;
                return ServiceToReturn;
            }
        }

        private sealed class TestObjectResultExecutor : IActionResultExecutor<ObjectResult>
        {
            public bool Executed { get; private set; }
            public ActionContext? CapturedActionContext { get; private set; }
            public ObjectResult? CapturedResult { get; private set; }

            public Task ExecuteAsync(ActionContext context, ObjectResult result)
            {
                Executed = true;
                CapturedActionContext = context;
                CapturedResult = result;
                return Task.CompletedTask;
            }
        }
    }
}
