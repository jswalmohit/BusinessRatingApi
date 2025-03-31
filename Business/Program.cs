using Business.Data;
using Business.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient<GeocodingService>();
var jwtConfig = builder.Configuration.GetSection("Jwt");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
            builder => builder.WithOrigins("https://sasmita2622606.github.io")
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});
// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddCustomServices();

builder.Services.AddDbContext<BusinessContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
//builder.Services.AddDbContext<BusinessContext>(options =>
//    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();

//builder.Configuration.AddJsonFile(@"C:\inetpub\wwwroot\businessapp\appsettings.json", optional: false, reloadOnChange: true);


//authentication configuration
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;  // Set Default Authenticate Scheme
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;  // Set Default Challenge Scheme
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;  // Set Default Challenge Scheme
})
.AddJwtBearer(options =>
 {
     options.RequireHttpsMetadata = false;  // In production, set to true
     options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
     {
         ValidateIssuer = true,
         ValidateAudience = true,
         ValidateLifetime = true,
         ValidIssuer = jwtConfig["Issuer"], // Replace with your JWT issuer
         ValidAudience = jwtConfig["Audience"], // Replace with your JWT audience
         IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtConfig["Key"])) // Replace with your secret key
     };
 });
    builder.Services.AddAuthorization(options => {
        options.AddPolicy("AllowAdminAccessOnly", policy => policy.RequireClaim("RoleID", "1", "2"));
        options.AddPolicy("AllowUserAccessOnly", policy => policy.RequireClaim("RoleID", "1", "2", "3", "4"));
    });

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    // Define the JWT security scheme
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",  // Use Bearer token
        Description = "Enter 'Bearer' followed by a space and then your JWT token"
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
            new string[] {}
        }
    });
});     
var app = builder.Build();

// Serve static files from the 'uploads' directory
app.UseStaticFiles(new StaticFileOptions
{
   
});

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigin");
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
