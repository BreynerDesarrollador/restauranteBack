using RestauranteBack.Modelo.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestauranteBack.Modelo.Entidades
{
    public class Resena : BaseEntity
    {
        public string RestauranteId { get; set; }
        public string UsuarioId { get; set; }
        public int Calificacion { get; set; }
        public string Comentario { get; set; }
        public List<string> Imagenes { get; set; } = new();
        public List<string> UsuariosMeGusta { get; set; } = new();
        public List<string> UsuariosNoMeGusta { get; set; } = new();
        public List<Respuesta> Respuestas { get; set; } = new();
        public DateTime Fecha { get; set; }
    }
}
