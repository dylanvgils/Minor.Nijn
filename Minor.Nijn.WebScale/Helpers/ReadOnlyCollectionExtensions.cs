using System;
using System.Collections.Generic;

namespace Minor.Nijn.WebScale.Helpers
{
    public static class ReadOnlyCollectionExtensions
    {
        public static void ForEach<T>(this IReadOnlyCollection<T> items, Action<T> action)
        {
            foreach (var item in items)
            {
                action(item);
            }
        }    
    }
}