using System;

namespace Minor.Nijn.WebScale.Attributes
{
    /// <summary>
    /// This attribute should decorate each domain listening class.
    /// A MicroserviceHost cannot have two CommandListeners that 
    /// listen to the same queue name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandListenerAttribute : Attribute
    {
        
    }
}
