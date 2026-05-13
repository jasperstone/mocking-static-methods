using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Tests
{
    public class RequestFormLimitsAttributeTests
    {
        [Fact]
        public void CreateInstance_ShouldReturnRequestFormLimitsFilter_WithCorrectFormOptions()
        {
            // Arrange
            var formOptions = new FormOptions
            {
                MultipartHeadersCountLimit = 10,
                MultipartHeadersLengthLimit = 1024,
                MultipartBodyLengthLimit = 1048576
            };

            var optionsMock = new Mock<IOptions<FormOptions>>();
            optionsMock.Setup(o => o.Value).Returns(formOptions);

            var serviceProviderMock = new Mock<IServiceProvider>();
            var filterMock = new Mock<RequestFormLimitsFilter>();
            serviceProviderMock.Setup(s => s.GetRequiredService<RequestFormLimitsFilter>()).Returns(filterMock.Object);

            var attribute = new RequestFormLimitsAttribute(optionsMock.Object);

            // Act
            var result = (RequestFormLimitsFilter)attribute.CreateInstance(serviceProviderMock.Object);

            // Assert
            Assert.Same(filterMock.Object, result);
            Assert.Equal(formOptions, result.FormOptions);
        }
    }
}
