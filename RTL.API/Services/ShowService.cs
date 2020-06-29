using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using RTL.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RTL.API.Services
{
    public class ShowService
    {
        private readonly IMongoCollection<Show> _shows;

        public ShowService(IRTLDBSettings dbSettings)
        {
            var client = new MongoClient(dbSettings.ConnectionString);
            var database = client.GetDatabase(dbSettings.DBName);

            _shows = database.GetCollection<Show>(dbSettings.ShowsCollectionName);

        }

        public List<Show> GetShowsByPage(int pageNumber, int pageSize)
        {
            return _shows.Find(s => true).Skip((pageNumber - 1) * pageSize).Limit(pageSize).ToList();
        }

        public Show GetShowByShowId(int showId)
        {
            return _shows.Find(s => s.ShowId == showId).FirstOrDefault();
        }

        public void Create(Show show)
        {
            _shows.InsertOne(show);
        }

        public void Update(string id,Show show)
        {
            _shows.ReplaceOne(s => s.Id == id, show);
        }

        public void Upsert(Show show)
        {
            var existingShow = GetShowByShowId(show.ShowId);

            if(existingShow == null)
            {
                Create(show);
            }
            else
            {
                Update(existingShow.Id, show);
            }
        }
    }
}
