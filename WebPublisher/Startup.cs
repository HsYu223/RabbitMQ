using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace WebPublisher
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services
                .AddSingleton<IConnectionFactory>(sp =>
                {
                    return new ConnectionFactory
                    {
                        VirtualHost = "search_condition",
                        UserName = "search_publisher",
                        Password = "1q2w3e4r5t_",
                        AutomaticRecoveryEnabled = true,
                        RequestedHeartbeat = 60
                    };
                })
                .AddSingleton(sp =>
                {
                    var connectionFactory = sp.GetRequiredService<IConnectionFactory>();
                    var endpoints = new List<string>() { "srvdocker-t:5672", "srvdocker-t:5673", "srvdocker-t:5674" };
                    var amqpTcpEndpoints = new List<AmqpTcpEndpoint>();
                    foreach (var endpoint in endpoints)
                    {
                        var url = endpoint.Split(':');
                        var hostName = url[0];
                        var portParse = int.TryParse(url[1], out var port);
                        amqpTcpEndpoints.Add(new AmqpTcpEndpoint(hostName, portParse ? port : 5672));
                    }

                    return connectionFactory.CreateConnection(amqpTcpEndpoints);
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
