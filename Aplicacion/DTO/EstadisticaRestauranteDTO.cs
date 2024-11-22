using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestauranteBack.Modelo.DTO
{
    public class EstadisticaRestauranteDTO
    {
        public int ContadorVisitas { get; set; }
        public double PromedioCalificacion { get; set; }
        public int TotalResenas { get; set; }
        public int TotalMeGusta { get; set; }
        public int TotalNoMeGusta { get; set; }
        public bool UsuarioHaVisitado { get; set; }
        public bool UsuarioHaDadoMeGusta { get; set; }
        public bool UsuarioHaDadoNoMeGusta { get; set; }
    }
}
