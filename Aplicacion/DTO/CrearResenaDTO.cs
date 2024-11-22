using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestauranteBack.Modelo.DTO
{
    public class CrearResenaDTO
    {
        public string RestauranteId { get; set; }
        public int Calificacion { get; set; }
        public string Comentario { get; set; }
        public List<string> Imagenes { get; set; } = new();
    }

    // DTO para las reglas de validación
    public class ValidacionResenaDTO
    {
        public bool PuedeCrear { get; set; }
        public string MensajeError { get; set; }
        public DateTime? UltimaResenaFecha { get; set; }
        public int TotalResenasPrevias { get; set; }
    }
}
