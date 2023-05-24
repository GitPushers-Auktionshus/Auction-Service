using System;
using System.Text;
using System.Text.Json;
using AuctionServiceAPI.Controllers;
using AuctionServiceAPI.Model;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using RabbitMQ.Client;
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
        private readonly string _hostName;

        private readonly string _imagePath;

        // Initializes MongoDB database collection
        private readonly IMongoCollection<User> _userCollection;
        private readonly IMongoCollection<Auctionhouse> _auctionHouseCollection;
        private readonly IMongoCollection<Article> _articleCollection;
        private readonly IMongoCollection<Auction> _listingsCollection;

        public MongoDBService(ILogger<AuctionServiceController> logger, IConfiguration config, EnvVariables vaultSecrets)
        {
            _logger = logger;
            _config = config;

            try
            {
                // Retrieves enviroment variables from program.cs, from injected EnviromentVariables class 
                _connectionURI = vaultSecrets.dictionary["ConnectionURI"];

                // Retrieves the hostname of RabbitMQ from the docker compose file
                _hostName = config["HostnameRabbit"];

                // User database and collections
                _usersDatabase = config["UsersDatabase"] ?? "UsersDatabase missing";
                _userCollectionName = config["UserCollection"] ?? "UserCollectionName missing";
                _auctionHouseCollectionName = config["AuctionHouseCollection"] ?? "AuctionHouseCollectionName missing";

                // Inventory database and collection
                _inventoryDatabase = config["InventoryDatabase"] ?? "InventoryDatabase missing";
                _articleCollectionName = config["ArticleCollection"] ?? "ArticleCollectionName missing";

                // Auction database and collection
                _auctionsDatabase = config["AuctionsDatabase"] ?? "AuctionDatabase missing";
                _listingsCollectionName = config["AuctionCollection"] ?? "AuctionCollectionName missing";


                _logger.LogInformation($"ArticleService secrets: ConnectionURI: {_connectionURI}, HostnameRabbit: {_hostName}");
                _logger.LogInformation($"ArticleService Database and Collections: Userdatabase: {_usersDatabase}, Inventorydatabase: {_inventoryDatabase}, Auctiondatabase: {_auctionsDatabase}, UserCollection: {_userCollectionName}, AuctionHouseCollection: {_auctionHouseCollectionName}, ArticleCollection: {_articleCollectionName}, ListingsCollection: {_listingsCollectionName}");

            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving enviroment variables");

                throw;
            }

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
                _logger.LogError($"Error trying to connect to database: {ex.Message}");

                throw;
            }
        }

        public async Task<BidDTO> AddBidToAuction(BidDTO bidDTO)
        {
            try
            {
                _logger.LogInformation($"[*] AddBidToAuction called: Adding bid to auction\n Price: {bidDTO.Price}, BidderID: {bidDTO.BidderID}, AuctionID: {bidDTO.AuctionID}");

                Auction auction = new Auction();

                // Finds the auction to which the bid needs to be added to
                auction = await _listingsCollection.Find(x => x.AuctionID == bidDTO.AuctionID).FirstOrDefaultAsync();

                if (auction == null)
                {
                    _logger.LogError("Auction not found");

                    return null;
                }
                // Checks if the auction is active at the current time
                else if (auction.StartDate <= DateTime.Now && auction.EndDate >= DateTime.Now)
                {
                    _logger.LogInformation("Auction active");

                    // Setting the minimum price increase.
                    // Current price increase is 2%
                    double percentageIncrease = 1.02;
                    double nextBid = auction.HighestBid * percentageIncrease;

                    // Checks if the posted bid is higher than the current highest bid * percentageIncrease.
                    // If it isn't it returns null
                    if (bidDTO.Price > nextBid)
                    {
                        // Connects to RabbitMQ
                        var factory = new ConnectionFactory
                        {
                            HostName = _hostName
                        };

                        using var connection = factory.CreateConnection();
                        using var channel = connection.CreateModel();

                        // Declares the topic exchange "AuctionHouse"
                        channel.ExchangeDeclare(exchange: "AuctionHouse", type: ExchangeType.Topic);

                        // Serializes the bidDTO to JSON
                        string message = JsonSerializer.Serialize(bidDTO);

                        _logger.LogInformation($"JsonSerialized message: \n\t{message}");

                        // Converts to byte-array
                        var body = Encoding.UTF8.GetBytes(message);

                        // Send the message to the AuctionHouse topic
                        channel.BasicPublish(exchange: "AuctionHouse",
                                             routingKey: "AuctionBid",
                                             basicProperties: null,
                                             body: body);

                        _logger.LogInformation($"Bid created and posted");

                        return bidDTO;

                    }

                    _logger.LogInformation($"Current bid ({bidDTO.Price}) must be higher than the highest bid ({auction.HighestBid})");

                    return null;

                }
                else
                {
                    _logger.LogError("Auction not active");

                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"EXCEPTION CAUGHT: {ex.Message}");

                throw;
            }
        }

        public async Task<Comment> AddCommentToAuction(CommentDTO commentDTO)
        {
            try
            {
                _logger.LogInformation($"[*] AddCommentToAuction called: Adding comment to auction\n Message: {commentDTO.Message}, UserID: {commentDTO.UserID}");

                Auction auction = new Auction();
                User user = new User();

                // Finds the auction, unto which the comment is going to be added, and the user that added the comment
                auction = await _listingsCollection.Find(x => x.AuctionID == commentDTO.AuctionID).FirstOrDefaultAsync();
                user = await _userCollection.Find(x => x.UserID == commentDTO.UserID).FirstOrDefaultAsync();

                // Creates a filter that finds a specific auction based on the commentDTO's auction ID
                var filter = Builders<Auction>.Filter.Eq("AuctionID", commentDTO.AuctionID);

                // Creates a new comment object based on the commentDTO
                Comment newComment = new Comment
                {
                    CommentID = ObjectId.GenerateNewId().ToString(),
                    UserID = user.UserID,
                    Username = user.Username,
                    DateCreated = DateTime.Now,
                    Message = commentDTO.Message
                };

                // Creates an update definition for the database to use. In this case we need to update the "Comments" property
                var update = Builders<Auction>.Update.Push("Comments", newComment);

                // Updates the listing collection and adds the comment to the auction
                var result = _listingsCollection.UpdateOne(filter, update);

                _logger.LogInformation($"[*] Listing collection update with one new comment\n CommentID: {newComment.CommentID}, UserID: {newComment.UserID}, Username: {newComment.Username}, DateCreated: {newComment.DateCreated}, Message: {newComment.Message}");

                return newComment;
            }
            catch (Exception ex)
            {
                _logger.LogError($"EXCEPTION CAUGHT: {ex.Message}");

                throw;
            }
        }


    }
}