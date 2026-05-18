using Xunit;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNetCore.Mvc.Core.Tests
{
    public class RequestFormLimitsAttributeTests
    {
        [Fact]
        public void CreateInstance_ShouldReturnRequestFormLimitsFilter()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var filterMock = new Mock<RequestFormLimitsFilter>(MockBehavior.Strict, new Mock<ILoggerFactory>().Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<RequestFormLimitsFilter>()).Returns(filterMock.Object);

            var attribute = new RequestFormLimitsAttribute();

            // Act
            var result = attribute.CreateInstance(serviceProviderMock.Object);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<RequestFormLimitsFilter>(result);
            Assert.Same(filterMock.Object, result);
        }
    }
}
