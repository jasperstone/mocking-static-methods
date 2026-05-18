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
    public async Task LogWarning_Called_When_TaxIdType_Is_Null()
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
            .Returns((string)null);

        // Act and Assert
        await Assert.ThrowsAsync<BadRequestException>(() => subscriberService.UpdateTaxInformationAsync(null, taxInformation));
        loggerMock.Verify(l => l.LogWarning(
            "Could not infer tax ID type in country '{Country}' with tax ID '{TaxID}'.",
            taxInformation.Country,
            taxInformation.TaxId),
            Times.Once);
    }
}
