using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestauranteBack.Modelo.DTO
{
    public class BusquedaResenasParametros
    {
        public int Pagina { get; set; } = 1;
        public int ElementosPorPagina { get; set; } = 10;
        public string OrdenarPor { get; set; } = "Fecha"; // Fecha, Calificacion, MeGusta
        public bool Descendente { get; set; } = true;
        public int? CalificacionMinima { get; set; }
        public bool? SoloConImagenes { get; set; }
        public bool? SoloConRespuestas { get; set; }
    }
    public class RespuestaPaginada<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public int TotalElementos { get; set; }
    }
}
