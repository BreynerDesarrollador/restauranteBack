using RestauranteBack.Modelo.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestauranteBack.Modelo.Entidades
{
    public class Respuesta : BaseEntity
    {
        public string UsuarioId { get; set; }
        public string Comentario { get; set; }
        public DateTime Fecha { get; set; }
        public List<string> UsuariosMeGusta { get; set; } = new();
        public List<string> UsuariosNoMeGusta { get; set; } = new();
    }
}
