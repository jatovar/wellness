using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wellness.Tests
{
    [TestClass]
    public class UnitTest1
    {
        /// <summary>
        /// Be sure of using fluent assertions
        /// </summary>
        [TestMethod]
        [TestCategory("UnitTest")]
        public void TestMethod1()
        {
            1.Should().BeLessThan(1000);
        }
    }
}