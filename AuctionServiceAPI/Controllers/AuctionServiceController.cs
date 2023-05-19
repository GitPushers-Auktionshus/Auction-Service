using Microsoft.AspNetCore.Mvc;
using AuctionServiceAPI.Model;
using AuctionServiceAPI.Service;

namespace AuctionServiceAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AuctionServiceController : ControllerBase
{
    private readonly ILogger<AuctionServiceController> _logger;

    private readonly IConfiguration _config;

    private readonly IAuctionRepository _service;

    public AuctionServiceController(ILogger<AuctionServiceController> logger, IConfiguration config, IAuctionRepository service)
    {
        _logger = logger;
        _config = config;
        _service = service;
    }

    //POST - Adds a new bid
    [HttpPost("addBid")]
    public async Task<Bid> AddBid(BidDTO bidDTO)
    {
        return await _service.AddBidToAuction(bidDTO);
    }
}