using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Tests
{
    public class FormatFilterAttributeTests
    {
        [Fact]
        public void CreateInstance_ThrowsArgumentNullException_WhenServiceProviderIsNull()
        {
            // Arrange
            FormatFilterAttribute attribute = new FormatFilterAttribute();

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => attribute.CreateInstance(null));
        }

        [Fact]
        public void CreateInstance_ReturnsFormatFilter_WhenServiceProviderIsNotNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var formatFilterMock = new Mock<FormatFilter>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<FormatFilter>()).Returns(formatFilterMock.Object);
            FormatFilterAttribute attribute = new FormatFilterAttribute();

            // Act
            var result = attribute.CreateInstance(serviceProviderMock.Object);

            // Assert
            Assert.Same(formatFilterMock.Object, result);
        }
    }
}
