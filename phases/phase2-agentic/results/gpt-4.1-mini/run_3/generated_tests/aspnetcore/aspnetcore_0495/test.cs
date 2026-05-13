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
        [Fact]
        public void CreateInstance_CallsGetRequiredServiceAndSetsFormOptions()
        {
            // Arrange
            var expectedFormOptions = new FormOptions
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

            var attribute = new RequestFormLimitsAttribute();
            attribute.BufferBody = expectedFormOptions.BufferBody;
            attribute.MemoryBufferThreshold = expectedFormOptions.MemoryBufferThreshold;
            attribute.BufferBodyLengthLimit = expectedFormOptions.BufferBodyLengthLimit;
            attribute.ValueCountLimit = expectedFormOptions.ValueCountLimit;
            attribute.KeyLengthLimit = expectedFormOptions.KeyLengthLimit;
            attribute.ValueLengthLimit = expectedFormOptions.ValueLengthLimit;
            attribute.MultipartBoundaryLengthLimit = expectedFormOptions.MultipartBoundaryLengthLimit;
            attribute.MultipartHeadersCountLimit = expectedFormOptions.MultipartHeadersCountLimit;
            attribute.MultipartHeadersLengthLimit = expectedFormOptions.MultipartHeadersLengthLimit;
            attribute.MultipartBodyLengthLimit = expectedFormOptions.MultipartBodyLengthLimit;

            var mockFilter = new Mock<RequestFormLimitsFilter>(MockBehavior.Strict, new Mock<Microsoft.Extensions.Logging.ILoggerFactory>().Object);
            mockFilter.SetupProperty(f => f.FormOptions);

            var serviceProviderMock = new Mock<IServiceProvider>(MockBehavior.Strict);
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(RequestFormLimitsFilter)))
                .Returns(mockFilter.Object);
            // Setup GetRequiredService extension method behavior by mocking GetService and throwing if null
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<RequestFormLimitsFilter>())
                .Returns(mockFilter.Object);

            // Act
            var filter = attribute.CreateInstance(serviceProviderMock.Object);

            // Assert
            Assert.Same(mockFilter.Object, filter);
            Assert.Equal(expectedFormOptions.BufferBody, mockFilter.Object.FormOptions.BufferBody);
            Assert.Equal(expectedFormOptions.MemoryBufferThreshold, mockFilter.Object.FormOptions.MemoryBufferThreshold);
            Assert.Equal(expectedFormOptions.BufferBodyLengthLimit, mockFilter.Object.FormOptions.BufferBodyLengthLimit);
            Assert.Equal(expectedFormOptions.ValueCountLimit, mockFilter.Object.FormOptions.ValueCountLimit);
            Assert.Equal(expectedFormOptions.KeyLengthLimit, mockFilter.Object.FormOptions.KeyLengthLimit);
            Assert.Equal(expectedFormOptions.ValueLengthLimit, mockFilter.Object.FormOptions.ValueLengthLimit);
            Assert.Equal(expectedFormOptions.MultipartBoundaryLengthLimit, mockFilter.Object.FormOptions.MultipartBoundaryLengthLimit);
            Assert.Equal(expectedFormOptions.MultipartHeadersCountLimit, mockFilter.Object.FormOptions.MultipartHeadersCountLimit);
            Assert.Equal(expectedFormOptions.MultipartHeadersLengthLimit, mockFilter.Object.FormOptions.MultipartHeadersLengthLimit);
            Assert.Equal(expectedFormOptions.MultipartBodyLengthLimit, mockFilter.Object.FormOptions.MultipartBodyLengthLimit);

            serviceProviderMock.Verify(sp => sp.GetRequiredService<RequestFormLimitsFilter>(), Times.Once);
        }
    }

    // Extension method to mock GetRequiredService for IServiceProvider
    internal static class ServiceProviderExtensions
    {
        public static T GetRequiredService<T>(this IServiceProvider provider)
        {
            var service = provider.GetService(typeof(T));
            if (service == null)
            {
                throw new InvalidOperationException($"Service of type {typeof(T)} not found.");
            }
            return (T)service;
        }
    }
}
