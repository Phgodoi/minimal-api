using minimal_api.API.Domain.DTOs;
using minimal_api.API.Domain.Entities;
using minimal_api.API.Domain.Interfaces;
using minimal_api.API.Infraestructure.Db;

namespace minimal_api.API.Domain.Services
{
    public class AdministradorService : IAdministradorService
    {
        private readonly Context _context;
        public AdministradorService(Context context)
        {
            _context = context;
        }

        public IQueryable<Administrador> Get(int? pagina)
        {
            var baseQuery = _context.administradores.AsQueryable();
            int itensPag = 10;

            if (pagina != null)
                baseQuery = baseQuery.Skip(((int)pagina - 1) * itensPag).Take(itensPag);

            return baseQuery;
        }

        public Administrador? Login(LoginDTO loginDTO)
        {
            return _context.administradores
                    .Where(a => a.Email == loginDTO.Email && a.Password == loginDTO.Password)
                        .FirstOrDefault();
        }

        public void Update(Administrador administrador)
        {
            _context.Update(administrador);
            _context.SaveChanges();
        }
    }
}
