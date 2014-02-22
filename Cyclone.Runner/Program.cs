using System;
using System.Collections.Generic;
using System.Linq;

namespace Cyclone.Runner
{
    /// <summary>
    /// Test program to run actual integration test with the RabbitMQ client.
    /// </summary>
    internal class Program
    {
        private static void Main(string[] args)
        {
            const string exchangeName = "TestExchange";
            const string queueName = "TestQueue";
            const string routingKey = "#";
            int counter = 0;

            IDictionary<string, string> data = Enumerable.Range(100, 110).ToDictionary(x => x.ToString(), y => y.ToString());

            var client = new SharedMessageClient<IDictionary<string, string>>(exchangeName, queueName, routingKey);
            Enumerable.Range(1, 10000).ToList().ForEach(x => client.Publish(data));

            client.AppendSubscription(x =>
                                      {
                                          counter++;
                                          if(counter % 1000 == 0)
                                            Console.WriteLine("Received Data : " + counter.ToString("N0"));
                                      });
            Console.ReadKey();
        }
    }
}