using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Tests
{
    public class RequestFormLimitsAttributeTests
    {
        [Fact]
        public void CreateInstance_ShouldRetrieveAndConfigureRequestFormLimitsFilter()
        {
            // Arrange
            var formOptions = new FormOptions
            {
                BufferBody = true,
                MemoryBufferThreshold = 1024,
                BufferBodyLengthLimit = 1048576,
                ValueCountLimit = 100,
                KeyLengthLimit = 255,
                ValueLengthLimit = 4096,
                MultipartBoundaryLengthLimit = 1024,
                MultipartHeadersCountLimit = 100,
                MultipartHeadersLengthLimit = 8192,
                MultipartBodyLengthLimit = 10485760
            };

            var requestFormLimitsAttribute = new RequestFormLimitsAttribute
            {
                FormOptions = formOptions
            };

            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockFilter = new Mock<RequestFormLimitsFilter>();
            mockServiceProvider.Setup(s => s.GetRequiredService<RequestFormLimitsFilter>()).Returns(mockFilter.Object);

            // Act
            var filter = (RequestFormLimitsFilter)requestFormLimitsAttribute.CreateInstance(mockServiceProvider.Object);

            // Assert
            Assert.Same(formOptions, filter.FormOptions);
            mockServiceProvider.Verify(s => s.GetRequiredService<RequestFormLimitsFilter>(), Times.Once);
        }
    }
}
