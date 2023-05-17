using System;
using MongoDB.Bson.Serialization.Attributes;

namespace AuctionServiceAPI.Model
{
    public class BidDTO
    {
        [BsonElement]
        public DateTime? Date { get; set; } = DateTime.UtcNow;
        public int Price { get; set; }
        public string BidderID { get; set; }

        public BidDTO()
        {
        }
    }
}
