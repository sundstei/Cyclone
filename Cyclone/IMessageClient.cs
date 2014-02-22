using System;

namespace Cyclone
{
    public interface IMessageClient<T>
    {
        void AppendSubscription(Action<T> subscription);
        void Publish(T message);
    }
}