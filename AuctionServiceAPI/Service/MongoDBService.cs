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

        public MongoDBService(ILogger<AuctionServiceController> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;

            try
            {
                // Retrieves enviroment variables from program.cs, from injected EnviromentVariables class 
                //_secret = config["Secret"] ?? "Secret missing";
                //_issuer = config["Issuer"] ?? "Issue'er missing";
                _connectionURI = config["ConnectionURI"] ?? "ConnectionURI missing";
                _hostName = config["HostnameRabbit"];


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


            // User database and collections
            _usersDatabase = "Users";
            _userCollectionName = "user";
            _auctionHouseCollectionName = "auctionhouse";

            // Inventory database and collection
            _inventoryDatabase = "Inventory";
            _articleCollectionName = "article";

            // Auction database and collection
            _auctionsDatabase = "Auctions";
            _listingsCollectionName = "listing";


            //_imagePath = "/Users/jacobkaae/Downloads/";

            _logger.LogInformation($"ArticleService secrets: ConnectionURI: {_connectionURI}, HostnameRabbit: {_hostName}");
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
                _logger.LogError($"Error trying to connect to database: {ex.Message}");

                throw;
            }
        }

        public async Task<Bid> AddBidToAuction(BidDTO bidDTO)
        {
            try
            {
                _logger.LogInformation($"[*] AddBidToAuction called: Adding bid to auction\n Price: {bidDTO.Price}, BidderID: {bidDTO.BidderID}, AuctionID: {bidDTO.AuctionID}");

                Auction auction = new Auction();

                auction = await _listingsCollection.Find(x => x.AuctionID == bidDTO.AuctionID).FirstOrDefaultAsync();

                if (auction == null)
                {
                    _logger.LogError("Auction not found");

                    return null;
                }
                else if (auction.StartDate < DateTime.Now && auction.EndDate > DateTime.Now)
                {
                    _logger.LogInformation("Auction active");

                    //Opretter forbindelse til RabbitMQ
                    var factory = new ConnectionFactory
                    {
                        HostName = _hostName
                    };

                    using var connection = factory.CreateConnection();
                    using var channel = connection.CreateModel();

                    channel.ExchangeDeclare(exchange: "AuctionHouse", type: ExchangeType.Topic);

                    // Serialiseres til JSON
                    string message = JsonSerializer.Serialize(bidDTO);

                    _logger.LogInformation($"JsonSerialized message: \n\t{message}");

                    // Konverteres til byte-array
                    var body = Encoding.UTF8.GetBytes(message);

                    // Sendes til Service-køen
                    channel.BasicPublish(exchange: "AuctionHouse",
                                         routingKey: "AuctionBid",
                                         basicProperties: null,
                                         body: body);

                    _logger.LogInformation($"Bid created and posted");

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

                auction = await _listingsCollection.Find(x => x.AuctionID == commentDTO.AuctionID).FirstOrDefaultAsync();
                user = await _userCollection.Find(x => x.UserID == commentDTO.UserID).FirstOrDefaultAsync();

                var filter = Builders<Auction>.Filter.Eq("AuctionID", commentDTO.AuctionID);

                Comment newComment = new Comment
                {
                    CommentID = ObjectId.GenerateNewId().ToString(),
                    UserID = user.UserID,
                    Username = user.Username,
                    DateCreated = DateTime.Now,
                    Message = commentDTO.Message
                };

                var update = Builders<Auction>.Update.Push("Comments", newComment);

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