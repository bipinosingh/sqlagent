
using SqlAgent.Data;
using SqlAgent.DTOs;
using SqlAgent.Middleware;
using SqlAgent.Services;
using StackExchange.Redis;

namespace SqlAgent
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddSingleton<DbConnectionFactory>();
            builder.Services.AddSingleton<IConnectionMultiplexer>(
            sp =>
            {
                var configuration = builder.Configuration;
                /*                
                var connectionString =
                    configuration["Redis:ConnectionString"];
                return ConnectionMultiplexer.Connect(connectionString);
                */

                var options = ConfigurationOptions.Parse(configuration["Redis:ConnectionString"]);

                options.AbortOnConnectFail = false;

                return ConnectionMultiplexer.Connect(options);

            });
            builder.Services.AddSingleton<SqlValidator>();
            builder.Services.AddScoped<AIService>();
            builder.Services.AddScoped<SqlExecutorService>();
            builder.Services.AddScoped<SchemaService>();
            builder.Services.AddScoped<QueryResponse>();
            builder.Services.AddScoped<RedisCacheService>();
            builder.Services.AddScoped<AgentService>();
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

            app.UseHttpsRedirection();
            app.UseMiddleware<ExceptionMiddleware>();
            app.UseAuthorization();            
            app.MapControllers();

            app.Run();
        }
    }
}
