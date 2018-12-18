using System.Reflection;
using System.Threading.Tasks;

namespace Minor.Nijn.WebScale.Helpers
{
    internal static class MethodInfoExtensions
    {
        public static async Task<object> InvokeAsync(this MethodInfo method, object instance, object[] parameters)
        {
            dynamic awaiter = method.Invoke(instance, parameters);
            await awaiter;
            return awaiter.GetAwaiter().GetResult();
        }
    }
}