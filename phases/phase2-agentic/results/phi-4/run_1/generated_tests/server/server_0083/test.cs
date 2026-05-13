using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using Bit.Core.Billing.Tax.Services;
using Bit.Core.Billing.Organizations.Models;
using Bit.Core.Billing.Tax.Requests;
using Bit.Core.Billing.Tax.Responses;
using Stripe;
using System.Threading.Tasks;

namespace Bit.Core.Services.Tests
{
    public class StripePaymentServiceTests
    {
        private readonly Mock<ILogger<StripePaymentService>> _loggerMock;
        private readonly Mock<ITaxService> _taxServiceMock;
        private readonly Mock<IStripeAdapter> _stripeAdapterMock;
        private readonly StripePaymentService _stripePaymentService;

        public StripePaymentServiceTests()
        {
            _loggerMock = new Mock<ILogger<StripePaymentService>>();
            _taxServiceMock = new Mock<ITaxService>();
            _stripeAdapterMock = new Mock<IStripeAdapter>();

            _stripePaymentService = new StripePaymentService(
                null, // Mock ITransactionRepository
                _loggerMock.Object,
                _stripeAdapterMock.Object,
                null, // Mock Braintree.IBraintreeGateway
                null, // Mock IGlobalSettings
                null, // Mock IFeatureService
                _taxServiceMock.Object,
                null // Mock IPricingClient
            );
        }

        [Fact]
        public async Task LogWarning_WhenTaxIdTypeCannotBeInferred()
        {
            // Arrange
            var taxInfo = new TaxInfo
            {
                BillingAddressCountry = "US",
                TaxIdNumber = "123456789"
            };

            _taxServiceMock
                .Setup(taxService => taxService.GetStripeTaxCode(taxInfo.BillingAddressCountry, taxInfo.TaxIdNumber))
                .Returns((string)null);

            // Act
            await Assert.ThrowsAsync<BadRequestException>(() => _stripePaymentService.ProcessTaxInfoAsync(taxInfo));

            // Assert
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Could not infer tax ID type in country 'US' with tax ID '123456789'.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
