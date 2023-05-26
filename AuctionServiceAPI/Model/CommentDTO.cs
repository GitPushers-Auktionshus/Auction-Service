using System;
using MongoDB.Bson.Serialization.Attributes;

namespace AuctionServiceAPI.Model
{
	public class CommentDTO
	{
        public string UserID { get; set; }
        public string Message { get; set; }
		public string AuctionID { get; set; }

        public CommentDTO()
		{
		}
	}
}

