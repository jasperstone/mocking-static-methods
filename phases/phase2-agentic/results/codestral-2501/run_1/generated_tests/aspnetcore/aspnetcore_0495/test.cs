using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Tests
{
    public class RequestFormLimitsAttributeTests
    {
        [Fact]
        public void CreateInstance_ShouldReturnRequestFormLimitsFilter()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockFilter = new Mock<RequestFormLimitsFilter>();

            mockServiceProvider
                .Setup(sp => sp.GetRequiredService<RequestFormLimitsFilter>())
                .Returns(mockFilter.Object);

            var attribute = new RequestFormLimitsAttribute();

            // Act
            var result = attribute.CreateInstance(mockServiceProvider.Object);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetRequiredService<RequestFormLimitsFilter>(), Times.Once);
            Assert.Same(mockFilter.Object, result);
            Assert.Same(attribute.FormOptions, mockFilter.Object.FormOptions);
        }
    }

    // Mock implementation of RequestFormLimitsFilter for testing purposes
    public class RequestFormLimitsFilter : IFilterMetadata
    {
        public FormOptions FormOptions { get; set; }
    }
}
