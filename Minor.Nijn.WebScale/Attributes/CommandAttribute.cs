using System;

namespace Minor.Nijn.WebScale.Attributes
{
    /// <summary>
    /// This attribute should decorate each commandhandling method.
    /// The queueName is the name of the RabbitMQ-queue on which it 
    /// will listen to incoming commands.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        public string QueueName { get; set; }

        public CommandAttribute(string queueName)
        {
            QueueName = queueName;
        }
    }
}