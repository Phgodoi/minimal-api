using minimal_api.API.Domain.DTOs;
using minimal_api.API.Domain.Entities;

namespace minimal_api.API.Domain.Interfaces
{
    public interface IAdministradorService
    {
        Administrador? Login(LoginDTO loginDTO);
        IQueryable<Administrador> Get(int? pagina);
        void Update(Administrador administrador);

    }
}
