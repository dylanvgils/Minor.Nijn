using System;

namespace Minor.Nijn.WebScale.Attributes
{
    /// <summary>
    /// This attribute should decorate each eventhandling method.
    /// All events matching the topicExpression will be handled by this method. 
    /// (the event wil possibly also handled by other methods with a matching 
    /// topicExpression)
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class TopicAttribute : Attribute
    {
        public string TopicExpression { get; set; }

        public TopicAttribute(string topicExpression)
        {
            TopicExpression = topicExpression;
        }
    }
}
