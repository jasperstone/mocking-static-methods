using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Bit.Core.Billing.Services.Implementations;
using Bit.Core.Billing.Tax.Services;
using Stripe;

namespace Bit.Core.Billing.Tests.Services.Implementations;

public class SubscriberServiceTests
{
    [Fact]
    public async Task CreateStripeCustomer_TaxIdTypeInferenceError_LogsWarningAndThrowsBadRequestException()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SubscriberService>>();
        var taxServiceMock = new Mock<ITaxService>();
        var stripeAdapterMock = new Mock<IStripeAdapter>();
        var subscriberService = new SubscriberService(
            null, // braintreeGateway
            null, // globalSettings
            loggerMock.Object,
            null, // organizationRepository
            null, // providerRepository
            null, // setupIntentCache
            stripeAdapterMock.Object,
            taxServiceMock.Object,
            null // userRepository
        );

        var taxInformation = new TaxInformation
        {
            Country = "US",
            TaxId = "123456789",
            TaxIdType = null
        };

        taxServiceMock.Setup(ts => ts.GetStripeTaxCode(taxInformation.Country, taxInformation.TaxId))
            .Returns((string?)null);

        // Act and Assert
        await Assert.ThrowsAsync<BadRequestException>(() => subscriberService.CreateStripeCustomer(null));
        loggerMock.Verify(l => l.LogWarning("Could not infer tax ID type in country '{Country}' with tax ID '{TaxID}'.", taxInformation.Country, taxInformation.TaxId), Times.Once);
    }

    [Fact]
    public async Task CreateStripeCustomer_InvalidTaxId_LogsWarningAndThrowsBadRequestException()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SubscriberService>>();
        var taxServiceMock = new Mock<ITaxService>();
        var stripeAdapterMock = new Mock<IStripeAdapter>();
        var subscriberService = new SubscriberService(
            null, // braintreeGateway
            null, // globalSettings
            loggerMock.Object,
            null, // organizationRepository
            null, // providerRepository
            null, // setupIntentCache
            stripeAdapterMock.Object,
            taxServiceMock.Object,
            null // userRepository
        );

        var taxInformation = new TaxInformation
        {
            Country = "US",
            TaxId = "123456789",
            TaxIdType = "individual"
        };

        stripeAdapterMock.Setup(sa => sa.TaxIdCreateAsync(It.IsAny<string>(), It.IsAny<TaxIdCreateOptions>()))
            .Throws(new StripeException("Invalid tax ID", "tax_id_invalid", null));

        // Act and Assert
        await Assert.ThrowsAsync<BadRequestException>(() => subscriberService.CreateStripeCustomer(null));
        loggerMock.Verify(l => l.LogWarning("Invalid tax ID '{TaxID}' for country '{Country}'.", taxInformation.TaxId, taxInformation.Country), Times.Once);
    }
}
