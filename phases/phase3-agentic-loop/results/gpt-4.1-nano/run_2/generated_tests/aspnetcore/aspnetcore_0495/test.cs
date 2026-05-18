using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc
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
