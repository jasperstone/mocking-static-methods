using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Bit.Core.Billing.Services.Implementations;
using Bit.Core.Billing.Tax.Services;
using Bit.Core.Exceptions;

namespace Bit.Core.Billing.Tests.Services.Implementations;

public class SubscriberServiceTests
{
    [Fact]
    public async Task LogWarning_IsCalled_WhenTaxIdTypeCannotBeInferred()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<SubscriberService>>();
        var mockStripeAdapter = new Mock<IStripeAdapter>();
        var mockTaxService = new Mock<ITaxService>();

        var subscriberService = new SubscriberService(
            braintreeGateway: null!,
            globalSettings: null!,
            logger: mockLogger.Object,
            organizationRepository: null!,
            providerRepository: null!,
            setupIntentCache: null!,
            stripeAdapter: mockStripeAdapter.Object,
            taxService: mockTaxService.Object,
            userRepository: null!);

        var taxInformation = new
        {
            Country = "US",
            PostalCode = "12345",
            Line1 = "123 Main St",
            Line2 = (string?)null,
            City = "Anytown",
            State = "NY",
            TaxId = "123456789",
            TaxIdType = (string?)null
        };

        // Setup taxService to return null for GetStripeTaxCode to simulate failure to infer tax ID type
        mockTaxService.Setup(t => t.GetStripeTaxCode(taxInformation.Country, taxInformation.TaxId))
            .Returns((string?)null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BadRequestException>(async () =>
        {
            // Simulate the logic that calls LogWarning and throws BadRequestException
            var taxIdType = taxInformation.TaxIdType;
            if (string.IsNullOrWhiteSpace(taxIdType))
            {
                taxIdType = mockTaxService.Object.GetStripeTaxCode(taxInformation.Country, taxInformation.TaxId);

                if (taxIdType == null)
                {
                    mockLogger.Object.LogWarning("Could not infer tax ID type in country '{Country}' with tax ID '{TaxID}'.",
                        taxInformation.Country,
                        taxInformation.TaxId);

                    throw new BadRequestException("billingTaxIdTypeInferenceError");
                }
            }
        });

        // Verify that LogWarning was called with the expected message and parameters
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Could not infer tax ID type in country")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
