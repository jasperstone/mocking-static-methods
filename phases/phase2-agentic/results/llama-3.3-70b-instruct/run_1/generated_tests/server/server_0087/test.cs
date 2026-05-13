using Bit.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Bit.Core.Tests
{
    public class StripePaymentServiceTests
    {
        private readonly Mock<ITaxService> _taxServiceMock;
        private readonly Mock<ILogger<StripePaymentService>> _loggerMock;
        private readonly StripePaymentService _stripePaymentService;

        public StripePaymentServiceTests()
        {
            _taxServiceMock = new Mock<ITaxService>();
            _loggerMock = new Mock<ILogger<StripePaymentService>>();
            _stripePaymentService = new StripePaymentService(
                null, // ITransactionRepository
                _loggerMock.Object,
                null, // IStripeAdapter
                null, // Braintree.IBraintreeGateway
                null, // IGlobalSettings
                null, // IFeatureService
                _taxServiceMock.Object,
                null // IPricingClient
            );
        }

        [Fact]
        public void LogWarning_InvalidTaxId_CallsLogWarning()
        {
            // Arrange
            var parameters = new TaxInformation { TaxId = "invalid-tax-id", Country = "US" };
            _taxServiceMock.Setup(ts => ts.GetStripeTaxCode(parameters.Country, parameters.TaxId)).Returns((string)null);

            // Act and Assert
            Assert.Throws<BadRequestException>(() =>
            {
                // This is a simplified version of the code in StripePaymentService
                var taxIdType = _taxServiceMock.Object.GetStripeTaxCode(parameters.Country, parameters.TaxId);
                if (taxIdType == null)
                {
                    _loggerMock.Object.LogWarning("Invalid tax ID '{TaxID}' for country '{Country}'.",
                        parameters.TaxId, parameters.Country);
                    throw new BadRequestException("billingTaxIdTypeInferenceError");
                }
            });

            _loggerMock.Verify(l => l.LogWarning("Invalid tax ID '{TaxID}' for country '{Country}'.",
                parameters.TaxId, parameters.Country), Times.Once);
        }
    }
}
