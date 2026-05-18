using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Microsoft.AspNetCore.Mvc.Tests
{
    public class FormatFilterAttributeTests
    {
        [Fact]
        public void CreateInstance_WhenCalled_ReturnsCorrectType()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var formatFilterMock = new Mock<IFilterMetadata>();
            mockServiceProvider
                .Setup(sp => sp.GetRequiredService<IFilterMetadata>())
                .Returns(formatFilterMock.Object);

            var formatFilterAttribute = new FormatFilterAttribute();

            // Act
            var result = formatFilterAttribute.CreateInstance(mockServiceProvider.Object);

            // Assert
            Assert.IsType<IFilterMetadata>(result);
            mockServiceProvider.Verify(sp => sp.GetRequiredService<IFilterMetadata>(), Times.Once);
        }
    }
}
