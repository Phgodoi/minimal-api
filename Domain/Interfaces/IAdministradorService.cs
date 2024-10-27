﻿using minimal_api.Domain.DTOs;
using minimal_api.Domain.Entities;

namespace minimal_api.Domain.Interfaces
{
    public interface IAdministradorService
    {
        Administrador? Login(LoginDTO loginDTO);
        IQueryable<Administrador> Get(int? pagina);
        void Update(Administrador administrador);

    }
}
