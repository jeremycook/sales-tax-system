using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiteKit.Text
{
    public class Tokenizer
    {
        public Tokenizer(string term)
        {
            Term = term;
            Tokenize();
            for (int i = 0; i < Tokens.Count; i++)
            {
                string token = Tokens[i];

                if (decimal.TryParse(token.TrimStart('$'), out decimal number))
                {
                    if (Tokens.Count > i + 2 &&
                        Tokens[i + 1] == "-" &&
                        decimal.TryParse(Tokens[i + 2].TrimStart('$'), out decimal ends))
                    {
                        NumberRanges.Add(new Tuple<decimal, decimal>(number, ends));
                        i += 2;
                        continue;
                    }
                    else
                    {
                        Numbers.Add(number);
                        continue;
                    }
                }

                if (DateTime.TryParse(token, out DateTime dateTime))
                {
                    if (Tokens.Count > i + 2 &&
                        Tokens[i + 1] == "-" &&
                        DateTime.TryParse(Tokens[i + 2], out DateTime ends))
                    {
                        DateRanges.Add(new Tuple<DateTime, DateTime>(dateTime, ends));
                        i += 2;
                        continue;
                    }
                    else
                    {
                        Dates.Add(dateTime);
                        continue;
                    }
                }
            }
        }

        public string Term { get; }
        public List<string> Tokens { get; } = new List<string>();
        public List<decimal> Numbers { get; } = new List<decimal>();
        public List<Tuple<decimal, decimal>> NumberRanges { get; } = new List<Tuple<decimal, decimal>>();
        public List<DateTime> Dates { get; } = new List<DateTime>();
        public List<Tuple<DateTime, DateTime>> DateRanges { get; } = new List<Tuple<DateTime, DateTime>>();
        public bool Any => Tokens.Count > 0;

        // TODO: Dates and/or DateRanges
        //public List<DateTime> Dates { get; private set; } = null!;
        //public List<Tuple<DateTime, DateTime>> DateRanges { get; private set; } = null!;

        private void Tokenize()
        {
            if (string.IsNullOrWhiteSpace(Term))
            {
                return;
            }

            var groupToken = new[] { '"', '\'' };

            bool inGroup = false;
            char lastChar = ' ';
            char lastGroupToken = ' ';
            var token = new StringBuilder();
            foreach (char ch in Term)
            {
                if (char.IsWhiteSpace(ch) && char.IsWhiteSpace(lastChar))
                {
                    // Ignore consecutive whitespace
                }
                else if (inGroup)
                {
                    if (ch == lastGroupToken)
                    {
                        // End a group
                        inGroup = false;
                        if (token.ToString() is string val && !string.IsNullOrWhiteSpace(val))
                            Tokens.Add(val);
                        token.Clear();
                    }
                    else
                    {
                        token.Append(ch);
                    }
                }
                else if (groupToken.Contains(ch))
                {
                    // Start a group
                    lastGroupToken = ch;
                    inGroup = true;
                    if (token.ToString() is string val && !string.IsNullOrWhiteSpace(val))
                        Tokens.Add(val);
                    token.Clear();
                }
                else if (char.IsWhiteSpace(ch))
                {
                    if (token.ToString() is string val && !string.IsNullOrWhiteSpace(val))
                        Tokens.Add(val);
                    token.Clear();
                }
                else
                {
                    token.Append(ch);
                }
                lastChar = ch;
            }

            {
                if (token.ToString() is string val && !string.IsNullOrWhiteSpace(val))
                    Tokens.Add(val);
            }
        }
    }
}
