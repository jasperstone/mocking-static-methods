using Bit.Core.Billing.Services.Implementations;
using Bit.Core.Billing.Tax.Services;
using Bit.Core.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;
using Stripe;
using Xunit;

namespace Bit.Core.Billing.Tests.Services.Implementations;

public class SubscriberServiceTests
{
    [Fact]
    public async Task TestLogWarningWhenTaxIdTypeInferenceFails()
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
        await Assert.ThrowsAsync<BadRequestException>(() => subscriberService.UpdateTaxInformationAsync(taxInformation));
        loggerMock.Verify(l => l.LogWarning("Could not infer tax ID type in country '{Country}' with tax ID '{TaxID}'.", taxInformation.Country, taxInformation.TaxId), Times.Once);
    }

    [Fact]
    public async Task TestLogWarningWhenTaxIdIsInvalid()
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
            TaxIdType = "US_SSN"
        };

        stripeAdapterMock.Setup(sa => sa.TaxIdCreateAsync(It.IsAny<string>(), It.IsAny<TaxIdCreateOptions>()))
            .Throws(new StripeException("Invalid tax ID", StripeConstants.ErrorCodes.TaxIdInvalid, null));

        // Act and Assert
        await Assert.ThrowsAsync<BadRequestException>(() => subscriberService.UpdateTaxInformationAsync(taxInformation));
        loggerMock.Verify(l => l.LogWarning("Invalid tax ID '{TaxID}' for country '{Country}'.", taxInformation.TaxId, taxInformation.Country), Times.Once);
    }
}
