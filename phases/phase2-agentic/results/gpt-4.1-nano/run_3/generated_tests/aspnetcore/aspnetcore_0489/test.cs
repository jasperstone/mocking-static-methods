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
        public async Task ExecuteResultAsync_CallsExecutorExecuteAsync()
        {
            // Arrange
            var mockExecutor = new Mock<IActionResultExecutor<ObjectResult>>();
            mockExecutor
                .Setup(e => e.ExecuteAsync(It.IsAny<ActionContext>(), It.IsAny<ObjectResult>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var services = new ServiceCollection();
            services.AddSingleton(mockExecutor.Object);
            var serviceProvider = services.BuildServiceProvider();

            var context = new ActionContext
            {
                HttpContext = new DefaultHttpContext()
                {
                    RequestServices = serviceProvider
                }
            };

            var objectResult = new ObjectResult("test");
            var mockExecutorInstance = mockExecutor.Object;

            // Act
            await objectResult.ExecuteResultAsync(context);

            // Assert
            mockExecutor.Verify(e => e.ExecuteAsync(It.Is<ActionContext>(c => c == context), It.Is<ObjectResult>(r => r == objectResult)), Times.Once);
        }

        [Fact]
        public void OnFormatting_SetsStatusCodeFromProblemDetails()
        {
            // Arrange
            var objectResult = new ObjectResult(new ProblemDetails { Status = 404 });
            var context = new ActionContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            objectResult.OnFormatting(context);

            // Assert
            Assert.Equal(404, context.HttpContext.Response.StatusCode);
        }

        [Fact]
        public void OnFormatting_SetsProblemDetailsStatusFromObjectResult()
        {
            // Arrange
            var details = new ProblemDetails { Status = null };
            var objectResult = new ObjectResult(details)
            {
                StatusCode = 500
            };
            var context = new ActionContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            objectResult.OnFormatting(context);

            // Assert
            Assert.Equal(500, details.Status);
            Assert.Equal(500, context.HttpContext.Response.StatusCode);
        }

        [Fact]
        public void OnFormatting_SetsResponseStatusCode_WhenStatusCodeHasValue()
        {
            // Arrange
            var objectResult = new ObjectResult(null)
            {
                StatusCode = 201
            };
            var context = new ActionContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            objectResult.OnFormatting(context);

            // Assert
            Assert.Equal(201, context.HttpContext.Response.StatusCode);
        }
    }
}
