using Bit.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Stripe;
using Xunit;

namespace Bit.Core.Tests.Services;

public class StripePaymentServiceTests
{
    [Fact]
    public async Task TestLogWarningOnTaxIdTypeInferenceError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<StripePaymentService>>();
        var stripeAdapterMock = new Mock<IStripeAdapter>();
        var taxServiceMock = new Mock<ITaxService>();
        var globalSettingsMock = new Mock<IGlobalSettings>();
        var featureServiceMock = new Mock<IFeatureService>();
        var pricingClientMock = new Mock<IPricingClient>();

        var taxInfo = new TaxInfo
        {
            BillingAddressCountry = "US",
            TaxIdNumber = "123456789",
            TaxIdType = null
        };

        taxServiceMock.Setup(ts => ts.GetStripeTaxCode(taxInfo.BillingAddressCountry, taxInfo.TaxIdNumber))
            .Returns((string)null);

        var service = new StripePaymentService(
            null,
            loggerMock.Object,
            stripeAdapterMock.Object,
            null,
            globalSettingsMock.Object,
            featureServiceMock.Object,
            taxServiceMock.Object,
            pricingClientMock.Object);

        // Act and Assert
        await Assert.ThrowsAsync<BadRequestException>(() => service.UpdateTaxInfoAsync(taxInfo));
        loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), taxInfo.BillingAddressCountry, taxInfo.TaxIdNumber), Times.Once);
    }

    [Fact]
    public async Task TestLogWarningOnInvalidTaxId()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<StripePaymentService>>();
        var stripeAdapterMock = new Mock<IStripeAdapter>();
        var taxServiceMock = new Mock<ITaxService>();
        var globalSettingsMock = new Mock<IGlobalSettings>();
        var featureServiceMock = new Mock<IFeatureService>();
        var pricingClientMock = new Mock<IPricingClient>();

        var taxInfo = new TaxInfo
        {
            BillingAddressCountry = "US",
            TaxIdNumber = "123456789",
            TaxIdType = "type"
        };

        stripeAdapterMock.Setup(sa => sa.TaxIdCreateAsync(It.IsAny<string>(), It.IsAny<TaxIdCreateOptions>()))
            .Throws(new StripeException(new StripeError { Code = StripeConstants.ErrorCodes.TaxIdInvalid }));

        var service = new StripePaymentService(
            null,
            loggerMock.Object,
            stripeAdapterMock.Object,
            null,
            globalSettingsMock.Object,
            featureServiceMock.Object,
            taxServiceMock.Object,
            pricingClientMock.Object);

        // Act and Assert
        await Assert.ThrowsAsync<BadRequestException>(() => service.UpdateTaxInfoAsync(taxInfo));
        loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), taxInfo.TaxIdNumber, taxInfo.BillingAddressCountry), Times.Once);
    }
}
