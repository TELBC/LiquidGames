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
        services.AddSingleton<LiquidGamesDatabase>(_ =>
            new LiquidGamesDatabase(
                Configuration.GetValue<string>("DatabaseSettings:ConnectionString"), 
                Configuration.GetValue<string>("DatabaseSettings:DatabaseName"),
                Configuration.GetValue<string>("DatabaseSettings:CollectionName")));
        services.AddControllers();
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseCors();
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthorization();

        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}