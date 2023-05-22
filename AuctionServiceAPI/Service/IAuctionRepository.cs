using System;
using AuctionServiceAPI.Model;
using Microsoft.AspNetCore.Mvc;

namespace AuctionServiceAPI.Service
{
    public interface IAuctionRepository
    {
        public Task<Bid> AddBidToAuction(BidDTO bidDTO);

        public Task<Comment> AddCommentToAuction(CommentDTO commentDTO);

    }

}
