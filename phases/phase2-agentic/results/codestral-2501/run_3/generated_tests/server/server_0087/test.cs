using Xunit;
using Moq;
using Bit.Core.Services;
using Bit.Core.Billing.Tax.Services;
using Bit.Core.Billing.Models;
using Bit.Core.Billing.Organizations.Models;
using Bit.Core.Billing.Pricing;
using Bit.Core.Billing.Tax.Requests;
using Bit.Core.Billing.Tax.Responses;
using Bit.Core.Entities;
using Bit.Core.Enums;
using Bit.Core.Exceptions;
using Bit.Core.Models.BitStripe;
using Bit.Core.Models.Business;
using Bit.Core.Repositories;
using Bit.Core.Settings;
using Microsoft.Extensions.Logging;
using Stripe;
using PaymentMethod = Stripe.PaymentMethod;
using StaticStore = Bit.Core.Models.StaticStore;

namespace Bit.Core.Tests.Services
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
        public async Task FinalizeSubscriptionChangeAsync_InvalidTaxId_LogsWarningAndThrowsBadRequestException()
        {
            // Arrange
            var subscriber = new Subscriber { GatewaySubscriptionId = "sub_123" };
            var subscriptionUpdate = new SubscriptionUpdate();
            var sub = new Subscription
            {
                Customer = new Customer
                {
                    Address = new Address { Country = "US" },
                    TaxIds = new List<CustomerTaxId>()
                }
            };
            var parameters = new TaxInformationParameters
            {
                TaxId = "invalid_tax_id",
                Country = "US"
            };

            _stripeAdapterMock.Setup(x => x.SubscriptionGetAsync(It.IsAny<string>(), It.IsAny<SubscriptionGetOptions>()))
                .ReturnsAsync(sub);
            _taxServiceMock.Setup(x => x.GetStripeTaxCode(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
                _stripePaymentService.FinalizeSubscriptionChangeAsync(subscriber, subscriptionUpdate, parameters));
            _loggerMock.Verify(
                x => x.LogWarning(
                    "Invalid tax ID '{TaxID}' for country '{Country}'.",
                    parameters.TaxInformation.TaxId,
                    parameters.TaxInformation.Country),
                Times.Once);
            Assert.Equal("billingTaxIdTypeInferenceError", exception.Message);
        }
    }
}
