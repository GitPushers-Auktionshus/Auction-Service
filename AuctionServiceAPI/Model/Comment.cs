using System;
using MongoDB.Bson.Serialization.Attributes;

namespace AuctionServiceAPI.Model
{
	public class Comment
	{
        [BsonId]
        public User User { get; set; }
        [BsonElement]
        public DateTime? DateCreated { get; set; } = DateTime.UtcNow;
        public string Message { get; set; }
        public Auction Auction { get; set; }

        public Comment()
		{
		}
	}
}

