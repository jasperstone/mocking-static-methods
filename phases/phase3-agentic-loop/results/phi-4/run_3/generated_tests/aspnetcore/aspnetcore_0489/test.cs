using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Tests
{
    public class ObjectResultTests
    {
        [Fact]
        public async Task ExecuteResultAsync_CallsGetRequiredService()
        {
            // Arrange
            var mockExecutor = new Mock<IActionResultExecutor<ObjectResult>>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider
                .Setup(s => s.GetRequiredService<IActionResultExecutor<ObjectResult>>())
                .Returns(mockExecutor.Object);

            var mockHttpContext = new Mock<HttpContext>();
            var mockActionDescriptor = new Mock<ActionDescriptor>();
            var mockActionContext = new ActionContext(
                new DefaultHttpContext { RequestServices = mockServiceProvider.Object },
                mockActionDescriptor.Object,
                new RouteData());

            var objectResult = new ObjectResult("test value")
            {
                DeclaredType = typeof(string)
            };

            // Act
            await objectResult.ExecuteResultAsync(mockActionContext);

            // Assert
            mockExecutor.Verify(e => e.ExecuteAsync(It.IsAny<ActionContext>(), It.IsAny<ObjectResult>()), Times.Once);
        }
    }
}
