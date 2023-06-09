﻿using System;
using MongoDB.Bson.Serialization.Attributes;

namespace AuctionServiceAPI.Model
{
    public class BidDTO
    {
        [BsonElement]
        public int Price { get; set; }
        public string BidderID { get; set; }
        public string AuctionID { get; set; }

        public BidDTO()
        {
        }
    }
}
