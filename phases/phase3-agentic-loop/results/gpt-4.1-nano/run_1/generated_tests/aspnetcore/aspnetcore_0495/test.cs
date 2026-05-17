using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;

namespace RequestFormLimitsAttributeTests
{
    public class RequestFormLimitsAttributeTests
    {
        [Fact]
        public void CreateInstance_Should_Call_GetRequiredService_And_Return_Filter()
        {
            // Arrange
            var mockFilter = new Mock<RequestFormLimitsFilter>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<RequestFormLimitsFilter>())
                .Returns(mockFilter.Object);

            var attribute = new RequestFormLimitsAttribute();

            // Act
            var result = attribute.CreateInstance(serviceProviderMock.Object);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService<RequestFormLimitsFilter>(), Times.Once);
            Assert.Equal(mockFilter.Object, result);
        }
    }
}
