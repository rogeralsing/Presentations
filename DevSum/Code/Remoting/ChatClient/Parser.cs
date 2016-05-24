using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient
{
    public class ParseResult
    {
        public string Command { get; set; }
        public string Text { get; set; }
    }
    public static class Parser
    {
        public static ParseResult Parse(string input)
        {
            if (input.StartsWith("/"))
            {
                var parts = input.Split(' ');
                var cmd = parts[0].ToLowerInvariant();
                var rest = string.Join(" ", parts.Skip(1));
                return new ParseResult()
                {
                    Text = rest,
                    Command = cmd
                };                
            }
            return new ParseResult() {Text = input};
        }
    }
}
