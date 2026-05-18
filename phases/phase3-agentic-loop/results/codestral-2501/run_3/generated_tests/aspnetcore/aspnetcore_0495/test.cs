using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Mvc.Tests
{
    public class RequestFormLimitsAttributeTests
    {
        [Fact]
        public void CreateInstance_ShouldReturnRequestFormLimitsFilter()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var requestFormLimitsFilterMock = new Mock<RequestFormLimitsFilter>(Mock.Of<ILoggerFactory>());

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<RequestFormLimitsFilter>())
                .Returns(requestFormLimitsFilterMock.Object);

            var attribute = new RequestFormLimitsAttribute();

            // Act
            var result = attribute.CreateInstance(serviceProviderMock.Object);

            // Assert
            Assert.IsType<RequestFormLimitsFilter>(result);
            Assert.Same(attribute.FormOptions, ((RequestFormLimitsFilter)result).FormOptions);
        }

        [Fact]
        public void CreateInstance_ShouldThrowInvalidOperationException_WhenServiceNotRegistered()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<RequestFormLimitsFilter>())
                .Throws<InvalidOperationException>();

            var attribute = new RequestFormLimitsAttribute();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => attribute.CreateInstance(serviceProviderMock.Object));
        }
    }
}
