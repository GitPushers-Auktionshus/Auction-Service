using System.Threading;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Text;
using AuctionServiceAPI.Model;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography;
using MongoDB.Bson.Serialization.Attributes;
using System.IO.Pipelines;
using System.IO;
using AuctionServiceAPI.Service;
using AuctionServiceAPI.Model;

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

    //POST - Adds a new article
    [HttpPut("addBid/{id}")]
    public async Task<IActionResult> AddBid(BidDTO bidDTO, string id)
    {

        await _service.AddBidToAuction(bidDTO, id);

        return Ok("Test");
    }
}