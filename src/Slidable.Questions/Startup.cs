using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Slidable.Questions.Data;
using StackExchange.Redis;

namespace Slidable.Questions
{
    [PublicAPI]
    public class Startup
    {
        private static ConnectionMultiplexer _connectionMultiplexer;
        private readonly IHostingEnvironment _env;

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            _env = env;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var redisHost = Configuration.GetSection("Redis").GetValue<string>("Host");
            if (!string.IsNullOrWhiteSpace(redisHost))
            {
                var redisPort = Configuration.GetSection("Redis").GetValue<int>("Port");
                if (redisPort == 0)
                {
                    redisPort = 6379;
                }

                _connectionMultiplexer = ConnectionMultiplexer.Connect($"{redisHost}:{redisPort}");
                services.AddSingleton(_connectionMultiplexer);
            }
            else
            {
                services.AddSingleton<ConnectionMultiplexer>(_ => null);
            }

            if (!_env.IsDevelopment())
            {
                var dpBuilder = services.AddDataProtection().SetApplicationName("slidable");

                if (_connectionMultiplexer != null)
                {
                    dpBuilder.PersistKeysToRedis(_connectionMultiplexer, "DataProtection:Keys");
                }
            }
            else
            {
                services.AddDataProtection()
                    .DisableAutomaticKeyGeneration()
                    .SetApplicationName("slidable");
            }
            services.AddSingleton<RedisPublisher>();

            services.AddDbContextPool<QuestionContext>(b =>
            {
                b.UseNpgsql(Configuration.GetConnectionString("Questions"));
            });

            services.AddMvc();
        }

        private void ConfigureAuth(IServiceCollection services)
        {
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie();
        }


        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            var pathBase = Configuration["Runtime:PathBase"];
            if (!string.IsNullOrEmpty(pathBase))
            {
                app.UsePathBase(pathBase);
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseMvc();
        }
    }
}
