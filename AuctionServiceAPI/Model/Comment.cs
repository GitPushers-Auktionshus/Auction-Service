using System;
using MongoDB.Bson.Serialization.Attributes;

namespace AuctionServiceAPI.Model
{
	public class Comment
	{
        [BsonId]
        public string UserID { get; set; }
        [BsonElement]
        public DateTime? DateCreated { get; set; } = DateTime.UtcNow;
        public string Message { get; set; }
        public User Seller { get; set; }
        public User Bidder { get; set; }
        public Auction AuctionID { get; set; }

        public Comment()
		{
		}
	}
}

