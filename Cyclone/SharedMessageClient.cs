using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using Retlang.Channels;
using Retlang.Fibers;

namespace Cyclone
{
    public class SharedMessageClient<T> : IMessageClient<T>
    {
        #region Parameters

        private readonly string _exchangeName;
        private readonly string _queueName;
        private readonly string _routingKey; 
        
        #endregion


        #region Channels

        private readonly IChannel<T> _toMessageBus = new Channel<T>();
        private readonly IChannel<T> _fromMessageBus = new Channel<T>();
        private readonly IFiber _fiber;
        private IModel _channel;
        private PublicationAddress _publicationAddress;
        private IBasicProperties _props;

        #endregion

        public SharedMessageClient(string exchangeName, string queueName, string routingKey)
        {
            _exchangeName = exchangeName;
            _queueName = queueName;
            _routingKey = routingKey;

            _fiber = new PoolFiber();
            _fiber.Start();

            SetupSubscription();
        }

        public void AppendSubscription(Action<T> subscription)
        {
            _fromMessageBus.Subscribe(_fiber, subscription);
        }

        public void Publish(T message)
        {
            _toMessageBus.Publish(message);
        }

        private void SetupSubscription()
        {
            _publicationAddress = new PublicationAddress("topic", _exchangeName, _routingKey);
            _toMessageBus.Subscribe(_fiber, PublishMessageToMessageBus );
            
            // TODO: SES / Add config of these parameters.
            var factory = new ConnectionFactory
                          {
                              UserName = "guest", 
                              Password = "guest", 
                              VirtualHost = "/", 
                              Protocol = Protocols.FromEnvironment(), 
                              HostName = "localhost", 
                              Port = AmqpTcpEndpoint.UseDefaultPort
                          };

            var conn = factory.CreateConnection();
            _channel = conn.CreateModel();

            _channel.ExchangeDeclare(_exchangeName, ExchangeType.Topic, true, false, new Dictionary<string, object>());
            _channel.QueueDeclare(_queueName, true, false, false, null);
            _channel.QueueBind(_queueName, _exchangeName, _routingKey, null);

            var consumer = new QueueingBasicConsumer(_channel);
            String consumerTag = _channel.BasicConsume(_queueName, false, consumer);
            _props = _channel.CreateBasicProperties();
            Task.Factory.StartNew(() => ReceiveMessages(consumer, _channel)).ContinueWith(x => Console.WriteLine(GetType() + " : Error shutting down Feed endpoint."));
        }

        private void PublishMessageToMessageBus(T message)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
            _channel.BasicPublish(_publicationAddress, _props, messageBytes);
        }

        private void ReceiveMessages(QueueingBasicConsumer consumer, IModel channel)
        {
            while (true)
            {
                try
                {
                    BasicDeliverEventArgs e = consumer.Queue.Dequeue();
                    IBasicProperties props = e.BasicProperties;
                    byte[] body = e.Body;
                    var message = JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(body));

                    PublishMessage(message);

                    channel.BasicAck(e.DeliveryTag, false);
                }
                catch (OperationInterruptedException ex)
                {
                    throw new Exception("Error reading from Message Queue.", ex);
                }
            }
        }
        private void PublishMessage(T message)
        {
            _fromMessageBus.Publish(message);
        }
    }
}
