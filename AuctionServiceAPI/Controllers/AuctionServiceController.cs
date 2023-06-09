using Microsoft.AspNetCore.Mvc;
using AuctionServiceAPI.Model;
using AuctionServiceAPI.Service;
using Microsoft.AspNetCore.Authorization;

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

    //PUT - Adds a new bid
    [Authorize]
    [HttpPut("addBid")]
    public async Task<IActionResult> AddBid(BidDTO bidDTO)
    {
        _logger.LogInformation($"[PUT] addBid endpoint reached");

        try
        {
            BidDTO addedBid = await _service.AddBidToAuction(bidDTO);
            return new ObjectResult(addedBid)
            {
                StatusCode = 201
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while adding a new bid.");
            return BadRequest();
        }
    }

    //PUT - Adds a new comment
    [Authorize]
    [HttpPut("addComment")]
    public async Task<Comment> AddComment(CommentDTO commentDTO)
    {
        _logger.LogInformation($"[PUT] addComment endpoint reached");

        return await _service.AddCommentToAuction(commentDTO);
    }
}