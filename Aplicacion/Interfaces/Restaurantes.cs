using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using RestauranteBack.Modelo.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.PortableExecutable;
using RestauranteBack.Modelo.Entidades;

namespace RestauranteBack.Modelo.Interfaces
{
    public class Restaurante : BaseEntity
    {

        [BsonElement("nombre")]
        public string Nombre { get; set; }

        [BsonElement("descripcion")]
        public string Descripcion { get; set; }

        [BsonElement("categoria")]
        public string Categoria { get; set; }

        [BsonElement("horario")]
        public Horario Horario { get; set; }

        [BsonElement("ubicacion")]
        public Ubicacion Ubicacion { get; set; }

        [BsonElement("menu")]
        public List<MenuItem> Menu { get; set; }

        [BsonElement("imagenes")]
        public List<string> Imagenes { get; set; }

        [BsonElement("caracteristicas")]
        public Caracteristicas Caracteristicas { get; set; }
        public List<EstadisticaRestaurante> Estadisticas { get; set; } = new();
    }
    public class Horario
    {
        [BsonElement("lunes")]
        public DiaHorario Lunes { get; set; }

        [BsonElement("martes")]
        public DiaHorario Martes { get; set; }

        [BsonElement("miercoles")]
        public DiaHorario Miercoles { get; set; }

        [BsonElement("jueves")]
        public DiaHorario Jueves { get; set; }

        [BsonElement("viernes")]
        public DiaHorario Viernes { get; set; }

        [BsonElement("sabado")]
        public DiaHorario Sabado { get; set; }

        [BsonElement("domingo")]
        public DiaHorario Domingo { get; set; }
    }

    public class DiaHorario
    {
        [BsonElement("apertura")]
        public string Apertura { get; set; }

        [BsonElement("cierre")]
        public string Cierre { get; set; }
    }

    public class Ubicacion
    {
        [BsonElement("direccion")]
        public string Direccion { get; set; }

        [BsonElement("ciudad")]
        public string Ciudad { get; set; }

        [BsonElement("pais")]
        public string Pais { get; set; }

        [BsonElement("coordenadas")]
        public Coordenadas Coordenadas { get; set; }
    }

    public class Coordenadas
    {
        [BsonElement("latitud")]
        public double Latitud { get; set; }

        [BsonElement("longitud")]
        public double Longitud { get; set; }
    }

    public class MenuItem
    {
        [BsonElement("nombre")]
        public string Nombre { get; set; }

        [BsonElement("descripcion")]
        public string Descripcion { get; set; }

        [BsonElement("precio")]
        public int Precio { get; set; }

        [BsonElement("categoria")]
        public string Categoria { get; set; }

        [BsonElement("disponible")]
        public bool Disponible { get; set; }

        [BsonElement("ingredientes")]
        public List<string> Ingredientes { get; set; }

        [BsonElement("imagen")]
        public string Imagen { get; set; }
    }

    public class Caracteristicas
    {
        [BsonElement("tieneDelivery")]
        public bool TieneDelivery { get; set; }

        [BsonElement("aceptaTarjeta")]
        public bool AceptaTarjeta { get; set; }

        [BsonElement("tieneEstacionamiento")]
        public bool TieneEstacionamiento { get; set; }

        [BsonElement("wifi")]
        public bool Wifi { get; set; }
    }
}
