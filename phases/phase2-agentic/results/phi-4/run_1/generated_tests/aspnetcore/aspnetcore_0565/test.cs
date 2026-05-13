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
        public async Task ExecuteResultAsync_WhenExecutorIsNull_ThrowsInvalidOperationException()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            var services = new Mock<IServiceProvider>();
            var executor = services.Object.GetService(typeof(IActionResultExecutor<PartialViewResult>)) as IActionResultExecutor<PartialViewResult>;
            services.Setup(s => s.GetService(typeof(IActionResultExecutor<PartialViewResult>))).Returns(executor);

            var partialViewResult = new PartialViewResult
            {
                ViewEngine = null // Ensure no default view engine is used
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => partialViewResult.ExecuteResultAsync(actionContext));
        }
    }
}
