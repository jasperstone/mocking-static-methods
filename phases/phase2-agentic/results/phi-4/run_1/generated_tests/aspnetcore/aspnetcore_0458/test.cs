using System;
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
        public void CreateInstance_WhenCalled_ReturnsFormatFilter()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var formatFilterMock = new Mock<FormatFilter>();
            mockServiceProvider
                .Setup(sp => sp.GetRequiredService<FormatFilter>())
                .Returns(formatFilterMock.Object);

            var formatFilterAttribute = new FormatFilterAttribute();

            // Act
            var result = formatFilterAttribute.CreateInstance(mockServiceProvider.Object);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<FormatFilter>(result);
            mockServiceProvider.Verify(sp => sp.GetRequiredService<FormatFilter>(), Times.Once);
        }
    }
}
