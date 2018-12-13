using System;
using System.Collections.Generic;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.MessagePatterns;

namespace WebReceiver
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services
                .AddSingleton<IConnectionFactory>(sp =>
                {
                    return new ConnectionFactory()
                    {
                        VirtualHost = "search_condition",
                        UserName = "search_receiver",
                        Password = "1q2w3e4r5t_",
                        AutomaticRecoveryEnabled = true,
                        RequestedHeartbeat = 60
                    };
                })
                .AddSingleton(sp =>
                {
                    var connectionFactory = sp.GetRequiredService<IConnectionFactory>();
                    var endpoints = new List<string> { "srvdocker-t:5672", "srvdocker-t:5673", "srvdocker-t:5674" };
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

            app.Use(sp => {

                var serviceProvider = app.ApplicationServices;
                var connection = serviceProvider.GetRequiredService<IConnection>();
                var channel = connection.CreateModel();

                //// 建立 Exchange 與 Queue
                channel.ExchangeDeclare("Work.Exchange", ExchangeType.Direct, true);
                var queueDeclare = channel.QueueDeclare("Work.Queue", true, false, false, null);

                //// 綁定 Exchange 與 Queue
                channel.QueueBind(queueDeclare, "Work.Exchange", "", null);

                //// 設定 Channel 的 Qos (Quality of Service 服務品質)
                //// prefetchSize: 設定為 0 表示 Channel 訊息數量沒有上限
                //// prefetchCount: 設定為 1 表示 Channel 在同時間只會發送一個訊息給消費端
                channel.BasicQos(0, 1, false);

                //// 建立訂閱，從指定的 Queue 接收訊息
                var queueConsumer = new EventingBasicConsumer(channel);

                queueConsumer.Received += (sender, args) =>
                {
                    var eventData = MessagePackSerializer.Deserialize<int>(args.Body, ContractlessStandardResolver.Instance);

                    new ReceiverProcessor().ShowMessage(eventData);
                };

                channel.BasicConsume(
                    queue: "Work.Queue",
                    autoAck: true,
                    consumer: queueConsumer);

                return sp;
            });

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
