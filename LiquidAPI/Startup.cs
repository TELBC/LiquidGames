using LiquidAPI.Database.Relational;
using Microsoft.EntityFrameworkCore;

namespace LiquidAPI;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        // MongoDB
        services.AddSingleton<LiquidGamesDatabase>(_ =>
            new LiquidGamesDatabase(
                Configuration.GetValue<string>("DatabaseSettings:ConnectionString"), 
                Configuration.GetValue<string>("DatabaseSettings:DatabaseName"),
                Configuration.GetValue<string>("DatabaseSettings:CollectionName")));
        
        // Postgres
        services.AddDbContext<LiquidGamesContext>(options =>
            options.UseNpgsql(Configuration.GetConnectionString("PostgresConnection")));

        services.AddCors(options =>
            {
                options.AddPolicy("MyPolicy",
                    builder => builder.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod());
            }); 
        services.AddControllers();
    }

    public void Configure(IApplicationBuilder  app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseCors("MyPolicy");
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthorization();

        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}