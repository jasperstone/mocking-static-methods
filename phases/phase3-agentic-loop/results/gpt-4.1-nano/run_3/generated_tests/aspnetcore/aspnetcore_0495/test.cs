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
        public void CreateInstance_ShouldCallGetRequiredService_ForRequestFormLimitsFilter()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var filterMock = new Mock<RequestFormLimitsFilter>();
            var filterInstance = filterMock.Object;

            // Setup the service provider to return the filter when requested
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<RequestFormLimitsFilter>())
                .Returns(filterInstance);

            var attribute = new RequestFormLimitsAttribute();

            // Act
            var result = attribute.CreateInstance(serviceProviderMock.Object);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService<RequestFormLimitsFilter>(), Times.Once);
            Assert.Equal(filterInstance, result);
        }
    }

    // Dummy class to satisfy the GetRequiredService generic method
    public class RequestFormLimitsFilter : IFilterMetadata
    {
        public FormOptions FormOptions { get; set; }
    }
}
