using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Tests
{
    public class ObjectResultTests
    {
        [Fact]
        public async Task ExecuteResultAsync_ShouldCallExecutor()
        {
            // Arrange
            var mockExecutor = new Mock<IActionResultExecutor<ObjectResult>>();
            mockExecutor.Setup(e => e.ExecuteAsync(It.IsAny<ActionContext>(), It.IsAny<ObjectResult>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(sp => sp.GetRequiredService<IActionResultExecutor<ObjectResult>>())
                .Returns(mockExecutor.Object);

            var httpContext = new DefaultHttpContext
            {
                RequestServices = serviceProvider.Object
            };

            var actionContext = new ActionContext
            {
                HttpContext = httpContext
            };

            var objectResult = new ObjectResult("TestValue");

            // Act
            await objectResult.ExecuteResultAsync(actionContext);

            // Assert
            mockExecutor.Verify(e => e.ExecuteAsync(actionContext, objectResult), Times.Once());
        }

        [Fact]
        public void OnFormatting_ShouldSetStatusCodeFromProblemDetails()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var actionContext = new ActionContext
            {
                HttpContext = httpContext
            };

            var problemDetails = new ProblemDetails
            {
                Status = 404
            };

            var objectResult = new ObjectResult(problemDetails);

            // Act
            objectResult.OnFormatting(actionContext);

            // Assert
            Assert.Equal(404, objectResult.StatusCode);
            Assert.Equal(404, httpContext.Response.StatusCode);
        }

        [Fact]
        public void OnFormatting_ShouldSetStatusCodeToProblemDetails()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var actionContext = new ActionContext
            {
                HttpContext = httpContext
            };

            var problemDetails = new ProblemDetails();

            var objectResult = new ObjectResult(problemDetails)
            {
                StatusCode = 404
            };

            // Act
            objectResult.OnFormatting(actionContext);

            // Assert
            Assert.Equal(404, problemDetails.Status);
            Assert.Equal(404, httpContext.Response.StatusCode);
        }

        [Fact]
        public void OnFormatting_ShouldSetResponseStatusCode()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var actionContext = new ActionContext
            {
                HttpContext = httpContext
            };

            var objectResult = new ObjectResult("TestValue")
            {
                StatusCode = 200
            };

            // Act
            objectResult.OnFormatting(actionContext);

            // Assert
            Assert.Equal(200, httpContext.Response.StatusCode);
        }
    }
}
