using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Tests
{
    public class FormatFilterAttributeTests
    {
        [Fact]
        public void CreateInstance_GetRequiredService_Called()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var formatFilterMock = new Mock<IFilterMetadata>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(FormatFilter))).Returns(formatFilterMock.Object);

            var formatFilterAttribute = new FormatFilterAttribute();

            // Act
            var result = formatFilterAttribute.CreateInstance(serviceProviderMock.Object);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService(typeof(FormatFilter)), Times.Once);
            Assert.Same(formatFilterMock.Object, result);
        }

        [Fact]
        public void CreateInstance_ServiceProviderNull_Throws()
        {
            // Arrange
            var formatFilterAttribute = new FormatFilterAttribute();

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => formatFilterAttribute.CreateInstance(null));
        }
    }
}
