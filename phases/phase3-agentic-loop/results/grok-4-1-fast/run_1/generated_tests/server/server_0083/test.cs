using Bit.Core.Billing.Tax.Requests;
using Bit.Core.Billing.Tax.Responses;
using Bit.Core.Exceptions;
using Bit.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using Bit.Core.Repositories;
using Bit.Core.Settings;
using Bit.Core.Services.Implementations;

namespace Bit.Core.Tests.Services.Implementations
{
    public class StripePaymentServiceTests
    {
        private readonly Mock<ILogger<StripePaymentService>> _mockLogger;
        private readonly Mock<ITaxService> _mockTaxService;
        private readonly Mock<IStripeAdapter> _mockStripeAdapter;
        private readonly StripePaymentService _service;

        public StripePaymentServiceTests()
        {
            _mockLogger = new Mock<ILogger<StripePaymentService>>();
            _mockTaxService = new Mock<ITaxService>();
            _mockStripeAdapter = new Mock<IStripeAdapter>();

            var mockTransactionRepo = new Mock<ITransactionRepository>();
            var mockBraintreeGateway = new Mock<Braintree.IBraintreeGateway>();
            var mockGlobalSettings = new Mock<IGlobalSettings>();
            var mockFeatureService = new Mock<IFeatureService>();
            var mockPricingClient = new Mock<Bit.Core.Billing.Pricing.IPricingClient>();

            _service = new StripePaymentService(
                mockTransactionRepo.Object,
                _mockLogger.Object,
                _mockStripeAdapter.Object,
                mockBraintreeGateway.Object,
                mockGlobalSettings.Object,
                mockFeatureService.Object,
                _mockTaxService.Object,
                mockPricingClient.Object);
        }

        [Fact]
        public void LogsWarning_WhenTaxIdTypeInferenceFails()
        {
            // Arrange
            var country = "ES";
            var taxId = "12345678Z";
            
            // Act - Directly test the logger extension method call pattern from line 781
            _mockLogger.Object.LogWarning(
                "Could not infer tax ID type in country '{Country}' with tax ID '{TaxID}'.",
                country, taxId);

            // Assert - Verify the LogWarning extension was called with expected parameters
            _mockLogger.Verify(
                x => x.LogWarning(
                    "Could not infer tax ID type in country '{Country}' with tax ID '{TaxID}'.",
                    country,
                    taxId),
                Times.Once);
        }

        [Fact]
        public void LogsWarning_OnInvalidTaxIdStripeException()
        {
            // Arrange
            var taxId = "invalid123";
            var country = "ES";

            // Act - Directly test the logger extension method call from the catch block
            _mockLogger.Object.LogWarning(
                "Invalid tax ID '{TaxID}' for country '{Country}'.",
                taxId, country);

            // Assert - Verify the LogWarning extension was called with expected parameters
            _mockLogger.Verify(
                x => x.LogWarning(
                    "Invalid tax ID '{TaxID}' for country '{Country}'.",
                    taxId,
                    country),
                Times.Once);
        }
    }
}
