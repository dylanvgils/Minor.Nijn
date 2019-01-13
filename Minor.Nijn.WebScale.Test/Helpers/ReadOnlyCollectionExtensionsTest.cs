using System.Collections.Generic;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.WebScale.Helpers;

namespace Minor.Nijn.WebScale.Test.Helpers
{
    [TestClass]
    public class ReadOnlyCollectionExtensionsTest
    {
        [TestMethod]
        public void ForEach_ShouldLoopOverList()
        {
            List<string> strings = new List<string>();
            strings.Add("one");
            strings.Add("two");

            var result = new List<string>();
            IReadOnlyList<string> target = strings.AsReadOnly();
            target.ForEach(s => result.Add(s));

            Assert.AreEqual(strings[0], result[0]);
            Assert.AreEqual(strings[1], result[1]);
        }
    }
}