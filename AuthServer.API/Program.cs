
using AuthServer.Core.Configuration;
using AuthServer.Core.Dtos;
using AuthServer.Core.Models;
using AuthServer.Core.Repositories;
using AuthServer.Core.Services;
using AuthServer.Core.UnitOfWork;
using AuthServer.Data;
using AuthServer.Data.Repositories;
using AuthServer.Service.Services;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Configurations;
using SharedLibrary.Extensions;
using SharedLibrary.Services;
using System.Reflection;

namespace AuthServer.API
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
			builder.Services.AddScoped<IUserService, UserService>();
			builder.Services.AddScoped<ITokenService, TokenService>();
			builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
			builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
			builder.Services.AddScoped(typeof(IGenericService<,>), typeof(GenericService<,>));

			builder.Services.AddDbContext<AppDbContext>(options =>
			{
				options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"), sqlOptions =>
				{
					sqlOptions.MigrationsAssembly("AuthServer.Data");
				});
			});

			builder.Services.AddIdentity<UserApp, IdentityRole>(Opt =>
			{

				Opt.User.RequireUniqueEmail = true;
				Opt.Password.RequireNonAlphanumeric = false;

			}).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();


			builder.Services.Configure<CustomTokenOption>(builder.Configuration.GetSection("TokenOption"));
			builder.Services.Configure<List<Client>>(builder.Configuration.GetSection("Clients"));


			builder.Services.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

			}).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opts =>
			{
				var tokenOptions = builder.Configuration.GetSection("TokenOption").Get<CustomTokenOption>();
				opts.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
				{
					ValidIssuer = tokenOptions.Issuer,
					ValidAudience = tokenOptions.Audience[0],
					IssuerSigningKey = SignService.GetSymmetricSecurityKey(tokenOptions.SecurityKey),

					ValidateIssuerSigningKey = true,
					ValidateAudience = true,
					ValidateIssuer = true,
					ValidateLifetime = true,
					ClockSkew = TimeSpan.Zero
				};
			});

			// Add services to the container.

			builder.Services.AddControllers().AddFluentValidation(options => options.RegisterValidatorsFromAssembly(Assembly.GetExecutingAssembly()));

			builder.Services.UseCustomValidationResponse();

			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();

			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}
			else
			{
				
			}
			app.UseCustomException();
			app.UseHttpsRedirection();
			app.UseAuthentication();
			app.UseAuthorization();


			app.MapControllers();

			app.Run();
		}
	}
}
