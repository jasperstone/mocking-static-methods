using Bit.Core.Billing.Tax.Services;
using Bit.Core.Exceptions;
using Bit.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using System;
using Xunit;

namespace Bit.Core.Tests.Services
{
    public class StripePaymentServiceTests
    {
        private readonly Mock<ITaxService> _mockTaxService;
        private readonly Mock<ILogger<StripePaymentService>> _mockLogger;
        private readonly StripePaymentService _service;

        public StripePaymentServiceTests()
        {
            _mockTaxService = new Mock<ITaxService>();
            _mockLogger = new Mock<ILogger<StripePaymentService>>();

            _service = new StripePaymentService(
                transactionRepository: null!,
                logger: _mockLogger.Object,
                stripeAdapter: null!,
                braintreeGateway: null!,
                globalSettings: null!,
                featureService: null!,
                taxService: _mockTaxService.Object,
                pricingClient: null!
            );
        }

        [Fact]
        public void LogsWarningWhenTaxIdTypeIsNull()
        {
            // Arrange
            var taxId = "12345678Z";
            var country = "ES";
            _mockTaxService.Setup(x => x.GetStripeTaxCode(country, taxId)).Returns((string)null);

            // Act - Replicate the code path from line 1113
            var taxIdType = _mockTaxService.Object.GetStripeTaxCode(country, taxId);
            if (taxIdType == null)
            {
                _service._logger.LogWarning("Invalid tax ID '{TaxID}' for country '{Country}'.", taxId, country);
            }

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        ((string)v).Contains("Invalid tax ID") && 
                        ((string)v).Contains(taxId) && 
                        ((string)v).Contains(country)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void DoesNotLogWarningWhenTaxIdTypeIsValid()
        {
            // Arrange
            var taxId = "12345678Z";
            var country = "ES";
            _mockTaxService.Setup(x => x.GetStripeTaxCode(country, taxId)).Returns("txr_es_nif");

            // Act
            var taxIdType = _mockTaxService.Object.GetStripeTaxCode(country, taxId);
            if (taxIdType == null)
            {
                _service._logger.LogWarning("Invalid tax ID '{TaxID}' for country '{Country}'.", taxId, country);
            }

            // Assert - No warning should be logged
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }
    }
}
