using System.Collections.Generic;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;

namespace WebPublisher.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private IConnection _connection;

        public ValuesController(IConnection connection)
        {
            this._connection = connection;
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            // 建立 RabbitMQ 連線
            //using (var conn = this._connection)
            //{
                var channel = this._connection.CreateModel();
                channel.BasicQos(0, 8, false);

                //// 建立發行用的 Exchange 和 Queue
                //channel.ExchangeDeclare
                //(
                //    exchange: "Work.Exchange",
                //    type: ExchangeType.Direct,
                //    durable: true,
                //    autoDelete: false
                //);

                //channel.QueueDeclare
                //(
                //    queue: "Work.Queue",
                //    durable: true,
                //    exclusive: false,
                //    autoDelete: false,
                //    arguments: new Dictionary<string, object>()
                //);

                //// Queue 與 Exchange 繫結處理
                //channel.QueueBind
                //(
                //    queue: "Work.Queue",
                //    exchange: "Work.Exchange",
                //    routingKey: "",
                //    arguments: null
                //);

                var properties = channel.CreateBasicProperties();
                //properties.Persistent = true;
                //properties.DeliveryMode = 2;
                properties.Expiration = "60000";
                //properties.Type = ("").GetType().AssemblyQualifiedName;

                //var data = this.SerializeToBytes
                //(
                //    new MessageBusData
                //    {
                //        Type = $"{@event.GetType().FullName}, {@event.GetType().Assembly.GetName().Name}",
                //        Data = this.Serializer.SerializeToBytes(@event)
                //    }
                //);

                channel.BasicPublish
                (
                    exchange: "Work.Exchange",
                    routingKey: "",
                    basicProperties: properties,
                    body: MessagePackSerializer.Serialize(
                        id,
                        ContractlessStandardResolver.Instance)
                );
            //}

            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
