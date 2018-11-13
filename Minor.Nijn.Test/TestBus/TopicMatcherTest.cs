using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.TestBus;
using System.Collections.Generic;

namespace Minor.Nijn.Test.TestBus
{
    [TestClass]
    public class TopicMatcherTest
    {
        [TestMethod]
        public void IsMatch_ShouldReturnTrueWhenTopicMatchesExactExpression()
        {
            IEnumerable<string> topicExpressions = new List<string> { "a.b.c", "a.b" };
            Assert.IsTrue(TopicMatcher.IsMatch(topicExpressions, "a.b"), "'a.b' should match 'a.b'");
            Assert.IsTrue(TopicMatcher.IsMatch(topicExpressions, "a.b.c"), "'a.b.c' should match 'a.b.c'");
        }

        [TestMethod]
        public void IsMatch_ShouldReturnFalseWhenTopicNotMatchesExactExpression()
        {
            IEnumerable<string> topicExpressions = new List<string> { "a.b.c" };
            Assert.IsFalse(TopicMatcher.IsMatch(topicExpressions, "a"), "'a' should not match expressions");
            Assert.IsFalse(TopicMatcher.IsMatch(topicExpressions, "a.b"), "'a.b' should not match expressions");
            Assert.IsFalse(TopicMatcher.IsMatch(topicExpressions, "a.d.c"), "'a.d.c' should not match expressions");
        }

        [TestMethod]
        public void IsMatch_ShouldReturnTrueWhenTopicMatchesAsteriskExpression()
        {
            IEnumerable<string> topicExpressions = new List<string> { "a.*.c", "e.f.*", "*.y.z" };
            Assert.IsTrue(TopicMatcher.IsMatch(topicExpressions, "a.b.c"), "'a.b.c' should match 'a.*.c'");
            Assert.IsTrue(TopicMatcher.IsMatch(topicExpressions, "a.d.c"), "'a.d.c' should match 'a.*.c'");
            Assert.IsTrue(TopicMatcher.IsMatch(topicExpressions, "e.f.g"), "'e.f.g' should match 'e.f.*'");
            Assert.IsTrue(TopicMatcher.IsMatch(topicExpressions, "e.f.a"), "'e.f.a' should match 'e.f.*'");
            Assert.IsTrue(TopicMatcher.IsMatch(topicExpressions, "x.y.z"), "'x.y.z' should match '*.y.z'");
            Assert.IsTrue(TopicMatcher.IsMatch(topicExpressions, "q.y.z"), "'q.y.z' should match '*.y.z'");

            topicExpressions = new List<string> { "a.*.*", "*.*.z" };
            Assert.IsTrue(TopicMatcher.IsMatch(topicExpressions, "a.b.c"), "'k.a.b' should match 'a.*.*'");
            Assert.IsTrue(TopicMatcher.IsMatch(topicExpressions, "a.f.g"), "'a.f.g' should match 'a.*.*'");
            Assert.IsTrue(TopicMatcher.IsMatch(topicExpressions, "k.l.z"), "'x.y.z' should match '*.*.z'");
            Assert.IsTrue(TopicMatcher.IsMatch(topicExpressions, "g.h.z"), "'g.h.z' should match '*.*.z'");
        }

        [TestMethod]
        public void IsMatch_ShouldReturnFalseWhenTopicNotMatchesAsteriskExpression()
        {
            IEnumerable<string> topicExpressions = new List<string> { "a.*.c", "e.f.*", "*.y.z" };
            Assert.IsFalse(TopicMatcher.IsMatch(topicExpressions, "a.b.d"), "'a.b.d' should not match expressions");
            Assert.IsFalse(TopicMatcher.IsMatch(topicExpressions, "d.b.d"), "'d.b.d' should not match expressions");
            Assert.IsFalse(TopicMatcher.IsMatch(topicExpressions, "k.f.m"), "'k.f.m' should not match expressions");
            Assert.IsFalse(TopicMatcher.IsMatch(topicExpressions, "e.k.m"), "'e.k.m' should not match expressions");
            Assert.IsFalse(TopicMatcher.IsMatch(topicExpressions, "x.b.z"), "'x.b.z' should not match expressions");
            Assert.IsFalse(TopicMatcher.IsMatch(topicExpressions, "x.y.c"), "'x.y.c' should not match expressions");

            topicExpressions = new List<string> { "a.*.*", "*.*.z" };
            Assert.IsFalse(TopicMatcher.IsMatch(topicExpressions, "b.c.d"), "'b.c.d' should not match expressions");
            Assert.IsFalse(TopicMatcher.IsMatch(topicExpressions, "c.c.d"), "'c.c.d' should not match expressions");
            Assert.IsFalse(TopicMatcher.IsMatch(topicExpressions, "v.w.y"), "'v.w.y' should not match expressions");
            Assert.IsFalse(TopicMatcher.IsMatch(topicExpressions, "v.w.x"), "'v.w.z' should not match expressions");
        }

        [TestMethod]
        public void IsMatch_ShouldReturnTrueWhenTopicMatchesHashTagExpression()
        {
            IEnumerable<string> topicExpressions = new List<string> { "a.#" };
            Assert.IsTrue(TopicMatcher.IsMatch(topicExpressions, "a.b.c"), "'a.b.c' should match 'a.#'");
        }
    }
}
