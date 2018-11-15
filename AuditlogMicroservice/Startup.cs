using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AuditlogMicroservice.DAL;
using AuditlogMicroservice.Entities;
using AuditlogMicroservice.EventListeners;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minor.Nijn;
using Minor.Nijn.RabbitMQBus;

namespace AuditlogMicroservice
{
    public class Startup
    {
        private IRabbitMQBusContext rabbitMqContext;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            string connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
            var dBOptions = new DbContextOptionsBuilder<EventContext>()
                .UseSqlServer(connectionString)
                .Options;

            using (var context = new EventContext(dBOptions))
            {
                try
                {
                    context.Database.EnsureCreated();
                }
                catch (Exception)
                {
                    Console.WriteLine("Database not found, trying again in 5 seconds...");
                    Thread.Sleep(5000);
                    context.Database.EnsureCreated();
                }

            }

            services.AddSingleton(dBOptions);
            services.AddTransient<IDataMapper<Event, long>, EventDataMapper>();

            var connectionBuilder = new RabbitMQContextBuilder()
                    .WithExchange("RABBITMQ_EXCHANGE_NAME")
                    .WithAddress("RABBITMQ_HOST", 5672)
                    .WithCredentials(userName: "RABBITMQ_USER", password: "RABBITMQ_PASSWORD")
                    .WithType("RABBITMQ_TYPE")
                    .ReadFromEnvironmentVariables();

            MessageEventListener handler = new MessageEventListener(new EventDataMapper(dBOptions));

            rabbitMqContext = connectionBuilder.CreateContext();

            IEnumerable<string> topic = new List<string> { "#" };

            var receiver = rabbitMqContext.CreateMessageReceiver("AuditlogQueue", topic);
            receiver.DeclareQueue();
            receiver.StartReceivingMessages(handler.HandleEvent);
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime applicationLifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            applicationLifetime.ApplicationStopping.Register(OnShutdown);

            app.UseMvc();
        }

        private void OnShutdown()
        {
            rabbitMqContext.Dispose();
        }
    }
}
