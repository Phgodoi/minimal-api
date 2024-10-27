using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using minimal_api.Domain.DTOs;
using minimal_api.Domain.Entities;
using minimal_api.Domain.Enuns;
using minimal_api.Domain.Interfaces;
using minimal_api.Domain.ModelViews;
using minimal_api.Domain.Services;
using minimal_api.Infraestructure.Db;

#region Builder
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAdministradorService, AdministradorService>();
builder.Services.AddScoped<IVeiculoService, VeiculoService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<Context>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("SQLServer")));
var app = builder.Build();
#endregion

#region Home
app.MapGet("/", () => Results.Json(new Home())).WithTags("Home");
#endregion

#region Administradores
app.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorService administradorService) =>
{
    if (administradorService.Login(loginDTO) != null)
        return Results.Ok("Login Realizado com secosso!");
    else
        return Results.Unauthorized();
}).WithTags("Administradores");

app.MapPost("/administradores", ([FromBody] AdministradorDTO administradorDTO, IAdministradorService administradorService) =>
{
    var validacao = new ErrosDeValidacao { Menssagens = new List<string>() };

    if (string.IsNullOrEmpty(administradorDTO.Email)) validacao.Menssagens.Add("Email não poder ser vázio!");
    if (string.IsNullOrEmpty(administradorDTO.Password)) validacao.Menssagens.Add("Senha não poder ser vázia!");
    if (administradorDTO.Profile == null) validacao.Menssagens.Add("Perfil não poder ser vázia!");

    var adm = new Administrador
    {
        Email = administradorDTO.Email,
        Password = administradorDTO.Password,
        Profile = administradorDTO.Profile.ToString() ?? Profiles.Editor.ToString(),
    };

    administradorService.Update(adm);
    
    var admView = new
    {
        Id = adm.Id,
        Email = adm.Email,
        Profile = adm.Profile,
    };

    return Results.Created($"/administradores/{admView.Id}", admView);

}).WithTags("Administradores");

app.MapGet("/administradores", ([FromQuery] int? pagina, IAdministradorService administradorService) =>
{
    var adm = administradorService.Get(pagina).Select(x => new
    {
        Id = x.Id,
        Email = x.Email,
        Profile = x.Profile,
    });   

    return Results.Ok(adm);
}).WithTags("Administradores");

app.MapGet("/administradores/{id}", ([FromQuery] int? pagina, int id, IAdministradorService administradorService) =>
{
    var admById = administradorService.Get(pagina).Where(x => x.Id == id).Select(x => new
    {
        Id = x.Id,
        Email = x.Email,
        Profile = x.Profile
    }).FirstOrDefault();

    if (admById != null)
        return Results.Ok(admById);
    else 
        return Results.NotFound();

}).WithTags("Administradores");


#endregion

#region veiculos
ErrosDeValidacao Validar(VeiculoDTO veiculoDTO)
{
    var validacao = new ErrosDeValidacao { Menssagens = new List<string>() };

    if (string.IsNullOrEmpty(veiculoDTO.Nome)) validacao.Menssagens.Add("O Nome não pode ser vázio!");
    if (string.IsNullOrEmpty(veiculoDTO.Marca)) validacao.Menssagens.Add("A Marca não pode ser vázia!");
    if (veiculoDTO.Ano < 1950 || veiculoDTO.Ano > DateTime.Now.Year) validacao.Menssagens.Add($"Ano inválido, o interválo aceito é de 1950 até {DateTime.Now.Year}");

    return validacao;
}

app.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoService veiculoService) =>
{
    var validacao = Validar(veiculoDTO);
    if (validacao.Menssagens.Count() > 0) return Results.BadRequest(validacao);

    var veiculo = new Veiculo()
    {
        Nome = veiculoDTO.Nome,
        Marca = veiculoDTO.Marca,
        Ano = veiculoDTO.Ano,
    };

    veiculoService.Save(veiculo);

    return Results.Created($"/veiculo/{veiculo.Id}", veiculo);
}).WithTags("Veiculos");

app.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoService veiculoService) =>
{
    var veiculos = veiculoService.Get();

    return Results.Ok(veiculos);
}).WithTags("Veiculos");

app.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoService veiculoService) =>
{
    var veiculo = veiculoService.GetById(id);
    if (veiculo == null) return Results.NotFound();

    return Results.Ok(veiculo);
}).WithTags("Veiculos");


app.MapPut("/veiculos/{id}", ([FromRoute] int id, VeiculoDTO veiculoDTO, IVeiculoService veiculoService) =>
{
    var veiculo = veiculoService.GetById(id);
    if (veiculo == null) return Results.NotFound();

    var validacao = Validar(veiculoDTO);
    if (validacao.Menssagens.Count() > 0) return Results.BadRequest(validacao);

    veiculo.Nome = veiculoDTO.Nome;
    veiculo.Marca = veiculoDTO.Marca;
    veiculo.Ano = veiculoDTO.Ano;

    veiculoService.Update(veiculo);

    return Results.Ok(veiculo);

}).WithTags("Veiculos");

app.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoService veiculoService) =>
{
    var veiculo = veiculoService.GetById(id);

    if (veiculo == null) return Results.NotFound();

    veiculoService.
    Delete(veiculo);

    return Results.NoContent();

}).WithTags("Veiculos");
#endregion

#region  APP
app.UseSwagger();
app.UseSwaggerUI();

app.Run();
#endregion
