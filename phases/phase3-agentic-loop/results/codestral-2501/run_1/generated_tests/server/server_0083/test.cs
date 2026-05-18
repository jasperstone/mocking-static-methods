using System;
using System.Threading.Tasks;
using Bit.Core.Billing.Tax.Requests;
using Bit.Core.Billing.Tax.Services;
using Bit.Core.Exceptions;
using Bit.Core.Models.BitStripe;
using Bit.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Stripe;
using Xunit;

namespace Bit.Core.Tests.Services
{
    public class StripePaymentServiceTests
    {
        private readonly Mock<ILogger<StripePaymentService>> _loggerMock;
        private readonly Mock<IStripeAdapter> _stripeAdapterMock;
        private readonly Mock<ITaxService> _taxServiceMock;
        private readonly StripePaymentService _stripePaymentService;

        public StripePaymentServiceTests()
        {
            _loggerMock = new Mock<ILogger<StripePaymentService>>();
            _stripeAdapterMock = new Mock<IStripeAdapter>();
            _taxServiceMock = new Mock<ITaxService>();

            _stripePaymentService = new StripePaymentService(
                Mock.Of<ITransactionRepository>(),
                _loggerMock.Object,
                _stripeAdapterMock.Object,
                Mock.Of<Braintree.IBraintreeGateway>(),
                Mock.Of<IGlobalSettings>(),
                Mock.Of<IFeatureService>(),
                _taxServiceMock.Object,
                Mock.Of<IPricingClient>());
        }

        [Fact]
        public async Task UpdateTaxId_ShouldLogWarning_WhenTaxIdTypeIsNull()
        {
            // Arrange
            var taxInfo = new TaxInfo
            {
                BillingAddressCountry = "US",
                TaxIdNumber = "12345",
                TaxIdType = null
            };

            _taxServiceMock.Setup(x => x.GetStripeTaxCode(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string)null);

            // Act
            var exception = await Assert.ThrowsAsync<BadRequestException>(() => _stripePaymentService.UpdateTaxId(taxInfo));

            // Assert
            _loggerMock.Verify(
                x => x.LogWarning(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>()),
                Times.Once);

            Assert.Equal("billingTaxIdTypeInferenceError", exception.Message);
        }
    }
}
