using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc
{
    public class ObjectResultTests
    {
        [Fact]
        public async Task ExecuteResultAsync_CallsExecutorExecuteAsync()
        {
            // Arrange
            var mockExecutor = new Mock<IActionResultExecutor<ObjectResult>>();
            var objectResult = new ObjectResult("test");
            var httpContext = new DefaultHttpContext();

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
                .Setup(sp => sp.GetService(typeof(IActionResultExecutor<ObjectResult>)))
                .Returns(mockExecutor.Object);

            // Setup RequestServices to return the mock service provider
            httpContext.RequestServices = serviceProvider.Object;

            var actionContext = new ActionContext
            {
                HttpContext = httpContext
            };

            mockExecutor
                .Setup(executor => executor.ExecuteAsync(actionContext, objectResult))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            await objectResult.ExecuteResultAsync(actionContext);

            // Assert
            mockExecutor.Verify();
        }

        [Fact]
        public void OnFormatting_SetsStatusCodeFromProblemDetails_WhenStatusCodeIsNull()
        {
            // Arrange
            var problemDetails = new ProblemDetails { Status = 400 };
            var objectResult = new ObjectResult(problemDetails)
            {
                StatusCode = null
            };

            var httpContext = new DefaultHttpContext();
            var actionContext = new ActionContext
            {
                HttpContext = httpContext
            };

            // Act
            objectResult.OnFormatting(actionContext);

            // Assert
            Assert.Equal(400, objectResult.StatusCode);
            Assert.Equal(400, httpContext.Response.StatusCode);
        }

        [Fact]
        public void OnFormatting_SetsProblemDetailsStatus_WhenObjectResultStatusCodeIsSet()
        {
            // Arrange
            var problemDetails = new ProblemDetails { Status = null };
            var objectResult = new ObjectResult(problemDetails)
            {
                StatusCode = 500
            };

            var httpContext = new DefaultHttpContext();
            var actionContext = new ActionContext
            {
                HttpContext = httpContext
            };

            // Act
            objectResult.OnFormatting(actionContext);

            // Assert
            Assert.Equal(500, problemDetails.Status);
            Assert.Equal(500, httpContext.Response.StatusCode);
        }

        [Fact]
        public void OnFormatting_SetsResponseStatusCode_WhenStatusCodeIsSet()
        {
            // Arrange
            var objectResult = new ObjectResult("value")
            {
                StatusCode = 201
            };

            var httpContext = new DefaultHttpContext();
            var actionContext = new ActionContext
            {
                HttpContext = httpContext
            };

            // Act
            objectResult.OnFormatting(actionContext);

            // Assert
            Assert.Equal(201, httpContext.Response.StatusCode);
        }

        [Fact]
        public void ContentTypes_Setter_ThrowsArgumentNullException_WhenNull()
        {
            // Arrange
            var objectResult = new ObjectResult("value");

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => objectResult.ContentTypes = null!);
        }
    }
}
