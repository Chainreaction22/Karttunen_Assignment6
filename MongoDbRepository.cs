using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;

using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using System.Linq;

namespace Homma
{
    public class MongoDbRepository : IRepository
    {
        MongoClient _client;
        IMongoDatabase _database;
        IMongoCollection<Player> _collection;
        IMongoCollection<Log> _auditLog;
        IMongoCollection<BsonDocument> _bsonDocCollection;
        IMongoCollection<BsonDocument> _bsonTestCollection;
        
        
        //public string name = "ITS-A-ME";

        public MongoDbRepository () {
            _client = new MongoClient("mongodb://localhost:27017");
            _database = _client.GetDatabase("webapi");
            _collection = _database.GetCollection<Player>("players");
            _auditLog = _database.GetCollection<Log>("logs");
            _bsonDocCollection = _database.GetCollection<BsonDocument>("players");
            _bsonTestCollection = _database.GetCollection<BsonDocument>("logs");
        }
        

        public Task<Player> Get(Guid id){
            FilterDefinition<Player> filter = Builders<Player>.Filter.Eq("Id", id);
            return _collection.Find(filter).FirstAsync();
        }
        public async Task<Player[]> GetByName(string name)
        {
            var builder = Builders<Player>.Filter;
            var filter = builder.Eq("Name", name);
            List<Player> _players = await _collection.Find(filter).ToListAsync();
            return _players.ToArray();
        }

        public async Task<Player[]> GetByTag(PlayerTag tag)
        {
            var builder = Builders<Player>.Filter;
            var filter = builder.Eq("Tag", tag);
            List<Player> _players = await _collection.Find(filter).ToListAsync();
            return _players.ToArray();
        }

        public async Task<Player[]> GetPlayersWithMinScore (int score) {
            var builder = Builders<Player>.Filter;
            var filter = builder.Gte("Score", score);
            List<Player> _players = await _collection.Find(filter).ToListAsync();
            return _players.ToArray();

        }
        public Task<int> GetLevelAggregate () {
     

            var thing = _collection.Aggregate()
                .Project(r => new {Level = r.Level})
                .Group(r => r.Level, x => new {Level = x.Key, Count = x.Sum(o=>1)})
                .SortByDescending(r => r.Count)
                .Limit(3);

            var results = thing.ToList();

            return Task.FromResult(results[0].Level);
            
        }
        public async Task<Player[]> GetAll(){
            List<Player> players = await _collection.Find(new BsonDocument()).ToListAsync();
            return players.ToArray();
        }
        public async Task<Player> Create(Player player){
            await _collection.InsertOneAsync(player);
            return player;
        }
        public async Task<Player> Modify(Guid id, ModifiedPlayer player){
            var filter = Builders<Player>.Filter.Eq("Id", id);
            Player _player = await Get(id);
            _player.Score = player.Score;
            await _collection.ReplaceOneAsync(filter, _player);
            return _player;
        }
        public async Task<Player> BanHammer(Guid id, BanPlayer player){
            var filter = Builders<Player>.Filter.Eq("Id", id);
            Player _player = await Get(id);
            _player.IsBanned = player.IsBanned;
            await _collection.ReplaceOneAsync(filter, _player);
            return _player;
        }
        public async Task<Player> ModifyInventory(Guid id, ModifiedPlayerInventory player){
            var filter = Builders<Player>.Filter.Eq("Id", id);
            Player _player = await Get(id);
            _player.Items = player.Items;
            await _collection.ReplaceOneAsync(filter, _player);
            return _player;
        }
        public async Task<Player> ModifyRecord(Guid id, ModifiedPlayerBanRecord player){
            var filter = Builders<Player>.Filter.Eq("Id", id);
            Player _player = await Get(id);
            _player.BanRecord = player.BanRecord;
            await _collection.ReplaceOneAsync(filter, _player);
            return _player;
        }
        public Task<Player> Delete(Guid id){
            var filter = Builders<Player>.Filter.Eq("Id", id);
            return _collection.FindOneAndDeleteAsync(filter);
        }




