using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using Serilog;
using System.Text;
using System.Text.Json.Serialization;
using User;
using User.CustomAttribute;
using User.Data;
using User.Interface;
using User.Services;
public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddCors(options =>
            options.AddPolicy("AllowReactApp", policy =>
            {
                policy
                    .WithOrigins("https://localhost:3000") //react frontend url
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            })
        );

        builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); //Converts enums → string instead of number
            });


        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(c =>
        {
            c.UseInlineDefinitionsForEnums();

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "Enter JWT token like: Bearer YOUR_TOKEN",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
            {
                Description = "Enter API Key",
                Name = "x-api-key",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey
            });

            // 🔐 Apply BOTH globally
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
                    new string[] {}
                },
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = Constants.ApiKeyName
                        }
                    },
                    new string[] {}
                }
            });
        });


        builder.Services.AddDbContext<UserDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("UserDatabase"))
        );

        builder.Services.AddScoped<IAuthService, AuthService>();

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).
            AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["AppSettings:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["AppSettings:Audience"],
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:Token"]!)),
                    ValidateIssuerSigningKey = true
                };
            });

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .CreateLogger();

        builder.Services.AddTransient<IApiKeyValidation, ApiKeyValidation>();
        builder.Services.AddScoped<ApiKeyAuthFilter>();

        builder.Services.AddHttpContextAccessor();

        var app = builder.Build();


        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();


            // Swagger UI
            app.UseSwaggerUI();

            app.MapScalarApiReference(options =>  //https://localhost:7251/scalar/
            {

                options.AddDocument(
                    "v1",
                    "User API",
                    "/swagger/v1/swagger.json",
                    true
                );
            });
        }

        app.UseHttpsRedirection();

        app.UseCors("AllowReactApp");

        app.UseAuthentication();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}