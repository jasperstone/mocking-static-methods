using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
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
            var attribute = new FormatFilterAttribute();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => attribute.CreateInstance(null!));
        }

        [Fact]
        public void CreateInstance_CallsGetRequiredService_OnServiceProvider()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetRequiredService<FormatFilter>())
                              .Returns(new FormatFilter())
                              .Verifiable();
            var attribute = new FormatFilterAttribute();

            // Act
            var result = attribute.CreateInstance(mockServiceProvider.Object);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetRequiredService<FormatFilter>(), Times.Once);
            Assert.IsType<FormatFilter>(result);
        }

        [Fact]
        public void IsReusable_ReturnsTrue()
        {
            // Arrange & Act
            var attribute = new FormatFilterAttribute();

            // Assert
            Assert.True(attribute.IsReusable);
        }
    }
}
