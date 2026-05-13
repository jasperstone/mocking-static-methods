using Bit.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Stripe;
using Xunit;

namespace Bit.Core.Tests.Services;

public class StripePaymentServiceTests
{
    [Fact]
    public async Task LogWarning_WhenTaxIdTypeInferenceFails()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<StripePaymentService>>();
        var taxServiceMock = new Mock<ITaxService>();
        var stripeAdapterMock = new Mock<IStripeAdapter>();
        var globalSettingsMock = new Mock<IGlobalSettings>();
        var featureServiceMock = new Mock<IFeatureService>();
        var pricingClientMock = new Mock<IPricingClient>();

        taxServiceMock.Setup(ts => ts.GetStripeTaxCode(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string? taxIdType) => null);

        var taxInfo = new TaxInfo
        {
            BillingAddressCountry = "US",
            TaxIdNumber = "123456789",
            TaxIdType = null
        };

        var service = new StripePaymentService(
            Mock.Of<ITransactionRepository>(),
            loggerMock.Object,
            stripeAdapterMock.Object,
            Mock.Of<Braintree.IBraintreeGateway>(),
            globalSettingsMock.Object,
            featureServiceMock.Object,
            taxServiceMock.Object,
            pricingClientMock.Object);

        // Act and Assert
        await Assert.ThrowsAsync<BadRequestException>(() => service.UpdateTaxInfo(taxInfo));
        loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task LogWarning_WhenTaxIdIsInvalid()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<StripePaymentService>>();
        var taxServiceMock = new Mock<ITaxService>();
        var stripeAdapterMock = new Mock<IStripeAdapter>();
        var globalSettingsMock = new Mock<IGlobalSettings>();
        var featureServiceMock = new Mock<IFeatureService>();
        var pricingClientMock = new Mock<IPricingClient>();

        taxServiceMock.Setup(ts => ts.GetStripeTaxCode(It.IsAny<string>(), It.IsAny<string>()))
            .Returns("some-tax-id-type");

        stripeAdapterMock.Setup(sa => sa.TaxIdCreateAsync(It.IsAny<string>(), It.IsAny<TaxIdCreateOptions>()))
            .Throws(new StripeException(new StripeError { Code = StripeConstants.ErrorCodes.TaxIdInvalid }));

        var taxInfo = new TaxInfo
        {
            BillingAddressCountry = "US",
            TaxIdNumber = "123456789",
            TaxIdType = "some-tax-id-type"
        };

        var service = new StripePaymentService(
            Mock.Of<ITransactionRepository>(),
            loggerMock.Object,
            stripeAdapterMock.Object,
            Mock.Of<Braintree.IBraintreeGateway>(),
            globalSettingsMock.Object,
            featureServiceMock.Object,
            taxServiceMock.Object,
            pricingClientMock.Object);

        // Act and Assert
        await Assert.ThrowsAsync<BadRequestException>(() => service.UpdateTaxInfo(taxInfo));
        loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }
}
