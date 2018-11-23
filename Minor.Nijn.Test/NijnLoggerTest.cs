﻿using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Minor.Nijn.Test
{
    [TestClass]
    public class NijnLoggerTest
    {
        [TestCleanup]
        public void AfterEach()
        {
            NijnLogger.LoggerFactory = null;
        }

        [TestMethod]
        public void NijnLoggerShouldReturnCreateLoggerWhenNoLoggerIsProvided()
        {
            var result = NijnLogger.CreateLogger<NijnLoggerTest>();

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ILogger));
        }

        [TestMethod]
        public void NijnLoggerShouldUseProvidedLoggerFactory()
        {
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            loggerFactoryMock.Setup(fact => fact.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            NijnLogger.LoggerFactory = loggerFactoryMock.Object;
            var result = NijnLogger.CreateLogger<NijnLoggerTest>();

            loggerFactoryMock.VerifyAll();

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ILogger));
        }
    }
}
