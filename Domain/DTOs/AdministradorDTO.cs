using minimal_api.Domain.Enuns;

namespace minimal_api.Domain.DTOs
{
    public class AdministradorDTO
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
        public Profiles? Profile { get; set; } = default!;

    }
}
