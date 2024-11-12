using MongoDB.Driver;
using RestauranteBack.Modelo.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestauranteBack.Modelo.Provider
{
    public class MongoDBProvider
    {
        private readonly IMongoDatabase _database;

        public MongoDBProvider(IMongoDatabase database)
        {
            _database = database;
        }

        public IMongoCollection<Usuario> GetUsuariosCollection()
        {
            return _database.GetCollection<Usuario>("usuarios");
        }

        // Agrega métodos para otras colecciones si es necesario
    }
}
