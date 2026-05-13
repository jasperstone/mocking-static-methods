using Bit.Core.Billing.Services.Implementations;
using Bit.Core.Billing.Tax.Models;
using Bit.Core.Billing.Tax.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Stripe;
using Xunit;

namespace Bit.Core.Billing.Tests;

public class SubscriberServiceTests
{
    [Fact]
    public async Task CreateStripeCustomer_TaxIdTypeInferenceError_LogsWarning()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SubscriberService>>();
        var taxServiceMock = new Mock<ITaxService>();
        var stripeAdapterMock = new Mock<IStripeAdapter>();
        var subscriberService = new SubscriberService(
            null,
            null,
            loggerMock.Object,
            null,
            null,
            null,
            stripeAdapterMock.Object,
            taxServiceMock.Object,
            null);

        var taxInformation = new TaxInformation
        {
            Country = "US",
            TaxId = "123456789",
            TaxIdType = null
        };

        taxServiceMock.Setup(ts => ts.GetStripeTaxCode(taxInformation.Country, taxInformation.TaxId))
            .Returns((string)null);

        // Act and Assert
        await Assert.ThrowsAsync<BadRequestException>(() => subscriberService.CreateStripeCustomer(null));
        loggerMock.Verify(l => l.LogWarning("Could not infer tax ID type in country '{Country}' with tax ID '{TaxID}'.", taxInformation.Country, taxInformation.TaxId), Times.Once);
    }

    [Fact]
    public async Task CreateStripeCustomer_InvalidTaxId_LogsWarning()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SubscriberService>>();
        var taxServiceMock = new Mock<ITaxService>();
        var stripeAdapterMock = new Mock<IStripeAdapter>();
        var subscriberService = new SubscriberService(
            null,
            null,
            loggerMock.Object,
            null,
            null,
            null,
            stripeAdapterMock.Object,
            taxServiceMock.Object,
            null);

        var taxInformation = new TaxInformation
        {
            Country = "US",
            TaxId = "123456789",
            TaxIdType = "US_EIN"
        };

        stripeAdapterMock.Setup(sa => sa.TaxIdCreateAsync(It.IsAny<string>(), It.IsAny<TaxIdCreateOptions>()))
            .Throws(new StripeException("Invalid tax ID", "invalid_tax_id", null));

        // Act and Assert
        await Assert.ThrowsAsync<BadRequestException>(() => subscriberService.CreateStripeCustomer(null));
        loggerMock.Verify(l => l.LogWarning("Invalid tax ID '{TaxID}' for country '{Country}'.", taxInformation.TaxId, taxInformation.Country), Times.Once);
    }
}
