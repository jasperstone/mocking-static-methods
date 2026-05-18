using Moq;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Tests
{
    public class RequestFormLimitsAttributeTests
    {
        [Fact]
        public void CreateInstance_ShouldRetrieveAndConfigureRequestFormLimitsFilter()
        {
            // Arrange
            var formOptions = new Microsoft.AspNetCore.Http.Features.FormOptions
            {
                BufferBody = true,
                MemoryBufferThreshold = 1024,
                BufferBodyLengthLimit = 1048576,
                ValueCountLimit = 100,
                KeyLengthLimit = 128,
                ValueLengthLimit = 256,
                MultipartBoundaryLengthLimit = 1024,
                MultipartHeadersCountLimit = 10,
                MultipartHeadersLengthLimit = 2048,
                MultipartBodyLengthLimit = 10485760
            };

            var requestFormLimitsAttribute = new Microsoft.AspNetCore.Mvc.RequestFormLimitsAttribute
            {
                FormOptions = formOptions
            };

            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockLogger = new Mock<ILogger>();
            mockLoggerFactory.Setup(f => f.CreateLogger(typeof(Microsoft.AspNetCore.Mvc.Filters.RequestFormLimitsFilter))).Returns(mockLogger.Object);

            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockFilter = new Mock<Microsoft.AspNetCore.Mvc.Filters.RequestFormLimitsFilter>();
            mockFilter.Setup(f => f.FormOptions).Returns(formOptions);
            mockServiceProvider.Setup(s => s.GetRequiredService<Microsoft.AspNetCore.Mvc.Filters.RequestFormLimitsFilter>()).Returns(mockFilter.Object);

            // Act
            var filter = (Microsoft.AspNetCore.Mvc.Filters.RequestFormLimitsFilter)requestFormLimitsAttribute.CreateInstance(mockServiceProvider.Object);

            // Assert
            Assert.Same(mockFilter.Object, filter);
            Assert.Equal(formOptions, filter.FormOptions);
        }
    }
}
