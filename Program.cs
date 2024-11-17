using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using real_time_chat_web.Data;
using real_time_chat_web.Models;
using real_time_chat_web.Repository.IRepository;
using real_time_chat_web.Repository;
using real_time_chat_web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text;
using Microsoft.OpenApi.Models;
using real_time_chat_web.Services.IServices;
using System.Collections.Generic;
using real_time_chat_web;
using real_time_chat_web.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// JWT Authentication Setup
var key = builder.Configuration.GetValue<string>("ApiSettings:Secret");

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", builder =>
    {
        builder.WithOrigins("http://localhost:3000") // Only allow specific origin
               .AllowAnyHeader() // Allow all headers
               .AllowAnyMethod() // Allow all methods (GET, POST, PUT, DELETE, etc.)
               .AllowCredentials(); // Allow credentials like cookies or authorization headers
    });
});

// Configure JWT Authentication
builder.Services.AddAuthentication(o =>
{
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

// Add database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
    options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure AutoMapper and Dependency Injection
builder.Services.AddAutoMapper(typeof(MappingConfig));
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddScoped<IRoomsRepository, RoomsRepository>();
builder.Services.AddScoped<IRoomsService, RoomsServices>();
builder.Services.AddScoped<IRoomsUserRepository, RoomsUserRepository>();

// Configure Swagger
builder.Services.AddSwaggerGen(o =>
{
    o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description =
            "JWT Authorization header using the Bearer scheme. \r\n\r\n " +
            "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
            "Example: \"Bearer 12345abcdef\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Scheme = "Bearer"
    });
    o.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
    o.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "real-time-chat-web V1",
        Version = "v1.0",
        Contact = new OpenApiContact
        {
            Email = "trancongquangphu10@gmail.com",
            Name = "Tran Cong Quang Phu",
            Url = new Uri("https://github.com/quangphu1310"),
        }
    });
});

// SignalR setup
builder.Services.AddSignalR();
builder.Services.AddSingleton<IDictionary<string, UserConnection>>(opts => new Dictionary<string, UserConnection>());

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Add middleware for CORS before Authorization
app.UseCors("AllowSpecificOrigin");

app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Map SignalR Hub
app.MapHub<ChatHub>("/chat");

app.Run();
