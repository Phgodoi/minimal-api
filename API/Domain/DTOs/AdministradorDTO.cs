using minimal_api.API.Domain.Enuns;

namespace minimal_api.API.Domain.DTOs
{
    public class AdministradorDTO
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
        public Profiles? Profile { get; set; } = default!;

    }
}
