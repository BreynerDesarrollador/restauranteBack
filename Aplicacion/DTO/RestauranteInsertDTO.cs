using RestauranteBack.Modelo.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestauranteBack.Modelo.DTO
{
    public class RestauranteInsertDTO : BaseEntity
    {
        public string nombre { get; set; }
        public string descripcion { get; set; }
        public string categoria { get; set; }
        public string imagenPrincipal { get; set; }
        public HorarioDto horario { get; set; }
        public UbicacionDto ubicacion { get; set; }
        public List<MenuDto> menu { get; set; }
        public List<string> imagenes { get; set; } // Almacena imágenes en base64
        public CaracteristicasDto Caracteristicas { get; set; }
    }


    public class DiaDto
    {
        public string apertura { get; set; }
        public string cierre { get; set; }
    }



    public class CoordenadasDto
    {
        public double latitud { get; set; }
        public double longitud { get; set; }
    }

    public class MenuDto
    {
        public string nombre { get; set; }
        public string descripcion { get; set; }
        public double precio { get; set; }
        public string categoria { get; set; }
        public bool disponible { get; set; }
        public List<string> ingredientes { get; set; }
        public string imagen { get; set; } // Imagen en base64
    }


}
