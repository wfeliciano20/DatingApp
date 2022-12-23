using API.Data;
using API.Helpers;
using API.interfaces;
using API.services;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions
{
  public static  class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services,IConfiguration configuration)
        {
            services.AddDbContext<DataContext>(opt =>
            {
            opt.UseSqlite(configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddCors();

            services.AddScoped<ITokenService, TokenService>();

            services.AddScoped<IUserRepository, UserRepository>();

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.Configure<CloudinarySettings>(configuration.GetSection("CloudinarySettings"));

            services.AddScoped<IPhotoServices, PhotoService>();

            services.AddScoped<LogUserActivity>();

            services.AddScoped<ILikesRepository, LikesRepository>();

            return services;
        }
    }
}
