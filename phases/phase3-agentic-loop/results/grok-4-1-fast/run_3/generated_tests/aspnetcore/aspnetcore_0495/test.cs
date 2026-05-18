using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc
{
    public class RequestFormLimitsAttributeTests
    {
        [Fact]
        public void CreateInstance_WhenCalled_ReturnsFilterWithFormOptions()
        {
            // Arrange
            var attribute = new RequestFormLimitsAttribute();
            attribute.ValueCountLimit = 100;

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var expectedFilter = new RequestFormLimitsFilter(loggerFactoryMock.Object);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<RequestFormLimitsFilter>())
                .Returns(expectedFilter)
                .Verifiable();

            // Act
            var result = attribute.CreateInstance(serviceProviderMock.Object);

            // Assert
            Assert.Same(expectedFilter, result);
            Assert.Same(attribute.FormOptions, expectedFilter.FormOptions);
            serviceProviderMock.Verify();
        }

        [Fact]
        public void CreateInstance_WhenServiceProviderIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            var attribute = new RequestFormLimitsAttribute();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => attribute.CreateInstance(null!));
        }
    }
}
