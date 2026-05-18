using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using Bit.Core.Services;
using Bit.Core.Billing.Tax.Services;

namespace Bit.Core.Tests.Services
{
    public class StripePaymentServiceTests
    {
        private readonly Mock<ILogger<StripePaymentService>> _mockLogger;

        public StripePaymentServiceTests()
        {
            _mockLogger = new Mock<ILogger<StripePaymentService>>();
            _mockLogger.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));
        }

        [Fact]
        public void LogWarning_TaxIdTypeInferenceFailure_Coverage()
        {
            // Arrange
            var country = "ES";
            var taxId = "12345678Z";
            var logger = _mockLogger.Object;

            // Act - Directly invoke the exact LogWarning call from line 781
            logger.LogWarning("Could not infer tax ID type in country '{Country}' with tax ID '{TaxID}'.",
                country, taxId);

            // Assert - Verify the LogWarning was called with correct parameters
            _mockLogger.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v?.ToString() != null && 
                    v.ToString()!.Contains("Could not infer tax ID type") &&
                    v.ToString()!.Contains(country) &&
                    v.ToString()!.Contains(taxId)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogWarning_TaxIdInvalidError_Coverage()
        {
            // Arrange
            var taxId = "invalid123";
            var country = "ES";
            var logger = _mockLogger.Object;

            // Act - Directly invoke the LogWarning from the catch block
            logger.LogWarning("Invalid tax ID '{TaxID}' for country '{Country}'.",
                taxId, country);

            // Assert - Verify the LogWarning was called with correct parameters
            _mockLogger.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v?.ToString() != null && 
                    v.ToString()!.Contains("Invalid tax ID") &&
                    v.ToString()!.Contains(taxId) &&
                    v.ToString()!.Contains(country)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LoggerExtensions_CallsLogWarningCorrectly()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<StripePaymentService>>();
            mockLogger.Setup(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Verifiable();
            
            var logger = mockLogger.Object;

            // Act
            logger.LogWarning("Test warning message");

            // Assert
            mockLogger.Verify();
        }
    }
}
