﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NLex
{
    public class Lexer<TLexerState> where TLexerState : LexerState
    {
        protected LexerGrammar<TLexerState> Grammar { get; }
        protected LexerOptions Options { get; }

        public Lexer(LexerGrammar<TLexerState> grammar, LexerOptions options)
        {
            Options = options;
            Grammar = grammar ?? throw new ArgumentNullException(nameof(grammar));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public IEnumerable<Token> GetTokens(string text, TLexerState state = null)
        {
            int textTokenIndex = 0;
            LexerStringReader strReader = new LexerStringReader(text);
            string textToLex = null;

            while (textTokenIndex < text.Length)
            {
                if (Options.SingleLineTokens)
                {
                    if (textToLex == null)
                    {
                        textToLex = strReader.ReadLine();

                        if (textToLex == "") break;
                    }
                }
                else
                {
                    textToLex = text.Substring(textTokenIndex);
                }

                bool tokenReturned = false;

                LexerTokenRule<TLexerState> bestTokenRule;
                Match bestMatch;
                this.FindBestTokenRule(textToLex, state, out bestTokenRule, out bestMatch);

                if (bestMatch != null && bestTokenRule != null)
                {
                    var tokenActionResult = bestTokenRule.LexerRuleResultAction(state);
                    if (tokenActionResult == LexerRuleResult.ReturnToken)
                    {
                        yield return new Token()
                        {
                            Value = bestMatch.Value,
                            TokenType = bestTokenRule.TokenType,
                        };
                    }
                    state.PreviousTokenType = bestTokenRule.TokenType;
                    tokenReturned = true;
                    textTokenIndex += bestMatch.Length;
                    if (Options.SingleLineTokens)
                    {
                        textToLex = textToLex.Substring(bestMatch.Length);
                        if (textToLex == "") textToLex = null;
                    }
                }
                if (!tokenReturned)
                {
                    // undefined token in text
                    throw new LexerException("Can't get next token from text: '" + textToLex + "'");
                }
            }

            // yield EOF token
            yield return new Token()
            {
                TokenType = -1
            };
        }

        void FindBestTokenRule(string remainingText, TLexerState state, out LexerTokenRule<TLexerState> bestMatchTokenRule, out Match bestMatch)
        {
            bestMatchTokenRule = null;
            bestMatch = null;
            foreach (LexerTokenRule<TLexerState> tokenRule in Grammar.LexerRules)
            {
                if (tokenRule.IsActive(state))
                {
                    Match tokenMatch = tokenRule.RegularExpression.Match(remainingText);
                    if (tokenMatch.Success && tokenMatch.Length > 0)
                    {
                        if (bestMatch == null || tokenMatch.Length > bestMatch.Length)
                        {
                            bestMatch = tokenMatch;
                            bestMatchTokenRule = tokenRule;
                        }
                    }
                }
            }
        }
    }
}
