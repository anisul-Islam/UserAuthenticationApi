using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using UserAuthenticationWebApi2.Data;
using UserAuthenticationWebApi2.Services;


var builder = WebApplication.CreateBuilder(args);

DotNetEnv.Env.Load();


builder.Services.AddControllers();
builder.Services.AddScoped<AuthService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// for jwt
// Add services to the DI container.
var Configuration = builder.Configuration; // Ensure this is accessible

var jwtKey = Environment.GetEnvironmentVariable("JWT__KEY") ?? throw new InvalidOperationException("JWT Key is missing in configuration.");
var jwtIssuer = Environment.GetEnvironmentVariable("JWT__ISSUER") ?? throw new InvalidOperationException("JWT Issure is missing in configuration.");
var jwtAudience = Environment.GetEnvironmentVariable("JWT__AUDIENCE") ?? throw new InvalidOperationException("JWT Audience is missing in configuration.");
var databaseConnectionUrl = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");


builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(databaseConnectionUrl));


var key = Encoding.ASCII.GetBytes(jwtKey);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", builder =>
    {
        builder.WithOrigins("http://localhost:5173")
        .AllowAnyHeader()
        .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// /products 
app.Use(async (context, next) =>
{
    var clientIp = context.Connection.RemoteIpAddress?.ToString();
    var stopwatch = Stopwatch.StartNew();
    Console.WriteLine($"[{DateTime.UtcNow}] [Request] " +
                      $"{context.Request.Method} {context.Request.Path}{context.Request.QueryString} " +
                      $"from {clientIp}");

    await next.Invoke();
    stopwatch.Stop();
    Console.WriteLine($"Time Taken: {stopwatch.ElapsedMilliseconds}");
});


app.UseCors("AllowSpecificOrigins");

app.MapGet("/", () =>
{
    return new { Message = "hello" };
});

app.MapGet("/products", () =>
{
    return new { Products = "products are here" };
});



app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();


