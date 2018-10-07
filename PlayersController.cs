using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Homma.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlayersController
    {
        private PlayersProcessor _process;

        public PlayersController(PlayersProcessor process) {
            this._process = process;
        }

        
        [HttpGet]
        [Route("{id:Guid}")]
        public Task<Player> Get(Guid id){
            return _process.Get(id);
        
        }

        [HttpGet]
        [Route("{name}")]
        public Task<Player[]> GetByName (string name) {
            return _process.GetByName(name);
        }
        
        [HttpGet]
        [Route("agg/ag")]
        public Task<int> GetLevelAggregate () {
            return _process.GetLevelAggregate();
        }
        
        [HttpGet]
        [Route("")]
        public Task<Player[]> GetAll(int? minScore, PlayerTag? tag, string name = null){

            if (minScore.HasValue) {
                return _process.GetPlayersWithMinScore((int)minScore);
            } else if (tag.HasValue) {
                return _process.GetByTag((PlayerTag)tag);
            } else if (!string.IsNullOrEmpty(name)) {
                Player[] temp = _process.GetByName(name).Result;
                return Task.FromResult(temp);
            }
            return _process.GetAll();
        }

        [HttpPost]
        [Route("")]
        public Task<Player> Create(NewPlayer player){
            return _process.Create(player);
        }

        [HttpPut]
        [Route ("{id}")]
        public Task<Player> Modify(Guid id, ModifiedPlayer player){
            return _process.Modify(id, player);
        }
        
        [HttpPut]
        [Route ("banPlayer/{id}")]
        public Task<Player> BanHammer(Guid id, BanPlayer player){
            return _process.BanHammer(id, player);
        }

        [ServiceFilter(typeof(ThingFilter))]
        [HttpDelete]
        [Route ("{id}")]
        public Task<Player> Delete(Guid id){
            return _process.Delete(id);
        }

        [HttpGet]
        [Route ("logg/log")]
        public Task<Log[]> GetLog(){
            return _process.GetLog();
        }
    }
}