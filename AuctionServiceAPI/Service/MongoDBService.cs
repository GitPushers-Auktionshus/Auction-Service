using System;
using AuctionServiceAPI.Controllers;
using AuctionServiceAPI.Model;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using static System.Net.Mime.MediaTypeNames;

namespace AuctionServiceAPI.Service
{
    // Inherits from our interface - can be changed to eg. a SQL database
    public class MongoDBService : IAuctionRepository
    {
        private readonly ILogger<AuctionServiceController> _logger;
        private readonly IConfiguration _config;

        // Initializes enviroment variables
        private readonly string _connectionURI;

        private readonly string _usersDatabase;
        private readonly string _inventoryDatabase;
        private readonly string _auctionsDatabase;

        private readonly string _userCollectionName;
        private readonly string _articleCollectionName;
        private readonly string _auctionHouseCollectionName;
        private readonly string _listingsCollectionName;

        private readonly string _imagePath;

        // Initializes MongoDB database collection
        private readonly IMongoCollection<User> _userCollection;
        private readonly IMongoCollection<Auctionhouse> _auctionHouseCollection;
        private readonly IMongoCollection<Article> _articleCollection;
        private readonly IMongoCollection<Auction> _listingsCollection;

        public MongoDBService(ILogger<AuctionServiceController> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;

            try
            {
                // Retrieves enviroment variables from program.cs, from injected EnviromentVariables class 
                //_secret = config["Secret"] ?? "Secret missing";
                //_issuer = config["Issuer"] ?? "Issue'er missing";
                //_connectionURI = config["ConnectionURI"] ?? "ConnectionURI missing";

                //// Retrieves User database and collections
                //_usersDatabase = config["UsersDatabase"] ?? "Userdatabase missing";
                //_userCollectionName = config["UserCollection"] ?? "Usercollection name missing";
                //_auctionHouseCollectionName = config["AuctionHouseCollection"] ?? "Auctionhousecollection name missing";

                //// Retrieves Inventory database and collection
                //_inventoryDatabase = config["InventoryDatabase"] ?? "Invetorydatabase missing";
                //_articleCollectionName = config["ArticleCollection"] ?? "Articlecollection name missing";

            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving enviroment variables");

                throw;
            }

            _connectionURI = "mongodb://admin:1234@localhost:27018/";

            // User database and collections
            _usersDatabase = "Users";
            _userCollectionName = "user";
            _auctionHouseCollectionName = "auctionhouse";

            // Inventory database and collection
            _inventoryDatabase = "Inventory";
            _articleCollectionName = "article";

            // Auction database and collection
            _auctionsDatabase = "Auctions";
            _listingsCollectionName = "listings";

            //_imagePath = "/Users/jacobkaae/Downloads/";

            _logger.LogInformation($"ArticleService secrets: ConnectionURI: {_connectionURI}");
            _logger.LogInformation($"ArticleService Database and Collections: Userdatabase: {_usersDatabase}, Inventorydatabase: {_inventoryDatabase}, Auctiondatabase: {_auctionsDatabase}, UserCollection: {_userCollectionName}, AuctionHouseCollection: {_auctionHouseCollectionName}, ArticleCollection: {_articleCollectionName}, ListingsCollection: {_listingsCollectionName}");

            try
            {
                // Sets MongoDB client
                var mongoClient = new MongoClient(_connectionURI);

                // Sets MongoDB Database
                var userDatabase = mongoClient.GetDatabase(_usersDatabase);
                var inventoryDatabase = mongoClient.GetDatabase(_inventoryDatabase);
                var auctionsDatabase = mongoClient.GetDatabase(_auctionsDatabase);

                // Sets MongoDB Collection
                _userCollection = userDatabase.GetCollection<User>(_userCollectionName);
                _articleCollection = inventoryDatabase.GetCollection<Article>(_articleCollectionName);
                _auctionHouseCollection = userDatabase.GetCollection<Auctionhouse>(_auctionHouseCollectionName);
                _listingsCollection = auctionsDatabase.GetCollection<Auction>(_listingsCollectionName);

            }
            catch (Exception ex)
            {
                _logger.LogError($"Fejl ved oprettelse af forbindelse: {ex.Message}");

                throw;
            }
        }

        public async Task AddBidToAuction(BidDTO bidDTO, string id)
        {
            try
            {
                _logger.LogInformation("AddBidToAuction kaldt");

                // Find the document to update
                var filter = Builders<Auction>.Filter.Eq("AuctionID", id);

                User bidder = new User();
                bidder = _userCollection.Find(x => x.UserID == bidDTO.BidderID).FirstOrDefault<User>();

                // Pushes the image to the article
                var update = Builders<Auction>.Update.Push("Bids", new Bid
                {
                    BidID = ObjectId.GenerateNewId().ToString(),
                    Date = DateTime.UtcNow,
                    Price = bidDTO.Price,
                    Bidder = bidder,
                });

                // Updates the document with the new image
                var result = _listingsCollection.UpdateOne(filter, update);

                Console.WriteLine($"{result.ModifiedCount} document(s) updated.");

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Fejl ved AddImageToArticle: {ex.Message}");

                throw;
            }
        }

    }
}