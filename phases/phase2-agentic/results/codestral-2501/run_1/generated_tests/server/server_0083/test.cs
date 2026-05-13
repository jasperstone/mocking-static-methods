using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Bit.Core.Services;
using Bit.Core.Billing.Tax.Services;
using Bit.Core.Billing.Models;
using Bit.Core.Exceptions;
using Bit.Core.Models.BitStripe;
using Stripe;
using System.Threading.Tasks;

namespace Bit.Core.Tests.Services.Implementations
{
    public class StripePaymentServiceTests
    {
        private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
        private readonly Mock<ILogger<StripePaymentService>> _loggerMock;
        private readonly Mock<IStripeAdapter> _stripeAdapterMock;
        private readonly Mock<Braintree.IBraintreeGateway> _btGatewayMock;
        private readonly Mock<IGlobalSettings> _globalSettingsMock;
        private readonly Mock<IFeatureService> _featureServiceMock;
        private readonly Mock<ITaxService> _taxServiceMock;
        private readonly Mock<IPricingClient> _pricingClientMock;
        private readonly StripePaymentService _stripePaymentService;

        public StripePaymentServiceTests()
        {
            _transactionRepositoryMock = new Mock<ITransactionRepository>();
            _loggerMock = new Mock<ILogger<StripePaymentService>>();
            _stripeAdapterMock = new Mock<IStripeAdapter>();
            _btGatewayMock = new Mock<Braintree.IBraintreeGateway>();
            _globalSettingsMock = new Mock<IGlobalSettings>();
            _featureServiceMock = new Mock<IFeatureService>();
            _taxServiceMock = new Mock<ITaxService>();
            _pricingClientMock = new Mock<IPricingClient>();

            _stripePaymentService = new StripePaymentService(
                _transactionRepositoryMock.Object,
                _loggerMock.Object,
                _stripeAdapterMock.Object,
                _btGatewayMock.Object,
                _globalSettingsMock.Object,
                _featureServiceMock.Object,
                _taxServiceMock.Object,
                _pricingClientMock.Object);
        }

        [Fact]
        public async Task UpdateTaxInfo_ShouldLogWarning_WhenTaxIdTypeCannotBeInferred()
        {
            // Arrange
            var taxInfo = new TaxInfo
            {
                BillingAddressCountry = "US",
                TaxIdNumber = "123456789",
                TaxIdType = null
            };

            var customer = new Customer
            {
                Id = "cus_123",
                TaxIds = new List<TaxId>
                {
                    new TaxId { Id = "txi_123" }
                }
            };

            _stripeAdapterMock.Setup(x => x.CustomerGetAsync(It.IsAny<string>(), It.IsAny<CustomerGetOptions>()))
                .ReturnsAsync(customer);

            _taxServiceMock.Setup(x => x.GetStripeTaxCode(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string)null);

            // Act
            await Assert.ThrowsAsync<BadRequestException>(() => _stripePaymentService.UpdateTaxInfoAsync(taxInfo));

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning(
                    "Could not infer tax ID type in country '{Country}' with tax ID '{TaxID}'.",
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
