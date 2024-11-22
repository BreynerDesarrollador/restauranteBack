using RestauranteBack.Modelo.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestauranteBack.Modelo.Entidades
{
    public class EstadisticaRestaurante : BaseEntity
    {
        
        public string RestauranteId { get; set; }
        public int ContadorVisitas { get; set; }
        public double PromedioCalificacion { get; set; }
        public int TotalResenas { get; set; }
        public int TotalMeGusta { get; set; }
        public int TotalNoMeGusta { get; set; }
        public List<string> UsuariosVisitantes { get; set; } = new();
        public List<string> UsuariosMeGusta { get; set; } = new();
        public List<string> UsuariosNoMeGusta { get; set; } = new();
    }
}
