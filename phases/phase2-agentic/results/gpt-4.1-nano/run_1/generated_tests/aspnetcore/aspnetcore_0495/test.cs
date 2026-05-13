using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;

namespace RequestFormLimitsAttributeTests
{
    public class RequestFormLimitsAttributeTests
    {
        [Fact]
        public void CreateInstance_ShouldCallGetRequiredServiceAndSetFormOptions()
        {
            // Arrange
            var mockFilter = new Mock<RequestFormLimitsFilter>();
            var servicesMock = new Mock<IServiceProvider>();
            servicesMock.Setup(sp => sp.GetRequiredService<RequestFormLimitsFilter>())
                        .Returns(mockFilter.Object);

            var attribute = new RequestFormLimitsAttribute();

            // Act
            var result = attribute.CreateInstance(servicesMock.Object);

            // Assert
            servicesMock.Verify(sp => sp.GetRequiredService<RequestFormLimitsFilter>(), Times.Once);
            mockFilter.VerifySet(f => f.FormOptions = It.IsAny<FormOptions>(), Times.Once);
            Assert.Equal(mockFilter.Object, result);
        }
    }
}
