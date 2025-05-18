using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using ApiChamada.Data;
using ApiChamada.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Obter a connection string do arquivo de configura��o
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
}

// Configura��o do banco de dados MySQL para TicketContext
builder.Services.AddDbContext<TicketContext>(options =>
    options.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(8, 0, 21)),
        mysqlOptions => mysqlOptions.EnableRetryOnFailure()
    )
);

// Configura��o do banco de dados MySQL para AppDbContext (tabelas User e RefreshTokens)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(8, 0, 21)),
        mysqlOptions => mysqlOptions.EnableRetryOnFailure()
    )
);

// Registra os servi�os com os escopos corretos
builder.Services.AddScoped<TicketDao>();
builder.Services.AddScoped<UserDao>();
builder.Services.AddScoped<QRCodeService>();

// Registra o servi�o de background como singleton
builder.Services.AddHostedService<QRCodeScannerService>();

// Configurar autentica��o JWT
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 32)
{
    throw new InvalidOperationException("JWT Key must be at least 32 characters long.");
}
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// Configurar CORS para o frontend Vite
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVite", builder =>
    {
        builder.WithOrigins("http://localhost:5173")
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials();
    });
});

// Adicionar controllers
builder.Services.AddControllers();

// Adicionar Swagger para documenta��o
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configurar pipeline de requisi��es HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Ativar HTTPS apenas em produ��o
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowVite");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();