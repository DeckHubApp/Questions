using JetBrains.Annotations;
using MessagePack;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Slidable.Questions.Data;
using Slidable.Questions.Models;
using StackExchange.Redis;

namespace Slidable.Questions
{
    [PublicAPI]
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var redisHost = Configuration.GetSection("Redis").GetValue<string>("Host");
            var redisPort = Configuration.GetSection("Redis").GetValue<int>("Port");
            if (redisPort == 0)
            {
                redisPort = 6379;
            }

            services.AddSingleton(_ => ConnectionMultiplexer.Connect($"{redisHost}:{redisPort}"));
            services.AddSingleton<RedisPublisher>();

            services.AddDbContextPool<QuestionContext>(b =>
            {
                b.UseNpgsql(Configuration.GetConnectionString("Questions"));
            });

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }

    public class RedisPublisher
    {
        private readonly ConnectionMultiplexer _redis;

        public RedisPublisher(ConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public void PublishQuestion(string presenter, string slug, string question, string id, string from)
        {
            var m = new QuestionMsg
            {
                Presenter = presenter,
                Slug = slug,
                From = from,
                Id = id,
                Text = question
            };

            _redis.GetSubscriber().Publish("slidable:question", MessagePackSerializer.Serialize(m));
        }
    }
}
