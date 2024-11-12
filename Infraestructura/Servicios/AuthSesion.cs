using RestauranteBack.Modelo.DTO;

public interface IAuthService
{
    Task<InicioSesionRespuestaDto> LoginAsync(InicioSesionDto loginDto);
    Task<UsuarioDTO> VerifyTokenAsync(string token);
}