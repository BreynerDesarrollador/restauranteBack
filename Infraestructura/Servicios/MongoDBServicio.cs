using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestauranteBack.Infraestructura.Servicios
{
    public class MongoDBServicio
    {
        private readonly IMongoDatabase _mongoDatabase;

        public MongoDBServicio(IMongoDatabase mongoDatabase)
        {
            _mongoDatabase = mongoDatabase;
        }

        // Métodos para interactuar con la base de datos
    }
}
