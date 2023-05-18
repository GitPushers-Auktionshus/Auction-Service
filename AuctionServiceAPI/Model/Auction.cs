﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using static System.Net.Mime.MediaTypeNames;

namespace AuctionServiceAPI.Model
{
    public class Auction
    {
        [BsonId]
        public string AuctionID { get; set; }
        public int HighestBid { get; set; }
        public List<Bid> Bids { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Views { get; set; }
        public Article Article { get; set; }

        public Auction()
        {
        }
    }
}

