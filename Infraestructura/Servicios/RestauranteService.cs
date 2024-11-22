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
    public class RestauranteService
    {
        private readonly IMongoCollection<RestauranteConEstadisticas> _restaurantes;
        private readonly IMongoCollection<EstadisticaRestaurante> _estadisticas;
        private readonly IMongoCollection<Resena> _resenas;
        private readonly ValidacionSesionService _validacionSesionService;
        private readonly ResenaServicio _resenaServicio;
        public RestauranteService(IMongoDatabase database, ValidacionSesionService validacionSesionService, ResenaServicio resenaServicio)
        {
            _restaurantes = database.GetCollection<RestauranteConEstadisticas>("restaurantes");
            _estadisticas = database.GetCollection<EstadisticaRestaurante>("EstadisticasRestaurante");
            _resenas = database.GetCollection<Resena>("resenas");
            _validacionSesionService = validacionSesionService;
            _resenaServicio = resenaServicio;
        }

        // Método para crear un nuevo restaurante
        public async Task<RestauranteDto> CrearRestauranteAsync(RestauranteDto restauranteDto)
        {
            var restaurante = MapearEntidad(restauranteDto);
            await _restaurantes.InsertOneAsync(restaurante);
            return MapToDto(restaurante);
        }

        // Método para obtener todos los restaurantes
        public async Task<List<RestauranteDto>> ObtenerRestaurantesAsync(string buscar, string categoria, string ubicacion)
        {
            var userId = string.Empty;
            try
            {
                try
                {
                    userId = await _validacionSesionService.obtenerDatosusuarioJWT();
                }
                catch (Exception ex)
                {
                    userId = string.Empty;
                }

                var filter = Builders<RestauranteConEstadisticas>.Filter.Empty;

                if (!string.IsNullOrEmpty(buscar))
                {
                    var searchPattern = new BsonRegularExpression($".*{buscar}.*", "i");
                    var nombreFilter = Builders<RestauranteConEstadisticas>.Filter.Regex("nombre", searchPattern);
                    var descripcionFilter = Builders<RestauranteConEstadisticas>.Filter.Regex("descripcion", searchPattern);
                    filter &= Builders<RestauranteConEstadisticas>.Filter.Or(nombreFilter, descripcionFilter);
                }

                if (!string.IsNullOrEmpty(categoria))
                {
                    // Usar regex para hacer la búsqueda de categoría case-insensitive
                    var categoriaPattern = new BsonRegularExpression($"^{categoria}$", "i");
                    filter &= Builders<RestauranteConEstadisticas>.Filter.Regex("categoria", categoriaPattern);
                }

                if (!string.IsNullOrEmpty(ubicacion))
                {
                    // También hacer case-insensitive la búsqueda por ubicación
                    var ubicacionPattern = new BsonRegularExpression($"^{ubicacion}$", "i");
                    filter &= Builders<RestauranteConEstadisticas>.Filter.Regex("ubicacion.ciudad", ubicacionPattern);
                }

                var datosRestaurante = await _restaurantes.Aggregate()
            .Match(filter) // Aplica el filtro inicial a los restaurantes
            .Lookup<EstadisticaRestaurante, RestauranteConEstadisticas>(
                foreignCollectionName: "EstadisticasRestaurante",    // Colección de estadísticas
                localField: "_id",                  // Campo en la colección de restaurantes
                foreignField: "RestauranteId",      // Campo en la colección de estadísticas
                @as: "EstadisticasRestaurante"                 // Campo donde se guarda el resultado del join
            )
            .ToListAsync();
                var resultado = datosRestaurante.Select(r =>
                {
                    var estadisticas = r.EstadisticasRestaurante.FirstOrDefault();

                    var dto = MapToDto(r);
                    dto.MeGusta = estadisticas?.UsuariosMeGusta.Contains(userId) ?? false;
                    dto.NoMeGusta = estadisticas?.UsuariosNoMeGusta.Contains(userId) ?? false;

                    return dto;
                }).ToList();

                return resultado;
            }
            catch (Exception ex)
            {
                throw new ExcepcionPeticionApi(ex.Message, 500);
            }
        }

        // Método para obtener un restaurante por Id
        public async Task<RestauranteDto> ObtenerRestaurantePorIdAsync(string id)
        {
            try
            {
                //var restaurante = await _restaurantes.Find(r => r.Id == id).FirstOrDefaultAsync();

                // Construir el pipeline de agregación
                var pipeline = new[]
                {
        new BsonDocument("$match", new BsonDocument("_id", id)), // Filtrar el restaurante por ID
        new BsonDocument("$lookup", new BsonDocument
        {
            { "from", "EstadisticasRestaurante" }, // Nombre de la colección relacionada
            { "localField", "_id" },             // Campo en la colección principal
            { "foreignField", "RestauranteId" }, // Campo en la colección relacionada
            { "as", "EstadisticasRestaurante" }             // Nombre del campo resultante
        })
    };

                // Ejecutar el pipeline
                var datosRestaurante = await _restaurantes.Aggregate<RestauranteConEstadisticas>(pipeline).FirstOrDefaultAsync();
                return datosRestaurante != null ? MapToDto(datosRestaurante) : null;
            }
            catch (ExcepcionPeticionApi ex)
            {

                throw ex;
            }

        }

        // Método para actualizar un restaurante
        public async Task<bool> ActualizarRestauranteAsync(string id, RestauranteDto restauranteDto)
        {
            var restaurante = MapearEntidad(restauranteDto);
            restaurante._id = id;
            var resultado = await _restaurantes.ReplaceOneAsync(r => r._id == id, restaurante);
            return resultado.ModifiedCount > 0;
        }

        // Método para eliminar un restaurante
        public async Task<bool> EliminarRestauranteAsync(string id)
        {
            var resultado = await _restaurantes.DeleteOneAsync(r => r._id == id);
            return resultado.DeletedCount > 0;
        }

        public async Task DarMeGustaRestaurante(string restauranteId)
        {
            try
            {
                var userId = await _validacionSesionService.obtenerDatosusuarioJWT();

                var stats = await _estadisticas.Find(d => d.RestauranteId == restauranteId).FirstOrDefaultAsync();
                if (stats?.UsuariosMeGusta.Contains(userId) == true)
                    throw new ExcepcionPeticionApi("Ya has dado like a este restaurante", 400);

                // Si el usuario tiene un dislike, lo quitamos primero
                if (stats?.UsuariosNoMeGusta.Contains(userId) == true)
                {
                    await QuitarNoMeGustaRestaurante(restauranteId);
                }

                var update = await _estadisticas.UpdateOneAsync(
    Builders<EstadisticaRestaurante>.Filter.Eq(d => d.RestauranteId, restauranteId), // Usa un string válido como ID
    Builders<EstadisticaRestaurante>.Update
        .AddToSet(d => d.UsuariosMeGusta, userId)
        .Inc(d => d.TotalMeGusta, 1)
        .SetOnInsert(d => d.RestauranteId, restauranteId)
        .SetOnInsert(d => d._id, Guid.NewGuid().ToString()),
    new UpdateOptions { IsUpsert = true, Hint = new BsonDocument("_id", 1) }
);
                await _resenaServicio.ActualizarEstadisticas(restauranteId);
            }
            catch (ExcepcionPeticionApi ex)
            {
                throw new ExcepcionPeticionApi(ex.Message, ex.CodigoError);
            }
        }
        public async Task QuitarMeGustaRestaurante(string restauranteId)
        {
            try
            {
                var userId = await _validacionSesionService.obtenerDatosusuarioJWT();

                /*var stats = await _estadisticas.Find(d => d.RestauranteId == restauranteId).FirstOrDefaultAsync();
                if (stats == null || !stats.UsuariosNoMeGusta.Contains(userId))
                    throw new ExcepcionPeticionApi("No has dado dislike a este restaurante", 400);*/

                var filter = Builders<EstadisticaRestaurante>.Filter.Eq(d => d.RestauranteId, restauranteId);
                var update = await _estadisticas.UpdateOneAsync(
                    filter,
                    Builders<EstadisticaRestaurante>.Update
                        .Pull(d => d.UsuariosMeGusta, userId)
                        .Inc(d => d.TotalMeGusta, -1)
                );
                await _resenaServicio.ActualizarEstadisticas(restauranteId);
            }
            catch (ExcepcionPeticionApi ex)
            {
                throw new ExcepcionPeticionApi(ex.Message, ex.CodigoError);
            }
        }
        public async Task DarNoMeGustaRestaurante(string restauranteId)
        {
            try
            {
                var userId = await _validacionSesionService.obtenerDatosusuarioJWT();

                var stats = await _estadisticas.Find(d => d.RestauranteId == restauranteId).FirstOrDefaultAsync();

                if (stats?.UsuariosNoMeGusta.Contains(userId) == true)
                    throw new ExcepcionPeticionApi("Lo sentimos, ya has dado no me gusta a este restaurante", 400);

                // Si el usuario tiene un like, lo quitamos primero
                if (stats?.UsuariosMeGusta.Contains(userId) == true)
                {
                    await QuitarMeGustaRestaurante(restauranteId);
                }

                var filter = Builders<EstadisticaRestaurante>.Filter.Eq(d => d.RestauranteId, restauranteId);
                var update = await _estadisticas.UpdateOneAsync(
                    filter,
                    Builders<EstadisticaRestaurante>.Update
                        .AddToSet(d => d.UsuariosNoMeGusta, userId)
                        .Inc(d => d.TotalNoMeGusta, 1)
                        .SetOnInsert(d => d.RestauranteId, restauranteId)
                        .SetOnInsert(d => d._id, Guid.NewGuid().ToString()),
                    new UpdateOptions { IsUpsert = true }
                );
                await _resenaServicio.ActualizarEstadisticas(restauranteId);
            }
            catch (ExcepcionPeticionApi ex)
            {
                throw new ExcepcionPeticionApi(ex.Message, ex.CodigoError);
            }
        }
        public async Task QuitarNoMeGustaRestaurante(string restauranteId)
        {
            try
            {
                var userId = await _validacionSesionService.obtenerDatosusuarioJWT();

                var stats = await _estadisticas.Find(d => d.RestauranteId == restauranteId).FirstOrDefaultAsync();
                if (stats == null || !stats.UsuariosNoMeGusta.Contains(userId))
                    throw new ExcepcionPeticionApi("No has dado dislike a este restaurante", 400);

                var filter = Builders<EstadisticaRestaurante>.Filter.Eq(d => d.RestauranteId, restauranteId);
                var update = await _estadisticas.UpdateOneAsync(
                    filter,
                    Builders<EstadisticaRestaurante>.Update
                        .Pull(d => d.UsuariosNoMeGusta, userId)
                        .Inc(d => d.TotalNoMeGusta, -1)
                );
                await _resenaServicio.ActualizarEstadisticas(restauranteId);
            }
            catch (ExcepcionPeticionApi ex)
            {
                throw new ExcepcionPeticionApi(ex.Message, ex.CodigoError);
            }
        }
        public async Task<RespuestaPaginada<ResenaDTO>> ObtenerResenasRestaurante(string restauranteId, BusquedaResenasParametros parametros)
        {
            try
            {
                var userId = await _validacionSesionService.obtenerDatosusuarioJWT();

                // Construir el filtro base
                var filterBuilder = Builders<Resena>.Filter;
                var filter = filterBuilder.Eq(r => r.RestauranteId, restauranteId);

                // Aplicar filtros adicionales
                if (parametros.CalificacionMinima.HasValue)
                {
                    filter &= filterBuilder.Gte(r => r.Calificacion, parametros.CalificacionMinima.Value);
                }

                if (parametros.SoloConImagenes == true)
                {
                    filter &= filterBuilder.SizeGt(r => r.Imagenes, 0);
                }

                if (parametros.SoloConRespuestas == true)
                {
                    filter &= filterBuilder.SizeGt(r => r.Respuestas, 0);
                }

                // Construir el ordenamiento
                var sort = parametros.OrdenarPor.ToLower() switch
                {
                    "calificacion" => parametros.Descendente
                        ? Builders<Resena>.Sort.Descending(r => r.Calificacion)
                        : Builders<Resena>.Sort.Ascending(r => r.Calificacion),
                    "megusta" => parametros.Descendente
                        ? Builders<Resena>.Sort.Descending(r => r.UsuariosMeGusta.Count)
                        : Builders<Resena>.Sort.Ascending(r => r.UsuariosMeGusta.Count),
                    _ => parametros.Descendente
                        ? Builders<Resena>.Sort.Descending(r => r.Fecha)
                        : Builders<Resena>.Sort.Ascending(r => r.Fecha)
                };

                // Calcular el total de elementos y páginas
                var totalElementos = await _resenas.CountDocumentsAsync(filter);
                var totalPaginas = (int)Math.Ceiling(totalElementos / (double)parametros.ElementosPorPagina);

                // Obtener los elementos de la página actual
                var resenas = await _resenas.Find(filter)
                    .Sort(sort)
                    .Skip((parametros.Pagina - 1) * parametros.ElementosPorPagina)
                    .Limit(parametros.ElementosPorPagina)
                    .ToListAsync();

                // Mapear a DTO
                var resenasDTO = resenas.Select(r => new ResenaDTO
                {
                    Id = r._id,
                    UsuarioId = r.UsuarioId,
                    Calificacion = r.Calificacion,
                    Comentario = r.Comentario,
                    Imagenes = r.Imagenes,
                    TotalMeGusta = r.UsuariosMeGusta.Count,
                    TotalNoMeGusta = r.UsuariosNoMeGusta.Count,
                    UsuarioHaDadoMeGusta = r.UsuariosMeGusta.Contains(userId),
                    UsuarioHaDadoNoMeGusta = r.UsuariosNoMeGusta.Contains(userId),
                    Respuestas = r.Respuestas,
                    Fecha = r.Fecha
                });

                var resultado = new RespuestaPaginada<ResenaDTO>
                {
                    Items = resenasDTO,
                    PaginaActual = parametros.Pagina,
                    TotalPaginas = totalPaginas,
                    TotalElementos = (int)totalElementos
                };

                // Registrar la visita al restaurante
                await _resenaServicio.RegistrarVisita(restauranteId, userId);
                return resultado;
            }
            catch (ExcepcionPeticionApi ex)
            {
                throw new ExcepcionPeticionApi(ex.Message, ex.CodigoError);
            }
        }
        public async Task<EstadisticaRestauranteDTO> estadisticasRestuarante(string restauranteId)
        {
            try
            {
                var userId = await _validacionSesionService.obtenerDatosusuarioJWT();
                var stats = await _estadisticas
                    .Find(d => d.RestauranteId == restauranteId)
                    .FirstOrDefaultAsync();

                if (stats == null)
                {
                    await _resenaServicio.ActualizarEstadisticas(restauranteId);
                    stats = await _estadisticas
                        .Find(d => d.RestauranteId == restauranteId)
                        .FirstOrDefaultAsync();
                }

                var statsDto = new EstadisticaRestauranteDTO
                {
                    ContadorVisitas = stats.ContadorVisitas,
                    PromedioCalificacion = stats.PromedioCalificacion,
                    TotalResenas = stats.TotalResenas,
                    TotalMeGusta = stats.TotalMeGusta,
                    TotalNoMeGusta = stats.TotalNoMeGusta,
                    UsuarioHaVisitado = stats.UsuariosVisitantes.Contains(userId),
                    UsuarioHaDadoMeGusta = stats.UsuariosMeGusta.Contains(userId),
                    UsuarioHaDadoNoMeGusta = stats.UsuariosNoMeGusta.Contains(userId)
                };
                return statsDto;
            }
            catch (ExcepcionPeticionApi ex)
            {

                throw new ExcepcionPeticionApi(ex.Message, ex.CodigoError);
            }
        }
        public async Task registrarVisita(string idRestaurante)
        {
            try
            {
                var userId = await _validacionSesionService.obtenerDatosusuarioJWT();
                await _resenaServicio.RegistrarVisita(idRestaurante, userId);

                /*var reviews = await _resenas
                    .Find(r => r.RestauranteId == idRestaurante)
                    .ToListAsync();*/
            }
            catch (ExcepcionPeticionApi ex)
            {
                throw new ExcepcionPeticionApi(ex.Message, ex.CodigoError);
            }
        }
        public async Task ActualizarImagenesAsync(string restauranteId, ActualizarImagenesRestauranteDto dto)
        {
            // Obtener el restaurante de la base de datos
            var restaurante = await ObtenerRestaurantePorIdAsync(restauranteId);

            if (restaurante == null)
            {
                throw new Exception("Restaurante no encontrado");
            }

            // Actualizar imágenes
            restaurante.imagenPrincipal = dto.imagenPrincipal;
            //restaurante.imagenPrincipal = dto.imagenPrincipal;

            // Guardar cambios
            var datosRestaurante = await _restaurantes.Find(d => d._id == restauranteId)
                .FirstOrDefaultAsync();

            datosRestaurante.imagenPrincipal = "data:image/jpg;base64," + dto.imagenPrincipal;

            await _restaurantes.ReplaceOneAsync(
                d => d._id == restauranteId,
                datosRestaurante,
                new ReplaceOptions { IsUpsert = true }
            );
        }
        // Método para mapear DTO a entidad
        private RestauranteConEstadisticas MapearEntidad(RestauranteDto dto) => new RestauranteConEstadisticas
        {
            _id = dto.Id,
            nombre = dto.Nombre,
            descripcion = dto.Descripcion,
            categoria = dto.Categoria,
            horario = new Horario
            {
                Lunes = MapToDiaHorario(dto.Horario.Lunes),
                Martes = MapToDiaHorario(dto.Horario.Martes),
                Miercoles = MapToDiaHorario(dto.Horario.Miercoles),
                Jueves = MapToDiaHorario(dto.Horario.Jueves),
                Viernes = MapToDiaHorario(dto.Horario.Viernes),
                Sabado = MapToDiaHorario(dto.Horario.Sabado),
                Domingo = MapToDiaHorario(dto.Horario.Domingo)
            },
            ubicacion = new Ubicacion
            {
                Direccion = dto.Ubicacion.Direccion,
                Ciudad = dto.Ubicacion.Ciudad,
                Pais = dto.Ubicacion.Pais
            },
            menu = dto.Menu?.ConvertAll(menuItem => new MenuItem
            {
                Nombre = menuItem.Nombre,
                Descripcion = menuItem.Descripcion,
                Precio = menuItem.Precio,
                Disponible = menuItem.Disponible,
                Imagen = menuItem.Imagen
            }),
            imagenPrincipal = dto.imagenPrincipal,
            imagenes = dto.Imagenes,
            caracteristicas = new Caracteristicas
            {
                TieneDelivery = dto.Caracteristicas.TieneDelivery,
                AceptaTarjeta = dto.Caracteristicas.AceptaTarjeta,
                TieneEstacionamiento = dto.Caracteristicas.TieneEstacionamiento,
                Wifi = dto.Caracteristicas.Wifi
            }
        };

        // Método para mapear entidad a DTO
        private RestauranteDto MapToDto(RestauranteConEstadisticas entity)
        {
            // Mapear datos básicos del restaurante
            var dto = new RestauranteDto
            {
                Id = entity._id.ToString(),
                Nombre = entity.nombre,
                Descripcion = entity.descripcion,
                Categoria = entity.categoria,
                Horario = new HorarioDto
                {
                    Lunes = MapToDiaHorarioDto(entity.horario.Lunes),
                    Martes = MapToDiaHorarioDto(entity.horario.Martes),
                    Miercoles = MapToDiaHorarioDto(entity.horario.Miercoles),
                    Jueves = MapToDiaHorarioDto(entity.horario.Jueves),
                    Viernes = MapToDiaHorarioDto(entity.horario.Viernes),
                    Sabado = MapToDiaHorarioDto(entity.horario.Sabado),
                    Domingo = MapToDiaHorarioDto(entity.horario.Domingo)
                },
                Ubicacion = new UbicacionDto
                {
                    Direccion = entity.ubicacion.Direccion,
                    Ciudad = entity.ubicacion.Ciudad,
                    Pais = entity.ubicacion.Pais
                },
                Menu = entity.menu?.ConvertAll(menuItem => new MenuItemDto
                {
                    Nombre = menuItem.Nombre,
                    Descripcion = menuItem.Descripcion,
                    Precio = menuItem.Precio,
                    Disponible = menuItem.Disponible,
                    Imagen = menuItem.Imagen
                }),
                imagenPrincipal = entity.imagenPrincipal,
                Imagenes = entity.imagenes,
                Caracteristicas = new CaracteristicasDto
                {
                    TieneDelivery = entity.caracteristicas.TieneDelivery,
                    AceptaTarjeta = entity.caracteristicas.AceptaTarjeta,
                    TieneEstacionamiento = entity.caracteristicas.TieneEstacionamiento,
                    Wifi = entity.caracteristicas.Wifi
                }
            };
            // Mapear estadísticas (si existen)
            var estadisticas = entity.EstadisticasRestaurante?.FirstOrDefault();
            if (estadisticas != null)
            {
                dto.Estadisticas = new List<EstadisticaRestaurante>() {
                new EstadisticaRestaurante
                {
                    _id = estadisticas._id,
                    RestauranteId=estadisticas.RestauranteId,
                    ContadorVisitas=estadisticas.ContadorVisitas,
                    PromedioCalificacion=estadisticas.PromedioCalificacion,
                    TotalResenas=estadisticas.TotalResenas,
                    TotalMeGusta=estadisticas.TotalMeGusta,
                    TotalNoMeGusta=estadisticas.TotalNoMeGusta,
                    UsuariosVisitantes=estadisticas.UsuariosVisitantes,
                    UsuariosMeGusta=estadisticas.UsuariosMeGusta,
                    UsuariosNoMeGusta=estadisticas.UsuariosNoMeGusta
                }
                };
                dto.ContadorVisitas = estadisticas.ContadorVisitas;
                dto.PromedioCalificacion = estadisticas.PromedioCalificacion;
                dto.TotalResenas = estadisticas.TotalResenas;
                dto.TotalMeGusta = estadisticas.TotalMeGusta;
                dto.TotalNoMeGusta = estadisticas.TotalNoMeGusta;
            }

            return dto;
        }


        private DiaHorario MapToDiaHorario(DiaHorarioDto dto) => new DiaHorario { Apertura = dto.Apertura, Cierre = dto.Cierre };
        private DiaHorarioDto MapToDiaHorarioDto(DiaHorario diaHorario) => new DiaHorarioDto { Apertura = diaHorario.Apertura, Cierre = diaHorario.Cierre };
    }
}
