using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Tests
{
    public class RequestFormLimitsAttributeTests
    {
        [Fact]
        public void CreateInstance_GetRequiredService_ReturnsRequestFormLimitsFilter()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddScoped<RequestFormLimitsFilter>()
                .BuildServiceProvider();

            var attribute = new RequestFormLimitsAttribute();

            // Act
            var filter = attribute.CreateInstance(serviceProvider);

            // Assert
            Assert.IsType<RequestFormLimitsFilter>(filter);
        }

        [Fact]
        public void CreateInstance_GetRequiredService_SetsFormOptions()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddScoped<RequestFormLimitsFilter>()
                .BuildServiceProvider();

            var attribute = new RequestFormLimitsAttribute
            {
                BufferBody = true,
                MemoryBufferThreshold = 1024,
                BufferBodyLengthLimit = 1024,
                ValueCountLimit = 10,
                KeyLengthLimit = 10,
                ValueLengthLimit = 10,
                MultipartBoundaryLengthLimit = 10,
                MultipartHeadersCountLimit = 10,
                MultipartHeadersLengthLimit = 10,
                MultipartBodyLengthLimit = 1024,
            };

            // Act
            var filter = (RequestFormLimitsFilter)attribute.CreateInstance(serviceProvider);

            // Assert
            Assert.True(filter.FormOptions.BufferBody);
            Assert.Equal(1024, filter.FormOptions.MemoryBufferThreshold);
            Assert.Equal(1024, filter.FormOptions.BufferBodyLengthLimit);
            Assert.Equal(10, filter.FormOptions.ValueCountLimit);
            Assert.Equal(10, filter.FormOptions.KeyLengthLimit);
            Assert.Equal(10, filter.FormOptions.ValueLengthLimit);
            Assert.Equal(10, filter.FormOptions.MultipartBoundaryLengthLimit);
            Assert.Equal(10, filter.FormOptions.MultipartHeadersCountLimit);
            Assert.Equal(10, filter.FormOptions.MultipartHeadersLengthLimit);
            Assert.Equal(1024, filter.FormOptions.MultipartBodyLengthLimit);
        }
    }
}
