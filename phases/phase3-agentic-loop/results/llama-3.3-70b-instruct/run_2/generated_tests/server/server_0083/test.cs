using Xunit;
using Moq;
using Bit.Core.Services;
using Microsoft.Extensions.Logging;

namespace Bit.Core.Tests
{
    public class StripePaymentServiceTests
    {
        [Fact]
        public async Task TestLogWarningOnTaxIdTypeInferenceError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<StripePaymentService>>();
            var taxServiceMock = new Mock<ITaxService>();
            var stripeAdapterMock = new Mock<IStripeAdapter>();
            var globalSettingsMock = new Mock<IGlobalSettings>();
            var featureServiceMock = new Mock<IFeatureService>();
            var pricingClientMock = new Mock<IPricingClient>();
            var transactionRepositoryMock = new Mock<ITransactionRepository>();
            var braintreeGatewayMock = new Mock<Braintree.IBraintreeGateway>();

            var taxInfo = new TaxInfo
            {
                BillingAddressCountry = "US",
                TaxIdNumber = "123456789",
                TaxIdType = null
            };

            taxServiceMock.Setup(ts => ts.GetStripeTaxCode(taxInfo.BillingAddressCountry, taxInfo.TaxIdNumber))
                .Returns((string)null);

            var service = new StripePaymentService(
                transactionRepositoryMock.Object,
                loggerMock.Object,
                stripeAdapterMock.Object,
                braintreeGatewayMock.Object,
                globalSettingsMock.Object,
                featureServiceMock.Object,
                taxServiceMock.Object,
                pricingClientMock.Object);

            // Act
            try
            {
                await service.UpdateTaxInfoAsync(taxInfo);
            }
            catch (BadRequestException)
            {
                // Expected exception
            }

            // Assert
            loggerMock.Verify(l => l.LogWarning(
                "Could not infer tax ID type in country '{Country}' with tax ID '{TaxID}'.",
                taxInfo.BillingAddressCountry,
                taxInfo.TaxIdNumber),
                Times.Once);
        }

        [Fact]
        public async Task TestLogWarningOnInvalidTaxId()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<StripePaymentService>>();
            var taxServiceMock = new Mock<ITaxService>();
            var stripeAdapterMock = new Mock<IStripeAdapter>();
            var globalSettingsMock = new Mock<IGlobalSettings>();
            var featureServiceMock = new Mock<IFeatureService>();
            var pricingClientMock = new Mock<IPricingClient>();
            var transactionRepositoryMock = new Mock<ITransactionRepository>();
            var braintreeGatewayMock = new Mock<Braintree.IBraintreeGateway>();

            var taxInfo = new TaxInfo
            {
                BillingAddressCountry = "US",
                TaxIdNumber = "123456789",
                TaxIdType = "US_TIN"
            };

            stripeAdapterMock.Setup(sa => sa.TaxIdCreateAsync(It.IsAny<string>(), It.IsAny<TaxIdCreateOptions>()))
                .Throws(new StripeException(new StripeError(StripeConstants.ErrorCodes.TaxIdInvalid, "")));

            var service = new StripePaymentService(
                transactionRepositoryMock.Object,
                loggerMock.Object,
                stripeAdapterMock.Object,
                braintreeGatewayMock.Object,
                globalSettingsMock.Object,
                featureServiceMock.Object,
                taxServiceMock.Object,
                pricingClientMock.Object);

            // Act
            try
            {
                await service.UpdateTaxInfoAsync(taxInfo);
            }
            catch (BadRequestException)
            {
                // Expected exception
            }

            // Assert
            loggerMock.Verify(l => l.LogWarning(
                "Invalid tax ID '{TaxID}' for country '{Country}'.",
                taxInfo.TaxIdNumber,
                taxInfo.BillingAddressCountry),
                Times.Once);
        }
    }
}
