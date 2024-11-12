using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using RestauranteBack.Modelo.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestauranteBack.Modelo.Interfaces
{
    public class Maestros : BaseEntity
    {
        [BsonElement("categorias")]
        public List<string> categorias { get; set; }

        [BsonElement("ubicaciones")]
        public List<string> ubicaciones { get; set; }
    }
}
