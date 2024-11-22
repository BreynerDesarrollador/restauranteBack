using RestauranteBack.Modelo.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestauranteBack.Modelo.DTO
{
    public class ResenaDTO
    {
        public string Id { get; set; }
        public string UsuarioId { get; set; }
        public int Calificacion { get; set; }
        public string Comentario { get; set; }
        public List<string> Imagenes { get; set; }
        public int TotalMeGusta { get; set; }
        public int TotalNoMeGusta { get; set; }
        public bool UsuarioHaDadoMeGusta { get; set; }
        public bool UsuarioHaDadoNoMeGusta { get; set; }
        public List<Respuesta> Respuestas { get; set; }
        public DateTime Fecha { get; set; }
    }
}
