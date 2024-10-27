﻿using minimal_api.Domain.Entities;
using minimal_api.Domain.Interfaces;
using minimal_api.Infraestructure.Db;
using System.Linq;

namespace minimal_api.Domain.Services
{
    public class VeiculoService : IVeiculoService
    {
        private readonly Context _context;
        public VeiculoService(Context context) 
        {
            _context = context;
        }      

        public IQueryable<Veiculo> Get()
        {
            return _context.veiculos.AsQueryable();
        }

        public IQueryable<Veiculo> GetBy(int? page = 1, string? nome = null, string? marca = null)
        {
            var baseQuery = _context.veiculos.AsQueryable();

            if (String.IsNullOrEmpty(nome)) {baseQuery = baseQuery.Where(x => x.Nome.Contains(nome)); }
            if (String.IsNullOrEmpty(marca)) { baseQuery = baseQuery.Where(x => x.Marca.Contains(marca)); }

            int itensPag = 10;

            if (page != null) 
                baseQuery = baseQuery.Skip(((int)page - 1) * itensPag).Take(itensPag);

            return baseQuery;
        }

        public Veiculo? GetById(int id)
        {
            return _context.veiculos.FirstOrDefault(x => x.Id == id);
        }

        public void Save(Veiculo veiculo)
        {
            _context.veiculos.Add(veiculo);
            _context.SaveChanges();
        }

        public void Update(Veiculo veiculo)
        {
            _context.veiculos.Update(veiculo);
            _context.SaveChanges();
        }
        public void Delete(Veiculo veiculo)
        {
            _context.veiculos.Remove(veiculo);
            _context.SaveChanges();
        }
    }
}
