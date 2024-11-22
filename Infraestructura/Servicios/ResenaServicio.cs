using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Driver;
using RestauranteBack.Modelo.DTO;
using RestauranteBack.Modelo.Entidades;
using RestauranteBack.Modelo.Interfaces;
using RestauranteBack.WebApiRestaurante.ClasesGenerales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestauranteBack.Infraestructura.Servicios
{
    public class ResenaServicio
    {
        private readonly IMongoCollection<Resena> _resenas;
        private readonly IMongoCollection<EstadisticaRestaurante> _estadisticas;
        private readonly ValidacionSesionService _validacionSesionService;
        public ResenaServicio(IMongoDatabase database, IHttpContextAccessor httpContextAccessor, ValidacionSesionService validacionSesionService)
        {
            _resenas = database.GetCollection<Resena>("resenas");
            _estadisticas = database.GetCollection<EstadisticaRestaurante>("EstadisticasRestaurante");
            _validacionSesionService = validacionSesionService;
        }
        public async Task<ResenaDTO> CrearResena(CrearResenaDTO dto)
        {
            var userId = await _validacionSesionService.obtenerDatosusuarioJWT();
            try
            {
                // Validar la entrada
                var validacionResult = await ValidarCreacionResena(dto.RestauranteId, userId);
                if (!validacionResult.PuedeCrear)
                {
                    throw new ExcepcionPeticionApi("El usuario no tiene permitido registrar una reseña", 400);
                }

                // Crear la reseña
                var resena = new Resena
                {
                    RestauranteId = dto.RestauranteId,
                    UsuarioId = userId,
                    Calificacion = Math.Clamp(dto.Calificacion, 1, 5), // Asegurar que esté entre 1 y 5
                    Comentario = dto.Comentario?.Trim(),
                    Imagenes = dto.Imagenes?.Where(img => !string.IsNullOrWhiteSpace(img)).ToList() ?? new List<string>(),
                    UsuariosMeGusta = new List<string>(),
                    UsuariosNoMeGusta = new List<string>(),
                    Respuestas = new List<Respuesta>(),
                    Fecha = DateTime.UtcNow
                };

                // Guardar la reseña
                await _resenas.InsertOneAsync(resena);

                // Actualizar las estadísticas del restaurante
                //await ActualizarEstadisticasAlCrearResena(dto.RestauranteId, resena.Calificacion);
                await ActualizarEstadisticas(dto.RestauranteId);
                // Preparar la respuesta
                var resenaDTO = new ResenaDTO
                {
                    Id = resena._id,
                    UsuarioId = resena.UsuarioId,
                    Calificacion = resena.Calificacion,
                    Comentario = resena.Comentario,
                    Imagenes = resena.Imagenes,
                    TotalMeGusta = 0,
                    TotalNoMeGusta = 0,
                    UsuarioHaDadoMeGusta = false,
                    UsuarioHaDadoNoMeGusta = false,
                    Respuestas = resena.Respuestas,
                    Fecha = resena.Fecha
                };
                var datosResena = await ObtenerResena(dto.RestauranteId, resena._id);
                return datosResena;
            }
            catch (ExcepcionPeticionApi ex)
            {

                throw ex;
            }



        }
        private async Task ActualizarEstadisticasAlCrearResena(string restauranteId, int nuevaCalificacion)
        {
            var filter = Builders<EstadisticaRestaurante>.Filter.Eq(d => d.RestauranteId, restauranteId);

            // Obtener estadísticas actuales
            var stats = await _estadisticas.Find(filter).FirstOrDefaultAsync();

            if (stats == null)
            {
                // Si no existen estadísticas, crear nuevas
                stats = new EstadisticaRestaurante
                {
                    RestauranteId = restauranteId,
                    PromedioCalificacion = nuevaCalificacion,
                    TotalResenas = 1,
                    ContadorVisitas = 0,
                    TotalMeGusta = 0,
                    TotalNoMeGusta = 0,
                    UsuariosVisitantes = new List<string>(),
                    UsuariosMeGusta = new List<string>(),
                    UsuariosNoMeGusta = new List<string>()
                };
                await _estadisticas.InsertOneAsync(stats);
            }
            else
            {
                // Actualizar estadísticas existentes
                var totalResenas = stats.TotalResenas + 1;
                var nuevoPromedio = ((stats.PromedioCalificacion * stats.TotalResenas) + nuevaCalificacion) / totalResenas;

                var update = Builders<EstadisticaRestaurante>.Update
                    .Set(d => d.PromedioCalificacion, nuevoPromedio)
                    .Inc(d => d.TotalResenas, 1);

                await _estadisticas.UpdateOneAsync(filter, update);
            }
        }
        public async Task<ResenaDTO> ObtenerResena(string restauranteId, string resenaId)
        {
            try
            {
                var userId = await _validacionSesionService.obtenerDatosusuarioJWT();

                var resena = await _resenas.Find(r =>
                    r.RestauranteId == restauranteId &&
                    r._id == resenaId)
                    .FirstOrDefaultAsync();

                if (resena == null)
                    throw new ExcepcionPeticionApi("Lo sentimos, la reseña no existe", 400);

                var resenaDTO = new ResenaDTO
                {
                    Id = resena._id,
                    UsuarioId = resena.UsuarioId,
                    Calificacion = resena.Calificacion,
                    Comentario = resena.Comentario,
                    Imagenes = resena.Imagenes,
                    TotalMeGusta = resena.UsuariosMeGusta.Count,
                    TotalNoMeGusta = resena.UsuariosNoMeGusta.Count,
                    UsuarioHaDadoMeGusta = resena.UsuariosMeGusta.Contains(userId),
                    UsuarioHaDadoNoMeGusta = resena.UsuariosNoMeGusta.Contains(userId),
                    Respuestas = resena.Respuestas,
                    Fecha = resena.Fecha
                };
                return resenaDTO;
            }
            catch (ExcepcionPeticionApi ex)
            {
                throw new ExcepcionPeticionApi(ex.Message, ex.CodigoError);
            }
        }

        private async Task<ValidacionResenaDTO> ValidarCreacionResena(string restauranteId, string userId)
        {
            try
            {
                var resultado = new ValidacionResenaDTO { PuedeCrear = true };

                // Buscar reseñas previas del usuario para este restaurante
                var resenasUsuario = await _resenas
                    .Find(r => r.RestauranteId == restauranteId && r.UsuarioId == userId)
                    .SortByDescending(r => r.Fecha)
                    .ToListAsync();

                resultado.TotalResenasPrevias = resenasUsuario.Count;

                if (resenasUsuario.Any())
                {
                    /*resultado.UltimaResenaFecha = resenasUsuario.First().Fecha;

                    // Verificar si ya existe una reseña reciente (ejemplo: en los últimos 30 días)
                    if (DateTime.UtcNow.Subtract(resultado.UltimaResenaFecha.Value).TotalDays < 30)
                    {
                        resultado.PuedeCrear = false;
                        resultado.MensajeError = "Solo puedes crear una reseña cada 30 días para el mismo restaurante";
                        return resultado;
                    }*/
                }

                return resultado;
            }
            catch (ExcepcionPeticionApi ex)
            {
                throw ex;
            }

        }
        public async Task RegistrarVisita(string documentoId, string userId)
        {
            var filter = Builders<EstadisticaRestaurante>.Filter.Eq(d => d.RestauranteId, documentoId);
            var update = await _estadisticas.UpdateOneAsync(
          filter,
          Builders<EstadisticaRestaurante>.Update
              .AddToSet(d => d.UsuariosVisitantes, userId)
              .Inc(d => d.ContadorVisitas, 1)
              .SetOnInsert(d => d.RestauranteId, documentoId)
              .SetOnInsert(d => d._id, Guid.NewGuid().ToString()),
          new UpdateOptions { IsUpsert = true }
      );
        }
        public async Task ActualizarEstadisticas(string restauranteId)
        {
            var reviews = await _resenas
                .Find(r => r.RestauranteId == restauranteId)
                .ToListAsync();

            /*if (!reviews.Any())
                return;*/

            var stats = await _estadisticas.Find(d => d.RestauranteId == restauranteId)
                .FirstOrDefaultAsync() ?? new EstadisticaRestaurante
                {
                    RestauranteId = restauranteId,
                    UsuariosVisitantes = new List<string>(),
                    UsuariosMeGusta = new List<string>(),
                    UsuariosNoMeGusta = new List<string>()
                };

            stats.PromedioCalificacion = (reviews.Count > 0 ? reviews.Average(r => r.Calificacion) : 0);
            stats.TotalResenas = (reviews.Count > 0 ? reviews.Count : 0);
            stats.TotalMeGusta = stats.UsuariosMeGusta.Count;
            stats.TotalNoMeGusta = stats.UsuariosNoMeGusta.Count;
            stats.ContadorVisitas = stats.UsuariosVisitantes.Count;

            await _estadisticas.ReplaceOneAsync(
                d => d.RestauranteId == restauranteId,
                stats,
                new ReplaceOptions { IsUpsert = true }
            );
        }
        public async Task DarMeGustaResena(string restauranteId, string resenaId)
        {
            try
            {
                var userId = await _validacionSesionService.obtenerDatosusuarioJWT();
                var review = await _resenas.Find(r => r._id == resenaId).FirstOrDefaultAsync();
                if (review == null) throw new ExcepcionPeticionApi("Lo sentimos, no pudimos obtener la reseña", 400);

                // Verificar si el usuario ya dio like
                if (review.UsuariosMeGusta.Contains(userId))
                    throw new ExcepcionPeticionApi("Lo sentimos, ya has dado me gusta a esta reseña", 400);

                // Quitar dislike si existe
                if (review.UsuariosNoMeGusta.Contains(userId))
                {
                    await QuitarNoMeGusta(restauranteId, resenaId);
                }

                // Actualizar la reseña
                var updateReview = Builders<Resena>.Update
                    .AddToSet(r => r.UsuariosMeGusta, userId);
                await _resenas.UpdateOneAsync(r => r._id == resenaId, updateReview);


                var filter = Builders<EstadisticaRestaurante>.Filter.Eq(d => d.RestauranteId, restauranteId);
                var update = await _estadisticas.UpdateOneAsync(
              filter,
              Builders<EstadisticaRestaurante>.Update
                  .AddToSet(d => d.UsuariosMeGusta, userId)
                  .Inc(d => d.TotalMeGusta, 1)
                  .SetOnInsert(d => d.RestauranteId, restauranteId)
                  .SetOnInsert(d => d._id, Guid.NewGuid().ToString()),
              new UpdateOptions { IsUpsert = true });
                await ActualizarEstadisticas(restauranteId);

            }
            catch (ExcepcionPeticionApi ex)
            {

                throw new ExcepcionPeticionApi(ex.Message, ex.CodigoError);
            }
        }
        public async Task QuitarMeGustaResena(string restauranteId, string resenaId)
        {
            try
            {
                var userId = await _validacionSesionService.obtenerDatosusuarioJWT();
                var review = await _resenas.Find(r => r._id == resenaId).FirstOrDefaultAsync();
                if (review == null) throw new ExcepcionPeticionApi("Lo sentimos, no pudimos obtener la reseña", 400);

                // Verificar si el usuario ya dio like
                /*if (!review.UsuariosMeGusta.Contains(userId))
                    throw new ExcepcionPeticionApi("Lo sentimos, no le has dado me gusta a esta reseña", 400);*/

                // Quitar dislike si existe
                if (review.UsuariosNoMeGusta.Contains(userId))
                {
                    await QuitarNoMeGusta(restauranteId, resenaId);
                }

                // Actualizar la reseña
                var updateReview = Builders<Resena>.Update
                    .Pull(r => r.UsuariosMeGusta, userId);
                await _resenas.UpdateOneAsync(r => r._id == resenaId, updateReview);


                var filter = Builders<EstadisticaRestaurante>.Filter.Eq(d => d.RestauranteId, restauranteId);
                var update = await _estadisticas.UpdateOneAsync(
              filter,
              Builders<EstadisticaRestaurante>.Update
                  .AddToSet(d => d.UsuariosMeGusta, userId)
                  .Inc(d => d.TotalMeGusta, -1)
                  .SetOnInsert(d => d.RestauranteId, restauranteId)
                  .SetOnInsert(d => d._id, Guid.NewGuid().ToString()),
              new UpdateOptions { IsUpsert = true });
                await ActualizarEstadisticas(restauranteId);

            }
            catch (ExcepcionPeticionApi ex)
            {

                throw new ExcepcionPeticionApi(ex.Message, ex.CodigoError);
            }
        }
        public async Task DarNoMeGusta(string restauranteId, string resenaId)
        {
            try
            {
                var userId = await _validacionSesionService.obtenerDatosusuarioJWT();
                var review = await _resenas.Find(r => r._id == resenaId).FirstOrDefaultAsync();
                if (review == null) throw new ExcepcionPeticionApi("Lo sentimos, no se hay resultados para esta reseña", 400);

                // Verificar si el usuario ya dio dislike
                if (review.UsuariosNoMeGusta.Contains(userId))
                    throw new ExcepcionPeticionApi("Lo sentimos, ya has dado no me gusta a esta reseña", 400);

                // Quitar like si existe
                if (review.UsuariosMeGusta.Contains(userId))
                {
                    await QuitarMeGustaResena(restauranteId, resenaId);
                }

                // Actualizar la reseña
                var updateReview = Builders<Resena>.Update
                    .AddToSet(r => r.UsuariosNoMeGusta, userId);
                await _resenas.UpdateOneAsync(r => r._id == resenaId, updateReview);


                var filter = Builders<EstadisticaRestaurante>.Filter.Eq(d => d.RestauranteId, restauranteId);
                var update = await _estadisticas.UpdateOneAsync(
              filter,
              Builders<EstadisticaRestaurante>.Update
                  .AddToSet(d => d.UsuariosNoMeGusta, userId)
                  .Inc(d => d.TotalNoMeGusta, 1)
                  .SetOnInsert(d => d.RestauranteId, restauranteId)
                  .SetOnInsert(d => d._id, Guid.NewGuid().ToString())
                  , new UpdateOptions { IsUpsert = true });
                await ActualizarEstadisticas(restauranteId);
            }
            catch (ExcepcionPeticionApi ex)
            {

                throw new ExcepcionPeticionApi(ex.Message, ex.CodigoError);
            }
        }
        public async Task QuitarNoMeGusta(string restauranteId, string resenaId)
        {
            try
            {
                var userId = await _validacionSesionService.obtenerDatosusuarioJWT();
                var review = await _resenas.Find(r => r._id == resenaId).FirstOrDefaultAsync();
                if (review == null) throw new ExcepcionPeticionApi("Lo sentimos, no hay datos de la reseña", 400);

                /*if (!review.UsuariosMeGusta.Contains(userId))
                    throw new ExcepcionPeticionApi("Lo sentimos, no has dado no me gusta a esta reseña", 400);*/

                // Actualizar la reseña
                var updateReview = Builders<Resena>.Update
                    .Pull(r => r.UsuariosNoMeGusta, userId);
                await _resenas.UpdateOneAsync(r => r._id == resenaId, updateReview);


                var filter = Builders<EstadisticaRestaurante>.Filter.Eq(d => d.RestauranteId, restauranteId);
                var update = await _estadisticas.UpdateOneAsync(
              filter,
              Builders<EstadisticaRestaurante>.Update
                  .Pull(d => d.UsuariosNoMeGusta, userId)
                  .Inc(d => d.TotalNoMeGusta, -1)
                  .SetOnInsert(d => d.RestauranteId, restauranteId)
                  .SetOnInsert(d => d._id, Guid.NewGuid().ToString()), 
              new UpdateOptions { IsUpsert = true });

                await ActualizarEstadisticas(restauranteId);
            }
            catch (ExcepcionPeticionApi ex)
            {
                throw new ExcepcionPeticionApi(ex.Message, ex.CodigoError);
            }
        }
        public async Task agregarRespuestaResena(string resenaId, Respuesta respuesta)
        {
            try
            {
                var userId = await _validacionSesionService.obtenerDatosusuarioJWT();
                respuesta.UsuarioId = userId;
                respuesta.Fecha = DateTime.UtcNow;
                respuesta._id = Guid.NewGuid().ToString();

                var update = Builders<Resena>.Update.Push(r => r.Respuestas, respuesta);
                var result = await _resenas.UpdateOneAsync(r => r._id == resenaId, update);

                if (result.ModifiedCount == 0)
                    throw new ExcepcionPeticionApi("Lo sentimos, no se pudo agregar la respuesta", 400);
            }
            catch (ExcepcionPeticionApi ex)
            {
                throw new ExcepcionPeticionApi(ex.Message, ex.CodigoError);
            }
        }
        public async Task DarMeGustaRespuesta(string resenaId, string respuestaId)
        {
            try
            {
                var userId = await _validacionSesionService.obtenerDatosusuarioJWT();

                // Actualizar usando un array filter para encontrar la respuesta específica
                var filter = Builders<Resena>.Filter.And(
                    Builders<Resena>.Filter.Eq(r => r._id, resenaId),
                    Builders<Resena>.Filter.ElemMatch(r => r.Respuestas,
                        resp => resp._id == respuestaId)
                );

                var update = Builders<Resena>.Update.AddToSet(
                    "Respuestas.$.UsuariosMeGusta",
                    userId
                );

                var result = await _resenas.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });

                if (result.ModifiedCount == 0)
                    throw new ExcepcionPeticionApi("Lo sentimos, no pudimos dar me gusta a la respuesta", 400);
            }
            catch (ExcepcionPeticionApi ex)
            {

                throw new ExcepcionPeticionApi(ex.Message, ex.CodigoError);
            }
        }
    }
}
