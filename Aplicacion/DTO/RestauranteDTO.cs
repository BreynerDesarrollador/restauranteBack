using RestauranteBack.Modelo.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestauranteBack.Modelo.DTO
{
    public class RestauranteDto
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Categoria { get; set; }
        public HorarioDto Horario { get; set; }
        public UbicacionDto Ubicacion { get; set; }
        public List<MenuItemDto> Menu { get; set; }
        public List<string> Imagenes { get; set; }
        public CaracteristicasDto Caracteristicas { get; set; }
        public int ContadorVisitas { get; set; }
        public double PromedioCalificacion { get; set; }
        public int TotalResenas { get; set; }
        public int TotalMeGusta { get; set; }
        public int TotalNoMeGusta { get; set; }
        public bool MeGusta { get; set; }
        public bool NoMeGusta { get; set; }
        public List<EstadisticaRestaurante> Estadisticas { get; set; } = new();
    }

    public class HorarioDto
    {
        public DiaHorarioDto Lunes { get; set; }
        public DiaHorarioDto Martes { get; set; }
        public DiaHorarioDto Miercoles { get; set; }
        public DiaHorarioDto Jueves { get; set; }
        public DiaHorarioDto Viernes { get; set; }
        public DiaHorarioDto Sabado { get; set; }
        public DiaHorarioDto Domingo { get; set; }
    }

    public class DiaHorarioDto
    {
        public string Apertura { get; set; }
        public string Cierre { get; set; }
    }

    public class UbicacionDto
    {
        public string Direccion { get; set; }
        public string Ciudad { get; set; }
        public string Pais { get; set; }
    }

    public class MenuItemDto
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int Precio { get; set; }
        public bool Disponible { get; set; }
        public string Imagen { get; set; }
    }

    public class CaracteristicasDto
    {
        public bool TieneDelivery { get; set; }
        public bool AceptaTarjeta { get; set; }
        public bool TieneEstacionamiento { get; set; }
        public bool Wifi { get; set; }
    }

}
