using System;

namespace Minor.Nijn.WebScale.Attributes
{
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