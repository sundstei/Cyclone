using System;
using System.Collections.Generic;
using System.Linq;

namespace Cyclone.Runner
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            const string exchangeName = "TestExchange";
            const string queueName = "TestQueue";
            const string routingKey = "#";
            int counter = 0;

            IDictionary<string, string> data = Enumerable.Range(100, 200).ToDictionary(x => x.ToString(), y => y.ToString());

            var client = new SharedMessageClient<IDictionary<string, string>>(exchangeName, queueName, routingKey);
            Enumerable.Range(1, 100000).ToList().ForEach(x => client.Publish(data));

            client.AppendSubscription(x =>
                                      {
                                          counter++;
                                          Console.WriteLine("Got a dictionary. " + counter);
                                      });
            Console.ReadKey();
        }
    }
}