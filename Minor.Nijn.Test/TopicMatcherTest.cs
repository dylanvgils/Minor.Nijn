using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Minor.Nijn.Test
{
    [TestClass]
    public class TopicMatcherTest
    {
        [TestMethod]
        public void IsMatch_ShouldReturnTrueWhenTopicMatchesExactExpression()
        {
            IEnumerable<string> topicExpressions = new List<string> { "a.b.c", "a.b", "solution.service.event" };
            Assert.IsTrue(TopicMatcher.IsMatch(topicExpressions, "a.b"), "'a.b' should match 'a.b'");
            Assert.IsTrue(TopicMatcher.IsMatch(topicExpressions, "a.b.c"), "'a.b.c' should match 'a.b.c'");
            Assert.IsTrue(TopicMatcher.IsMatch(topicExpressions, "solution.service.event"), "'solution.service.event' should match 'solution.service.event'");
        }

        [TestMethod]
        public void IsMatch_ShouldReturnFalseWhenTopicNotMatchesExactExpression()
        {
            IEnumerable<string> topicExpressions = new List<string> { "a.b.c", "solution.service.event" };
            Assert.IsFalse(TopicMatcher.IsMatch(topicExpressions, "a"), "'a' should not match expressions");
            Assert.IsFalse(TopicMatcher.IsMatch(topicExpressions, "a.b"), "'a.b' should not match expressions");
            Assert.IsFalse(TopicMatcher.IsMatch(topicExpressions, "a.d.c"), "'a.d.c' should not match expressions");
            Assert.IsFalse(TopicMatcher.IsMatch(topicExpressions, "event.service.solution"), "'event.service.solution' should not match expressions");
        }

        [TestMethod]
        public void IsMatch_ShouldReturnTrueWhenTopicMatchesAsteriskExpression()
        {
            IEnumerable<string> topicExpressions = new List<string> { "a.*.c", "e.f.*", "*.y.z", "solution.*.event" };
            Assert.IsTrue(TopicMatcher.IsMatch(topicExpressions, "a.b.c"), "'a.b.c' should match 'a.*.c'");
            Assert.IsTrue(TopicMatcher.IsMatch(topicExpressions, "a.d.c"), "'a.d.c' should match 'a.*.c'");
            Assert.IsTrue(TopicMatcher.IsMatch(topicExpressions, "e.f.g"), "'e.f.g' should match 'e.f.*'");
            Assert.IsTrue(TopicMatcher.IsMatch(topicExpressions, "e.f.a"), "'e.f.a' should match 'e.f.*'");
            Assert.IsTrue(TopicMatcher.IsMatch(topicExpressions, "x.y.z"), "'x.y.z' should match '*.y.z'");
            Assert.IsTrue(TopicMatcher.IsMatch(topicExpressions, "q.y.z"), "'q.y.z' should match '*.y.z'");
            Assert.IsTrue(TopicMatcher.IsMatch(topicExpressions, "solution.*.event"), "'solution.*.event' should match 'solution.*.event'");

            topicExpressions = new List<string> { "*", "*.*", "*.*.*" };
            Assert.IsTrue(TopicMatcher.IsMatch(topicExpressions, "a"), "'a.b.c' should match '*'");
            Assert.IsTrue(TopicMatcher.IsMatch(topicExpressions, "x"), "'a.b.c' should match '*'");
            Assert.IsTrue(TopicMatcher.IsMatch(topicExpressions, "a.b"), "'a.b.c' should match '*.*'");
            Assert.IsTrue(TopicMatcher.IsMatch(topicExpressions, "y.z"), "'a.b.c' should match '*.*'");
            Assert.IsTrue(TopicMatcher.IsMatch(topicExpressions, "a.b.c"), "'a.b.c' should match '*.*.*'");
            Assert.IsTrue(TopicMatcher.IsMatch(topicExpressions, "solution.service.event"), "'solution.service.event' should match '*.*.*'");

            topicExpressions = new List<string> { "a.*.*", "*.*.z", "*.*.event" };
            Assert.IsTrue(TopicMatcher.IsMatch(topicExpressions, "a.b.c"), "'k.a.b' should match 'a.*.*'");
            Assert.IsTrue(TopicMatcher.IsMatch(topicExpressions, "a.f.g"), "'a.f.g' should match 'a.*.*'");
            Assert.IsTrue(TopicMatcher.IsMatch(topicExpressions, "k.l.z"), "'x.y.z' should match '*.*.z'");
            Assert.IsTrue(TopicMatcher.IsMatch(topicExpressions, "g.h.z"), "'g.h.z' should match '*.*.z'");
            Assert.IsTrue(TopicMatcher.IsMatch(topicExpressions, "solution.service.event"), "'solution.service.event' should match '*.*.event'");
        }

        [TestMethod]
        public void IsMatch_ShouldReturnFalseWhenTopicNotMatchesAsteriskExpression()
        {
            IEnumerable<string> topicExpressions = new List<string> { "a.*.c", "e.f.*", "*.y.z", "solution.*.event" };
            Assert.IsFalse(TopicMatcher.IsMatch(topicExpressions, "a.b.d"), "'a.b.d' should not match expressions");
            Assert.IsFalse(TopicMatcher.IsMatch(topicExpressions, "d.b.d"), "'d.b.d' should not match expressions");
            Assert.IsFalse(TopicMatcher.IsMatch(topicExpressions, "k.f.m"), "'k.f.m' should not match expressions");
            Assert.IsFalse(TopicMatcher.IsMatch(topicExpressions, "e.k.m"), "'e.k.m' should not match expressions");
            Assert.IsFalse(TopicMatcher.IsMatch(topicExpressions, "x.b.z"), "'x.b.z' should not match expressions");
            Assert.IsFalse(TopicMatcher.IsMatch(topicExpressions, "x.y.c"), "'x.y.c' should not match expressions");
            Assert.IsFalse(TopicMatcher.IsMatch(topicExpressions, "event.service.solution"), "'event.service.solution' should not match expressions");

            topicExpressions = new List<string> { "*" };
            Assert.IsFalse(TopicMatcher.IsMatch(topicExpressions, "a.b"), "'a.b' should not match expressions");
            Assert.IsFalse(TopicMatcher.IsMatch(topicExpressions, "a.b.c"), "'a.b.c' should not match expressions");

            topicExpressions = new List<string> { "*.*" };
            Assert.IsFalse(TopicMatcher.IsMatch(topicExpressions, "a"), "'a' should not match expressions");
            Assert.IsFalse(TopicMatcher.IsMatch(topicExpressions, "a.b.c"), "'a.b.c' should not match expressions");

            topicExpressions = new List<string> { "a.*.*", "*.*.z", "*.*.event" };
            Assert.IsFalse(TopicMatcher.IsMatch(topicExpressions, "b.c.d"), "'b.c.d' should not match expressions");
            Assert.IsFalse(TopicMatcher.IsMatch(topicExpressions, "c.c.d"), "'c.c.d' should not match expressions");
            Assert.IsFalse(TopicMatcher.IsMatch(topicExpressions, "v.w.y"), "'v.w.y' should not match expressions");
            Assert.IsFalse(TopicMatcher.IsMatch(topicExpressions, "v.w.x"), "'v.w.z' should not match expressions");
            Assert.IsFalse(TopicMatcher.IsMatch(topicExpressions, "event.service.solution"), "'event.service.solution' should not match expressions");
        }

        [TestMethod]
        public void IsMatch_ShouldReturnTrueWhenTopicMatchesHashtagExpression()
        {
            IEnumerable<string> topicExpressions = new List<string> { "a.#", "solution.#" };
            Assert.IsTrue(TopicMatcher.IsMatch(topicExpressions, "a.b.c"), "'a.b.c' should match 'a.#'");
            Assert.IsTrue(TopicMatcher.IsMatch(topicExpressions, "a.b.c"), "'a.b.c' should match 'a.#'");
            Assert.IsTrue(TopicMatcher.IsMatch(topicExpressions, "solution.service.event"), "'solution.service.event' should match 'solution.#'");

            topicExpressions = new List<string> { "#" };
            Assert.IsTrue(TopicMatcher.IsMatch(topicExpressions, "a"), "'a'  should match '#'");
            Assert.IsTrue(TopicMatcher.IsMatch(topicExpressions, "a.b"), "'a.b'  should match '#'");
            Assert.IsTrue(TopicMatcher.IsMatch(topicExpressions, "a.b.c"), "'a.b.c'  should match '#'");
            Assert.IsTrue(TopicMatcher.IsMatch(topicExpressions, "solution.service.event"), "'solution.service.event'  should match '#'");
        }

        [TestMethod]
        public void IsMatch_ShouldReturnFalseWhenTopicNotMatchesHashtagExpression()
        {
            IEnumerable<string> topicExpressions = new List<string> { "a.#", "solution.#" };
            Assert.IsFalse(TopicMatcher.IsMatch(topicExpressions, "c.b.a"), "'a.b.c' should not match expressions");
            Assert.IsFalse(TopicMatcher.IsMatch(topicExpressions, "event.service.solution"), "'a.b.c' should not match expressions");
        }

        [TestMethod]
        public void IsMatch_ShouldReturnTrueWhenTopicMatchesHashtagAndAsteriskCombinedExpression()
        {
            IEnumerable<string> topicExpressions = new List<string> { "u.#.x.*.z", "solution.#.service.*.event" };
            Assert.IsTrue(TopicMatcher.IsMatch(topicExpressions, "u.v.w.x.y.z"), "'a.b.c' should match 'u.#.x.*.z'");
            Assert.IsTrue(TopicMatcher.IsMatch(topicExpressions, "solution.sub.sub.service.sub.event"), "'solution.sub.sub.service.sub.event' should match 'solution.#.service.*.event'");
        }

        [TestMethod]
        public void IsMatch_ShouldReturnFalseWhenTopicNotMatchesHashtagAndAsteriskCombinedExpression()
        {
            IEnumerable<string> topicExpressions = new List<string> { "u.#.x.*.z", "solution.#.service.*.event" };
            Assert.IsFalse(TopicMatcher.IsMatch(topicExpressions, "z.y.w.v.u"), "'a.b.c' should not match expressions");
            Assert.IsFalse(TopicMatcher.IsMatch(topicExpressions, "event.sub.service.sub.sub.solution"), "'solution.sub.sub.service.sub.event' should not match expressions");
        }

        [TestMethod]
        public void AreValidTopicExpressions_ShouldReturnTrueWhenTopicIsInValidFormat()
        {
            var expressions = new List<string> { "a.b.c", "a.*.*.c", "a.*.c.*.d", "a.#.d", "a.#.d.*" };
            Assert.IsTrue(TopicMatcher.AreValidTopicExpressions(expressions));
        }

        [TestMethod]
        public void AreValidTopicExpressions_ShouldThrowInvalidTopicExceptionWhenTopicIsInvalid_DoubleAsterisk()
        {
            string expression = "solution.#.**.event";
            var expressions = new List<string> { expression };

            Action action = () =>
            {
                TopicMatcher.AreValidTopicExpressions(expressions);
            };

            var ex = Assert.ThrowsException<BusConfigurationException>(action);
            Assert.AreEqual(ex.Message, $"Topic expression '{expression}' is invalid");
        }

        [TestMethod]
        public void AreValidTopicExpressions_ShouldThrowInvalidTopicExceptionWhenTopicIsInvalid_DoubleHashtag()
        {
            string expression = "solution.##.*.event";
            var expressions = new List<string> { expression };

            Action action = () =>
            {
                TopicMatcher.AreValidTopicExpressions(expressions);
            };

            var ex = Assert.ThrowsException<BusConfigurationException>(action);
            Assert.AreEqual(ex.Message, $"Topic expression '{expression}' is invalid");
        }
    }
}
