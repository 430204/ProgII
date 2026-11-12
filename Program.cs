using InmoWeb.Models;
using InmoWeb.Repositories.Inmueble;
using InmoWeb.Repositories.Inmueble.Interfaz;
using InmoWeb.Services.Inmuebles;
using InmoWeb.Services.Inmuebles.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "InmoWeb - Inmobiliaria Córdoba",
        Version = "v1",
        Description = "API del Trabajo Práctico Integrador de Bases de Datos I - UTN FRC"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresá el JWT (sin la palabra bearer)."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
        {
            {
                new OpenApiSecurityScheme()
                {
                    Reference = new OpenApiReference()
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                [""]
            }
        });
}
);

builder.Services.AddDbContext<InmobiliariaDBContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Cnn")));
builder.Services.AddScoped<IInmuebleRepository, InmuebleRepository>();
builder.Services.AddScoped<IInmuebleServices, InmuebleServices>();

// Reportes del Trabajo Practico Integrador
builder.Services.AddScoped<
    InmoWeb.Repositories.Reporte.Interfaz.IReporteRepository,
    InmoWeb.Repositories.Reporte.ReporteRepository>();
builder.Services.AddScoped<
    InmoWeb.Services.Reportes.Interfaces.IReporteServices,
    InmoWeb.Services.Reportes.ReporteServices>();

// Operaciones resueltas con los procedimientos almacenados
builder.Services.AddScoped<
    InmoWeb.Repositories.Operacion.Interfaz.IOperacionRepository,
    InmoWeb.Repositories.Operacion.OperacionRepository>();
builder.Services.AddScoped<
    InmoWeb.Services.Operaciones.Interfaces.IOperacionServices,
    InmoWeb.Services.Operaciones.OperacionServices>();

// Repositorio de Personas: lo usa AuthController para validar el login.
// Sin este registro, cualquier POST a /api/Auth falla con error 500 de inyección de dependencias.
builder.Services.AddScoped<
    InmoWeb.Repositories.Persona.Interfaz.IPersonaRepository,
    InmoWeb.Repositories.Persona.PersonaRepository>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "JwtAuthPoc",
            ValidAudience = "JwtAuthPocUser",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("clave-super-secreta-para-poc-123456"))
        };
    });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// La raíz del sitio abre directamente el front, no Swagger.
app.MapGet("/", () => Results.Redirect("/frontend/Indice.html"))
   .ExcludeFromDescription();

app.Run();
