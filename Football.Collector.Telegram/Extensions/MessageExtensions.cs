using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types;

namespace Football.Collector.Telegram.Extensions
{
    public static class MessageExtensions
    {
        public static IDictionary<string, string> ParseMessage(this Message message, string command)
        {
            var text = message.Text.Substring((command + " ").Length);
            var args = ListToDictionary(SplitCommandLine(text));
            return args;
        }
        private static Dictionary<T, T> ListToDictionary<T>(IEnumerable<T> a)
        {
            var keys = a.Where((s, i) => i % 2 == 0);
            var values = a.Where((s, i) => i % 2 == 1);
            return keys
                .Zip(values, (k, v) => new KeyValuePair<T, T>(k, v))
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        }
        private static IEnumerable<string> SplitCommandLine(string commandLine)
        {
            bool inQuotes = false;

            return commandLine.Split(c =>
            {
                if (c == '\"')
                {
                    inQuotes = !inQuotes;
                }

                return !inQuotes && c == ' ';
            })
                .Select(arg => arg.Trim().TrimMatchingQuotes('\"'))
                .Where(arg => !string.IsNullOrEmpty(arg));
        }
        private static IEnumerable<string> Split(this string str, Func<char, bool> controller)
        {
            int nextPiece = 0;

            for (int c = 0; c < str.Length; c++)
            {
                if (controller(str[c]))
                {
                    yield return str.Substring(nextPiece, c - nextPiece);
                    nextPiece = c + 1;
                }
            }

            yield return str.Substring(nextPiece);
        }
        private static string TrimMatchingQuotes(this string input, char quote)
        {
            if ((input.Length >= 2) &&
                (input[0] == quote) && (input[input.Length - 1] == quote))
            {
                return input.Substring(1, input.Length - 2);
            }

            return input;
        }
    }
}
