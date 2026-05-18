using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Bit.Core.Services;
using Bit.Core.Billing.Tax.Services;
using System.Threading.Tasks;

namespace Bit.Core.Tests
{
    public class StripePaymentServiceTests
    {
        [Fact]
        public async Task LogWarning_CouldNotInferTaxIdType()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<StripePaymentService>>();
            var taxServiceMock = new Mock<ITaxService>();
            var stripeAdapterMock = new Mock<IStripeAdapter>();
            var stripePaymentService = new StripePaymentService(
                null, loggerMock.Object, stripeAdapterMock.Object, null, null, null, taxServiceMock.Object, null);

            taxServiceMock.Setup(ts => ts.GetStripeTaxCode(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string country, string taxIdNumber) => null);

            // Act and Assert
            await Assert.ThrowsAsync<BadRequestException>(() => stripePaymentService.UpdateTaxInfoAsync(
                new TaxInfo { BillingAddressCountry = "US", TaxIdNumber = "123456789", TaxIdType = null }));

            loggerMock.Verify(l => l.LogWarning(
                "Could not infer tax ID type in country '{Country}' with tax ID '{TaxID}'.",
                "US",
                "123456789"),
                Times.Once);
        }

        [Fact]
        public async Task LogWarning_InvalidTaxId()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<StripePaymentService>>();
            var taxServiceMock = new Mock<ITaxService>();
            var stripeAdapterMock = new Mock<IStripeAdapter>();
            var stripePaymentService = new StripePaymentService(
                null, loggerMock.Object, stripeAdapterMock.Object, null, null, null, taxServiceMock.Object, null);

            stripeAdapterMock.Setup(sa => sa.TaxIdCreateAsync(It.IsAny<string>(), It.IsAny<TaxIdCreateOptions>()))
                .Throws(new StripeException(new StripeError { Code = StripeConstants.ErrorCodes.TaxIdInvalid }));

            // Act and Assert
            await Assert.ThrowsAsync<BadRequestException>(() => stripePaymentService.UpdateTaxInfoAsync(
                new TaxInfo { BillingAddressCountry = "US", TaxIdNumber = "123456789", TaxIdType = "type" }));

            loggerMock.Verify(l => l.LogWarning(
                "Invalid tax ID '{TaxID}' for country '{Country}'.",
                "123456789",
                "US"),
                Times.Once);
        }
    }
}
