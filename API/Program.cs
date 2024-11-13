using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using minimal_api.API.Domain.DTOs;
using minimal_api.API.Domain.Entities;
using minimal_api.API.Domain.Enuns;
using minimal_api.API.Domain.Interfaces;
using minimal_api.API.Domain.ModelViews;
using minimal_api.API.Domain.Services;
using minimal_api.API.Infraestructure.Db;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

#region Builder
var builder = WebApplication.CreateBuilder(args);

var Key = builder.Configuration.GetSection("Jwt").ToString();

builder.Services.AddAuthentication(option =>
{

    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(option =>
{

    option.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key ?? "12345")),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<IAdministradorService, AdministradorService>();
builder.Services.AddScoped<IVeiculoService, VeiculoService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira seu Token: "
    });

    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },

            new string[] {}
        }
    });
});

builder.Services.AddDbContext<Context>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("SQLServer")));
var app = builder.Build();
#endregion

#region Home
app.MapGet("/", () => Results.Json(new Home())).WithTags("Home");
#endregion

#region Administradores

string GerarToken(Administrador adm)
{
    if (string.IsNullOrEmpty(Key)) return string.Empty;

    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var claims = new List<Claim>()
    {
        new Claim("Email", adm.Email),
        new Claim("Profile", adm.Profile),
        new Claim(ClaimTypes.Role, adm.Profile),
    };

    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.Now.AddDays(1),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
};

app.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorService administradorService) =>
{
    var adm = administradorService.Login(loginDTO);

    if (adm != null)
    {
        string token = GerarToken(adm);
        return Results.Ok(new AdministradorLogado
        {
            Email = adm.Email,
            Profile = adm.Profile,
            Token = token
        });
    }
    else
        return Results.Unauthorized();
}).AllowAnonymous().WithTags("Administradores");

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

}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Administradores");

app.MapGet("/administradores", ([FromQuery] int? pagina, IAdministradorService administradorService) =>
{
    var adm = administradorService.Get(pagina).Select(x => new
    {
        Id = x.Id,
        Email = x.Email,
        Profile = x.Profile,
    });

    return Results.Ok(adm);
}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Administradores");

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

}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Administradores");


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
}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" }).WithTags("Veiculos");

app.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoService veiculoService) =>
{
    var veiculos = veiculoService.Get();

    return Results.Ok(veiculos);
}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" }).WithTags("Veiculos");

app.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoService veiculoService) =>
{
    var veiculo = veiculoService.GetById(id);
    if (veiculo == null) return Results.NotFound();

    return Results.Ok(veiculo);
}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm, Editor" }).WithTags("Veiculos");


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

}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Veiculos");

app.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoService veiculoService) =>
{
    var veiculo = veiculoService.GetById(id);

    if (veiculo == null) return Results.NotFound();

    veiculoService.
    Delete(veiculo);

    return Results.NoContent();

}).RequireAuthorization().RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" }).WithTags("Veiculos");
#endregion

#region  APP
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();


app.Run();
#endregion
