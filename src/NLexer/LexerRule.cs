﻿using System.Text.RegularExpressions;

namespace NLexer
{
    public abstract class LexerRule
    {
        private Regex regex;
        private string regularExpressionPattern;

        /// <summary>
        /// Initializes a new instance of the <see cref="LexerRule"/> class.
        /// </summary>
        /// <param name="name">A name of lexer rule</param>
        /// <param name="regularExpressionPattern">A regular expression</param>
        public LexerRule(string name, string regularExpressionPattern)
        {
            RegularExpressionPattern = regularExpressionPattern;
            Name = name;
        }

        /// <summary>
        /// Gets name of lexer rule
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets or sets a regular expression pattern of lexer rule
        /// </summary>
        public string RegularExpressionPattern
        {
            get
            {
                return this.regularExpressionPattern;
            }

            set
            {
                this.regularExpressionPattern = value;
                this.regex = null;
            }
        }

        /// <summary>
        /// Gets a regular expression of lexer rule
        /// </summary>
        public Regex RegularExpression
        {
            get
            {
                if (regex == null)
                {
                    regex = new Regex("^" + RegularExpressionPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                }

                return regex;
            }
        }

        internal abstract LexerRule Clone();
    }
}