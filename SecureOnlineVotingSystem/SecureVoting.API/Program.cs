using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SecureVoting.API.Data;
using SecureVoting.API.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseWebRoot("wwwroot");

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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllClients", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
builder.Services.AddSingleton<TotpService>();
builder.Services.AddScoped<ApiLogRepository>();
builder.Services.AddSingleton<ApiLogCryptoService>();
builder.Services.AddScoped<ApiLogRepository>();
builder.Services.AddScoped<VoteRepository>();
builder.Services.AddScoped<VoterVerificationRepository>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<JurisdictionRepository>();
builder.Services.AddScoped<CandidateRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAllClients");

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<SecureVoting.API.Middleware.ApiLoggingMiddleware>();
app.MapControllers();

app.Run();