using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PapeleriaApi.Models;
using PapeleriaApi.Services;
using System.Text;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<FirebaseService>();

/*
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "tu-issuer",  
            ValidAudience = "tu-audience",  
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("tu-clave-secreta"))  // Usar la misma clave secreta que al generar el JWT
        };
    });

// Agregar política para permitir acceso anónimo a create_preference
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AllowMercadoPago", policy =>
    {
        policy.RequireAssertion(context =>
            context.Resource is Microsoft.AspNetCore.Http.HttpContext httpContext &&
            httpContext.Request.Path.StartsWithSegments("/api/mercadopago/create_preference")
            || context.User.Identity != null && context.User.Identity.IsAuthenticated
        );
    });
});
*/

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", builder =>
    {
        builder.WithOrigins("http://localhost:4200")
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Papeleria API", Version = "v1" });
});

var app = builder.Build();



if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseCors("AllowFrontend");

app.UseHttpsRedirection();

app.UseWhen(context => !context.Request.Path.StartsWithSegments("/api/mercadopago/create_preference"), appBuilder =>
{
    appBuilder.UseAuthentication();
    appBuilder.UseAuthorization();
});

app.UseAuthorization();

app.MapControllers();

app.Run();
