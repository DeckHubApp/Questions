using JetBrains.Annotations;
using MessagePack;
using Slidable.Questions.Data;
using Slidable.Questions.Models;
using StackExchange.Redis;

namespace Slidable.Questions
{
    [PublicAPI]
    public class RedisPublisher
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly ISubscriber _subscriber;

        public RedisPublisher(ConnectionMultiplexer redis)
        {
            _redis = redis;
            if (_redis != null)
            {
                _subscriber = _redis.GetSubscriber();
            }
        }

        public void PublishQuestion(Question question)
        {
            var m = new QuestionMsg
            {
                Show = question.Show,
                From = question.From,
                Text = question.Text,
                Time = question.Time,
                Id = question.Uuid
            };
            _subscriber.Publish("slidable:question", MessagePackSerializer.Serialize(m));
        }
    }
}