        public async Task<Item> GetItem(Guid playerId, Guid itemId){
            Player _p = await Get(playerId);
            foreach (Item i in _p.Items) {
                if (i.Id == itemId) {
                    return i;
                } 
            }
            throw new System.ArgumentException("Unable to find given ID.");
            //Item item = _p.Items.Find(x => x.Id == itemId);
        }
        public async Task<Item[]> GetAllItems(Guid playerId){
            Player _p = await Get(playerId);
            Item[] iArgh = _p.Items.ToArray();
            return iArgh;
        }
        public async Task<Item> CreateItem(Guid playerId, Item item){
            Player _p = await Get(playerId);
            _p.Items.Add(item);
            ModifiedPlayerInventory _modP = new ModifiedPlayerInventory();
            _modP.Items = _p.Items;
            await ModifyInventory(_p.Id, _modP);
            return item;
        }
        public async Task<Item> UpdateItem(Guid playerId, Guid itemId, ModifiedItem item){
            Player _p = await Get(playerId);
            for (int i = 0; i < _p.Items.Count; i++) {
                if (_p.Items[i].Id == itemId) {
                    _p.Items[i].Value = item.Value;
                    ModifiedPlayerInventory _modP = new ModifiedPlayerInventory();
                    _modP.Items = _p.Items;
                    await ModifyInventory(playerId, _modP);
                    return _p.Items[i];
                } 
            }
            throw new System.ArgumentException("Unable to find given ID.");
        }
        public async Task<Item> DeleteItem(Guid playerId, Guid itemId){
            Player _p = await Get(playerId);
            for (int i = 0; i < _p.Items.Count; i++) {
                if (_p.Items[i].Id == itemId) {
                    _p.Items.Remove(_p.Items[i]);
                    ModifiedPlayerInventory _modP = new ModifiedPlayerInventory();
                    _modP.Items = _p.Items;
                    await ModifyInventory(playerId, _modP);
                    return _p.Items[i];
                } 
            }
            throw new System.ArgumentException("Unable to find given ID.");
        }




        public async Task<Ban[]> GetAllBans(Guid playerId)
        {
            Player _p = await Get(playerId);
            Ban[] iArgh = _p.BanRecord.ToArray();
            return iArgh;
        }

        public async Task<Ban> CreateBan(Guid playerId, Ban ban)
        {
            Player _p = await Get(playerId);
            _p.BanRecord.Add(ban);
            ModifiedPlayerBanRecord _modP = new ModifiedPlayerBanRecord();
            _modP.BanRecord = _p.BanRecord;
            await ModifyRecord(_p.Id, _modP);
            return ban;
        }

        public async Task<Ban> UpdateBan(Guid playerId, Guid banId, ModifiedBan modBan)
        {
            Player _p = await Get(playerId);
            for (int i = 0; i < _p.BanRecord.Count; i++) {
                if (_p.BanRecord[i].BanId == banId) {
                    _p.BanRecord[i].Description = modBan.Description;
                    ModifiedPlayerBanRecord _modP = new ModifiedPlayerBanRecord();
                    _modP.BanRecord = _p.BanRecord;
                    await ModifyRecord(playerId, _modP);
                    return _p.BanRecord[i];
                }
            }
            throw new System.ArgumentException("Unable to find given ID.");
        }

        public async Task<Ban> DeleteBan(Guid playerId, Guid banId)
        {
            Player _p = await Get(playerId);
            for (int i = 0; i < _p.BanRecord.Count; i++) {
                if (_p.BanRecord[i].BanId == banId) {
                    _p.BanRecord.Remove(_p.BanRecord[i]);
                    ModifiedPlayerBanRecord _modP = new ModifiedPlayerBanRecord();
                    _modP.BanRecord = _p.BanRecord;
                    await ModifyRecord(playerId, _modP);
                    return _p.BanRecord[i];
                } 
            }
            throw new System.ArgumentException("Unable to find given ID.");
        }

        public async Task<Log[]> GetLog()
        {
            List<Log> entries = await _auditLog.Find(new BsonDocument()).ToListAsync();
            return entries.ToArray();
        }

        public async Task<Log> PostLog(string message)
        {
            Log log = new Log();
            log.Description = message;
            await _auditLog.InsertOneAsync(log);
            return log;
        }
    }
}