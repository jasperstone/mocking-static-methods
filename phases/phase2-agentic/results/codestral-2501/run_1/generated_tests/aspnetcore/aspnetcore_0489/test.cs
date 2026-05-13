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
        public async Task ExecuteResultAsync_ShouldCallExecutorExecuteAsync()
        {
            // Arrange
            var mockExecutor = new Mock<IActionResultExecutor<ObjectResult>>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockHttpContext = new Mock<HttpContext>();
            var mockActionContext = new Mock<ActionContext>();

            mockServiceProvider
                .Setup(sp => sp.GetRequiredService<IActionResultExecutor<ObjectResult>>())
                .Returns(mockExecutor.Object);

            mockHttpContext.Setup(hc => hc.RequestServices).Returns(mockServiceProvider.Object);
            mockActionContext.Setup(ac => ac.HttpContext).Returns(mockHttpContext.Object);

            var objectResult = new ObjectResult(null);

            // Act
            await objectResult.ExecuteResultAsync(mockActionContext.Object);

            // Assert
            mockExecutor.Verify(executor => executor.ExecuteAsync(mockActionContext.Object, objectResult), Times.Once);
        }

        [Fact]
        public void OnFormatting_ShouldSetStatusCodeFromProblemDetails()
        {
            // Arrange
            var mockHttpContext = new Mock<HttpContext>();
            var mockActionContext = new Mock<ActionContext>();
            var problemDetails = new ProblemDetails { Status = 404 };
            var objectResult = new ObjectResult(problemDetails);

            mockActionContext.Setup(ac => ac.HttpContext).Returns(mockHttpContext.Object);

            // Act
            objectResult.OnFormatting(mockActionContext.Object);

            // Assert
            Assert.Equal(404, objectResult.StatusCode);
            mockHttpContext.VerifySet(hc => hc.Response.StatusCode = 404);
        }

        [Fact]
        public void OnFormatting_ShouldSetProblemDetailsStatusFromStatusCode()
        {
            // Arrange
            var mockHttpContext = new Mock<HttpContext>();
            var mockActionContext = new Mock<ActionContext>();
            var problemDetails = new ProblemDetails();
            var objectResult = new ObjectResult(problemDetails) { StatusCode = 404 };

            mockActionContext.Setup(ac => ac.HttpContext).Returns(mockHttpContext.Object);

            // Act
            objectResult.OnFormatting(mockActionContext.Object);

            // Assert
            Assert.Equal(404, problemDetails.Status);
            mockHttpContext.VerifySet(hc => hc.Response.StatusCode = 404);
        }

        [Fact]
        public void OnFormatting_ShouldSetResponseStatusCode()
        {
            // Arrange
            var mockHttpContext = new Mock<HttpContext>();
            var mockActionContext = new Mock<ActionContext>();
            var objectResult = new ObjectResult(null) { StatusCode = 404 };

            mockActionContext.Setup(ac => ac.HttpContext).Returns(mockHttpContext.Object);

            // Act
            objectResult.OnFormatting(mockActionContext.Object);

            // Assert
            mockHttpContext.VerifySet(hc => hc.Response.StatusCode = 404);
        }
    }
}
