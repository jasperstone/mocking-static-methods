using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;

namespace Tests
{
    public class RequestFormLimitsAttributeTests
    {
        [Fact]
        public void CreateInstance_ShouldReturnRequestFormLimitsFilter()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockFilter = new Mock<RequestFormLimitsFilter>(Mock.Of<ILoggerFactory>());
            mockServiceProvider.Setup(sp => sp.GetRequiredService<RequestFormLimitsFilter>()).Returns(mockFilter.Object);

            var attribute = new RequestFormLimitsAttribute();

            // Act
            var result = attribute.CreateInstance(mockServiceProvider.Object);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<RequestFormLimitsFilter>(result);
            Assert.Same(mockFilter.Object.FormOptions, attribute.FormOptions);
        }

        [Fact]
        public void CreateInstance_ShouldSetFormOptionsOnFilter()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockFilter = new Mock<RequestFormLimitsFilter>(Mock.Of<ILoggerFactory>());
            mockServiceProvider.Setup(sp => sp.GetRequiredService<RequestFormLimitsFilter>()).Returns(mockFilter.Object);

            var attribute = new RequestFormLimitsAttribute
            {
                BufferBody = true,
                MemoryBufferThreshold = 1024,
                BufferBodyLengthLimit = 2048,
                ValueCountLimit = 10,
                KeyLengthLimit = 256,
                ValueLengthLimit = 512,
                MultipartBoundaryLengthLimit = 128,
                MultipartHeadersCountLimit = 20,
                MultipartHeadersLengthLimit = 512,
                MultipartBodyLengthLimit = 1024
            };

            // Act
            var result = attribute.CreateInstance(mockServiceProvider.Object);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<RequestFormLimitsFilter>(result);
            Assert.True(mockFilter.Object.FormOptions.BufferBody);
            Assert.Equal(1024, mockFilter.Object.FormOptions.MemoryBufferThreshold);
            Assert.Equal(2048, mockFilter.Object.FormOptions.BufferBodyLengthLimit);
            Assert.Equal(10, mockFilter.Object.FormOptions.ValueCountLimit);
            Assert.Equal(256, mockFilter.Object.FormOptions.KeyLengthLimit);
            Assert.Equal(512, mockFilter.Object.FormOptions.ValueLengthLimit);
            Assert.Equal(128, mockFilter.Object.FormOptions.MultipartBoundaryLengthLimit);
            Assert.Equal(20, mockFilter.Object.FormOptions.MultipartHeadersCountLimit);
            Assert.Equal(512, mockFilter.Object.FormOptions.MultipartHeadersLengthLimit);
            Assert.Equal(1024, mockFilter.Object.FormOptions.MultipartBodyLengthLimit);
        }
    }
}
