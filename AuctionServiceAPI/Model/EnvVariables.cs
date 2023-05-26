using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using static System.Net.Mime.MediaTypeNames;

namespace AuctionServiceAPI.Model
{
    public class EnvVariables
    {
        public Dictionary<string, string> dictionary { get; set; }

        public EnvVariables()
        {
        }
    }
}

