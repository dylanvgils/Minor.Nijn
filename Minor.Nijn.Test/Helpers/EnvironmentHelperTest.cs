using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.Helpers;

namespace Minor.Nijn.Test.Helpers
{
    [TestClass]
    public class EnvironmentHelperTest
    {
        private string envKey = "TEST_ENV_VARIABLE";

        [TestCleanup]
        public void AfterEach()
        {
            Environment.SetEnvironmentVariable(envKey, null);
        }

        [TestMethod]
        public void GetValue_ShouldReturnValueForEnvironmentVariable()
        {

            var value = "SomeValue";
            Environment.SetEnvironmentVariable(envKey, value);

            string result = null;
            EnvironmentHelper.GetValue(envKey, v =>  result = v);

            Assert.AreEqual(value, result);
        }

        [TestMethod]
        public void GetValue_ShouldReturnDefaultValueWhenEnvironemntVariableIsNotSet()
        {
            var defaultValue = "SomeDefaultValue";

            string result = null;
            EnvironmentHelper.GetValue(envKey, v => result = v, defaultValue);

            Assert.AreEqual(defaultValue, result);
        }

        [TestMethod]
        public void GetValue_ShouldThrowArgumentExceptionWhenNotSet()
        {
            string result = null;
            Action action = () => { EnvironmentHelper.GetValue(envKey, v => result = v); };

            var ex = Assert.ThrowsException<ArgumentException>(action);
            Assert.AreEqual($"Invalid environment variable with key: {envKey}", ex.Message);
        }
    }
}
