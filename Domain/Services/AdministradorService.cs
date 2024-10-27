using minimal_api.Domain.DTOs;
using minimal_api.Domain.Entities;
using minimal_api.Domain.Interfaces;
using minimal_api.Infraestructure.Db;

namespace minimal_api.Domain.Services
{
    public class AdministradorService : IAdministradorService
    {
        private readonly Context _context;
        public AdministradorService(Context context) 
        {
            _context = context;
        }
        public Administrador? Login(LoginDTO loginDTO)
        {
            return _context.administradores
                    .Where(a => a.Email == loginDTO.Email && a.Password == loginDTO.Password)
                        .FirstOrDefault();

           
            

        }
    }
}
