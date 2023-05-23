using System;
using AuctionServiceAPI.Model;
using Microsoft.AspNetCore.Mvc;

namespace AuctionServiceAPI.Service
{
    public interface IAuctionRepository
    {
        /// <summary>
        /// Adds a bid to an auction in the database
        /// </summary>
        /// <param name="bidDTO"></param>
        /// <returns>The bid added to the database</returns>
        public Task<BidDTO> AddBidToAuction(BidDTO bidDTO);

        /// <summary>
        /// Adds a comment to an auction in the database
        /// </summary>
        /// <param name="commentDTO"></param>
        /// <returns>The comment added to the database</returns>
        public Task<Comment> AddCommentToAuction(CommentDTO commentDTO);

    }

}
