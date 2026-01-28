using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SecureVoting.API.Data;
using SecureVoting.API.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var jwtSection = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSection["Key"]!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSingleton<DbHelper>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<ElectionRepository>();
builder.Services.AddScoped<VoteLedgerRepository>();
builder.Services.AddSingleton<CryptoService>();
builder.Services.AddScoped<MfaRepository>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddSingleton<VoteCryptoService>();
builder.Services.AddScoped<VoterElectionStatusRepository>();
builder.Services.AddScoped<VoteLedgerRepository>();


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthentication();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();



app.Run();
