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
        public async Task ExecuteResultAsync_CallsExecutor()
        {
            // Arrange
            var mockExecutor = new Mock<IActionResultExecutor<ObjectResult>>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider
                .Setup(s => s.GetRequiredService<IActionResultExecutor<ObjectResult>>())
                .Returns(mockExecutor.Object);

            var actionContext = new ActionContext
            {
                HttpContext = new DefaultHttpContext
                {
                    RequestServices = mockServiceProvider.Object
                }
            };

            var objectResult = new ObjectResult("test value");

            // Act
            await objectResult.ExecuteResultAsync(actionContext);

            // Assert
            mockExecutor.Verify(e => e.ExecuteAsync(actionContext, objectResult), Times.Once);
        }
    }
}
