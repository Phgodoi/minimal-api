﻿using minimal_api.Domain.Entities;

namespace minimal_api.Domain.Interfaces
{
    public interface IVeiculoService
    {
        IQueryable<Veiculo> Get();
        IQueryable<Veiculo> GetBy(int? page = 1, string? nome = null, string? marca = null);
        Veiculo? GetById(int id);
        void Save(Veiculo veiculo);
        void Update(Veiculo veiculo);
        void Delete(Veiculo veiculo);
    }
}