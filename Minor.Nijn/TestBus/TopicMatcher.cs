using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Minor.Nijn.TestBus
{
    internal static class TopicMatcher
    {
        private const string AsteriskCaptureGroup = @"(?:\w+)";
        private const string HashTagCaptureGroup  = @"(?:\w+\.?)+";

        public static bool IsMatch(IEnumerable<string> topicExpressions, string topic)
        {
            if (topicExpressions.Contains(topic))
            {
                return true;
            }

            return topicExpressions.Any(expr => MatchTopicExpressions(expr, topic));
        }

        private static bool MatchTopicExpressions(string expression, string topic)
        {
            string[] expressionParts = expression.Split('.');
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < expressionParts.Length; i++)
            {
                bool isLast = expressionParts.Length == (i + 1);
                builder.Append(ParseExpressionPart(expressionParts[i], isLast));
            }

            Regex regex = new Regex($"^{builder.ToString()}$");
            return regex.IsMatch(topic.Trim());
        }

        private static string ParseExpressionPart(string expressionPart, bool isLast)
        {
            string result;

            switch (expressionPart.Trim())
            {
                case "*":
                    result = AsteriskCaptureGroup; 
                    break;
                case "#":
                    result = HashTagCaptureGroup;
                    break;
                default:
                    result = $"(?:{expressionPart.Trim()})";
                    break;
            }

            return isLast ? result : result += @"\."; ;
        }
    }
}
