using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc
{
    public class RequestFormLimitsAttributeTests
    {
        private class TestRequestFormLimitsFilter : IFilterMetadata
        {
            public FormOptions FormOptions { get; set; } = new FormOptions();
        }

        [Fact]
        public void CreateInstance_CallsGetRequiredServiceAndSetsFormOptions()
        {
            // Arrange
            var attribute = new RequestFormLimitsAttribute
            {
                BufferBody = true,
                MemoryBufferThreshold = 123,
                BufferBodyLengthLimit = 456,
                ValueCountLimit = 789,
                KeyLengthLimit = 10,
                ValueLengthLimit = 11,
                MultipartBoundaryLengthLimit = 12,
                MultipartHeadersCountLimit = 13,
                MultipartHeadersLengthLimit = 14,
                MultipartBodyLengthLimit = 15
            };

            var filter = new TestRequestFormLimitsFilter();

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(RequestFormLimitsFilter)))
                .Returns(filter);

            // Act
            var result = attribute.CreateInstance(serviceProviderMock.Object);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService(typeof(RequestFormLimitsFilter)), Times.Once);
            Assert.Same(filter, result);
            Assert.Equal(attribute.BufferBody, filter.FormOptions.BufferBody);
            Assert.Equal(attribute.MemoryBufferThreshold, filter.FormOptions.MemoryBufferThreshold);
            Assert.Equal(attribute.BufferBodyLengthLimit, filter.FormOptions.BufferBodyLengthLimit);
            Assert.Equal(attribute.ValueCountLimit, filter.FormOptions.ValueCountLimit);
            Assert.Equal(attribute.KeyLengthLimit, filter.FormOptions.KeyLengthLimit);
            Assert.Equal(attribute.ValueLengthLimit, filter.FormOptions.ValueLengthLimit);
            Assert.Equal(attribute.MultipartBoundaryLengthLimit, filter.FormOptions.MultipartBoundaryLengthLimit);
            Assert.Equal(attribute.MultipartHeadersCountLimit, filter.FormOptions.MultipartHeadersCountLimit);
            Assert.Equal(attribute.MultipartHeadersLengthLimit, filter.FormOptions.MultipartHeadersLengthLimit);
            Assert.Equal(attribute.MultipartBodyLengthLimit, filter.FormOptions.MultipartBodyLengthLimit);
        }
    }
}
