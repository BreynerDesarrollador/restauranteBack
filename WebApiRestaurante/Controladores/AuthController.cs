using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestauranteBack.Infraestructura.Servicios;
using RestauranteBack.Modelo.DTO;
using RestauranteBack.WebApiRestaurante.ClasesGenerales;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestauranteBack.WebApiRestaurante.Controladores
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<InicioSesionRespuestaDto>> Login(InicioSesionDto loginDto)
        {
            try
            {
                var response = await _authService.LoginAsync(loginDto);
                return Ok(new RespuestaWebApi<object>
                {
                    data = response
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Credenciales inválidas" });
            }
            catch (ExcepcionPeticionApi ex)
            {
                return StatusCode(ex.CodigoError, new RespuestaWebApi<object>
                {
                    exito = false,
                    mensaje = ex.Message
                });
            }
        }

        [Authorize]
        [HttpGet("verify")]
        public async Task<ActionResult<UsuarioDTO>> VerifyToken()
        {
            try
            {
                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var user = await _authService.VerifyTokenAsync(token);
                return Ok(new { user });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Token inválido" });
            }
            catch (ExcepcionPeticionApi ex)
            {
                return StatusCode(ex.CodigoError, new RespuestaWebApi<object>
                {
                    exito = false,
                    mensaje = ex.Message
                });
            }
        }

        // Crear usuario
        [HttpPost("register")]
        public async Task<ActionResult<RespuestaWebApi<object>>> Register(UsuarioDTO usuarioDto)
        {
            try
            {
                var usuario = await _authService.CreateUserAsync(usuarioDto);
                return Ok(new RespuestaWebApi<object> { exito = true, mensaje = "Usuario registrado con éxito", data = usuario });
            }
            catch (ExcepcionPeticionApi ex)
            {
                return StatusCode(ex.CodigoError, new RespuestaWebApi<object>
                {
                    exito = false,
                    mensaje = ex.Message
                });
            }
        }

        // Obtener todos los usuarios
        [HttpGet]
        public async Task<ActionResult<RespuestaWebApi<IEnumerable<UsuarioDTO>>>> GetAllUsers()
        {
            try
            {
                var usuarios = await _authService.GetAllUsersAsync();
                return Ok(new RespuestaWebApi<IEnumerable<UsuarioDTO>> { exito = true, mensaje = "Usuarios obtenidos con éxito", data = usuarios });
            }
            catch (ExcepcionPeticionApi ex)
            {
                return StatusCode(ex.CodigoError, new RespuestaWebApi<object>
                {
                    exito = false,
                    mensaje = ex.Message
                });
            }
        }

        // Obtener usuario por ID
        [HttpGet("{id}")]
        public async Task<ActionResult<RespuestaWebApi<UsuarioDTO>>> GetUserById(string id)
        {
            try
            {
                var usuario = await _authService.GetUserByIdAsync(id);
                if (usuario == null)
                {
                    return NotFound(new RespuestaWebApi<object> { exito = false, mensaje = "Usuario no encontrado" });
                }
                return Ok(new RespuestaWebApi<UsuarioDTO> { exito = true, mensaje = "Usuario obtenido con éxito", data = usuario });
            }
            catch (ExcepcionPeticionApi ex)
            {
                return StatusCode(ex.CodigoError, new RespuestaWebApi<object>
                {
                    exito = false,
                    mensaje = ex.Message
                });
            }
        }

        // Actualizar usuario
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, UsuarioDTO usuarioDto)
        {
            try
            {
                if (id != usuarioDto.Id)
                {
                    return BadRequest(new RespuestaWebApi<object> { exito = false, mensaje = "ID del usuario no coincide" });
                }

                var updated = await _authService.UpdateUserAsync(id, usuarioDto);
                if (!updated)
                {
                    return NotFound(new RespuestaWebApi<object> { exito = false, mensaje = "Usuario no encontrado" });
                }

                return NoContent();
            }
            catch (ExcepcionPeticionApi ex)
            {
                return StatusCode(ex.CodigoError, new RespuestaWebApi<object>
                {
                    exito = false,
                    mensaje = ex.Message
                });
            }
        }

        // Eliminar usuario
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                var deleted = await _authService.DeleteUserAsync(id);
                if (!deleted)
                {
                    return NotFound(new RespuestaWebApi<object> { exito = false, mensaje = "Usuario no encontrado" });
                }

                return NoContent();
            }
            catch (ExcepcionPeticionApi ex)
            {
                return StatusCode(ex.CodigoError, new RespuestaWebApi<object>
                {
                    exito = false,
                    mensaje = ex.Message
                });
            }
        }
    }
}