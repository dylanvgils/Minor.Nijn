using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Minor.Nijn
{
    internal static class TopicMatcher
    {
        private const string ValidTopicExpression = @"^(?:(?:\w+|\*|\#)\.)*(?:\w+|\*|\#)$";
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

        /// <summary>
        /// Checks if the provided topics are valid
        /// </summary>
        /// <returns>Returns true when all topics are valid</returns>
        /// <exception cref="BusConfigurationException">This exception is thrown when one of the topic is invalid</exception>
        public static bool AreValidTopicExpressions(IEnumerable<string> topics)
        {
            foreach (var topic in topics)
            {
                IsValidTopic(topic);
            }

            return true;
        }

        private static bool MatchTopicExpressions(string expression, string topic)
        {
            IsValidTopic(expression);

            string[] expressionParts = expression.Split('.');
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < expressionParts.Length; i++)
            {
                string expressionPart = expressionParts[i];
                bool isLast = expressionParts.Length == (i + 1);
                builder.Append(ParseExpressionPart(expressionPart, isLast));
            }

            string pattern = "^" + builder.ToString() + "$";
            Regex regex = new Regex(pattern);
            return regex.IsMatch(topic.Trim());
        }

        private static void IsValidTopic(string topic)
        {
            Regex regex = new Regex(ValidTopicExpression, RegexOptions.Compiled);

            if (regex.IsMatch(topic))
            {
                return;
            }

            throw new BusConfigurationException($"Topic expression '{topic}' is invalid");
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

            return isLast ? result : result + @"\.";
        }
    }
}
