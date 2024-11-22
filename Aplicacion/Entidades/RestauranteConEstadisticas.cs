using MongoDB.Bson;
using RestauranteBack.Modelo.Common;
using RestauranteBack.Modelo.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestauranteBack.Modelo.Entidades
{
    public class RestauranteConEstadisticas : BaseEntity
    {
        public string nombre { get; set; }
        public string descripcion { get; set; }
        public string categoria { get; set; }
        public string imagenPrincipal { get; set; }
        public Horario horario { get; set; }
        public Ubicacion ubicacion { get; set; }
        public List<MenuItem> menu { get; set; }
        public List<string> imagenes { get; set; }
        public Caracteristicas caracteristicas { get; set; }

        // Campo adicional para estadísticas
        public List<EstadisticaRestaurante> EstadisticasRestaurante { get; set; } = new();
    }
}
