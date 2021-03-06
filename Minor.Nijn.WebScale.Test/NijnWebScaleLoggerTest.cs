﻿using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Minor.Nijn.WebScale.Test
{
    [TestClass]
    public class NijnWebScaleLoggerTest
    {
        [TestMethod]
        public void NijnWebScaleLoggerShouldUseProvidedLoggerFactory()
        {
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            loggerFactoryMock.Setup(fact => fact.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            NijnWebScaleLogger.LoggerFactory = loggerFactoryMock.Object;
            var result = NijnWebScaleLogger.CreateLogger<NijnWebScaleLoggerTest>();

            loggerFactoryMock.VerifyAll();

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ILogger));
        }
    }
}
