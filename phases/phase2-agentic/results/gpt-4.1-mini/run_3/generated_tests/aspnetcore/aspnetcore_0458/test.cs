using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc
{
    public class FormatFilterAttributeTests
    {
        [Fact]
        public void CreateInstance_ThrowsArgumentNullException_WhenServiceProviderIsNull()
        {
            // Arrange
            var attribute = new FormatFilterAttribute();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => attribute.CreateInstance(null!));
        }

        [Fact]
        public void CreateInstance_CallsGetRequiredServiceOnServiceProvider()
        {
            // Arrange
            var expectedFilter = new Mock<FormatFilter>().Object;

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(FormatFilter)))
                .Returns(expectedFilter);

            // We need to mock the extension method GetRequiredService<T>().
            // Since it's an extension method, it calls GetService and throws if null.
            // So we simulate that behavior by setting up GetService to return the expected instance.

            var attribute = new FormatFilterAttribute();

            // Act
            var result = attribute.CreateInstance(serviceProviderMock.Object);

            // Assert
            Assert.Same(expectedFilter, result);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(FormatFilter)), Times.Once);
        }
    }

    // Minimal stub for FormatFilter to allow mocking
    public class FormatFilter : IFilterMetadata
    {
    }
}
