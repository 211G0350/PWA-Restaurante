using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PWA_Restaurante.Models.Entities;
using PWA_Restaurante.Models.Validators;
using PWA_Restaurante.Repositories;
using PWA_Restaurante.Services;
using System.Text;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAll", policy =>
	{
		policy.AllowAnyOrigin()
			  .AllowAnyMethod()
			  .AllowAnyHeader();
	});

	options.AddPolicy("Restricted", policy =>
	{
		policy.WithOrigins("https://localhost:7209", "http://localhost:5253")
			  .AllowAnyMethod()
			  .AllowAnyHeader()
			  .AllowCredentials();
	});
});


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, jwtOptions =>
	{
		jwtOptions.Audience = builder.Configuration["Jwt:Audience"];
		jwtOptions.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidIssuer = builder.Configuration["Jwt:Issuer"],
			ValidateAudience = true,
			ValidAudience = builder.Configuration["Jwt:Audience"],
			ValidateIssuerSigningKey = true,
			IssuerSigningKey = new SymmetricSecurityKey(
				Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
			ValidateLifetime = true,
			ClockSkew = TimeSpan.Zero
		};
	});


var connectionString = builder.Configuration.GetConnectionString("Restaurante");
builder.Services.AddDbContext<RestauranteContext>(options =>
	options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddScoped(typeof(Repository<>), typeof(Repository<>));
builder.Services.AddTransient<JwtService>();
builder.Services.AddTransient<UsuarioValidator>();
builder.Services.AddTransient<ProductosValidator>();
builder.Services.AddSignalR();

var app = builder.Build();




if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapHub<PedidosHub>("/pedidosHub");


app.MapGet("/", async context =>
{
	var path = Path.Combine(app.Environment.WebRootPath ?? Path.Combine(app.Environment.ContentRootPath, "wwwroot"), "Home", "index.cshtml");
	if (File.Exists(path))
	{
		await context.Response.SendFileAsync(path);
		return;
	}
	context.Response.Redirect("/index.html");
});


app.MapControllers();

app.Run();