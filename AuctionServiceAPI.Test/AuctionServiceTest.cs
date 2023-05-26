using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AuctionServiceAPI.Controllers;
using AuctionServiceAPI.Model;
using AuctionServiceAPI.Service;
using Moq;
using Microsoft.AspNetCore.Mvc;

namespace AuctionServiceAPI.Test;

public class AuctionServiceTest
{

    private ILogger<AuctionServiceController> _logger = null!;
    private IConfiguration _configuration = null!;


    [SetUp]
    public void Setup()
    {
        _logger = new Mock<ILogger<AuctionServiceController>>().Object;

        var myConfiguration = new Dictionary<string, string?>
        {
            {"AuctionServiceBrokerHost", "http://testhost.local"}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(myConfiguration)
            .Build();
    }

    // Tests that the  method returns a CreatedAtActionResult object, when a bid is placed correctly
    [Test]
    public async Task TestAddBidEndpoint_valid_dto()
    {
        // Arrange
        var bidDTO = CreateBidDTO(100);


        var stubRepo = new Mock<IAuctionRepository>();

        stubRepo.Setup(svc => svc.AddBidToAuction(bidDTO))
            .Returns(Task.FromResult<BidDTO>(bidDTO));

        var controller = new AuctionServiceController(_logger, _configuration, stubRepo.Object);

        // Act        
        var result = await controller.AddBid(bidDTO);

        // Assert
        Assert.That(result, Is.TypeOf<ObjectResult>());
        Assert.That((result as ObjectResult)?.Value, Is.TypeOf<BidDTO>());

    }

    // Tests that the method returns a BadRequestResult object, when the AddBid method fails / throws an exception
    [Test]
    public async Task TestAddBidEndpoint_failure_posting()
    {
        // Arrange
        var bidDTO = CreateBidDTO(100);

        var stubRepo = new Mock<IAuctionRepository>();

        stubRepo.Setup(svc => svc.AddBidToAuction(bidDTO))
            .ThrowsAsync(new Exception());

        var controller = new AuctionServiceController(_logger, _configuration, stubRepo.Object);

        // Act        
        var result = await controller.AddBid(bidDTO);

        // Assert
        Assert.That(result, Is.TypeOf<BadRequestResult>());

    }

    /// <summary>
    /// Helper method for creating BidDTO instance.
    /// </summary>
    /// <param name="bidDTO"></param>
    /// <returns></returns>
    private BidDTO CreateBidDTO(int price)
    {
        var bidDTO = new BidDTO()
        {
            Price = price,
            BidderID = "Test BidderID",
            AuctionID = "Test AuctionID"
        };

        return bidDTO;
    }
}