using System;
using AuctionServiceAPI.Model;
using Microsoft.AspNetCore.Mvc;

namespace AuctionServiceAPI.Service
{
    public interface IAuctionRepository
    {

        public Task AddBidToAuction(BidDTO bidDTO, string id);

    }

}
