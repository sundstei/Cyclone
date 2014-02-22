using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cyclone.Tests
{
    [TestClass]
    public class SharedMessageClientTests
    {
        private string _exchangeName = "TestExchange";
        private string _queueName = "TestQueue";
        private string _routingKey = "#";
        private IDictionary<string, string> _data;

        [TestInitialize]
        public void Setup()
        {
            _data = Enumerable.Range(100, 200).ToDictionary(x => x.ToString(), y => y.ToString());
        }

        [TestMethod]
        public void Should_start_a_message_client()
        {
            var client = new SharedMessageClient<IDictionary<string, string>>(_exchangeName, _queueName, _routingKey);
            Enumerable.Range(1, 1000).ToList().ForEach(x=>client.Publish(_data));
        }
    }
}
