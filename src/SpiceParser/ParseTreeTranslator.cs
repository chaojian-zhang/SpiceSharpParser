﻿using System;
using System.Collections.Generic;
using SpiceNetlist;
using SpiceNetlist.SpiceObjects;
using SpiceNetlist.SpiceObjects.Parameters;

namespace SpiceParser
{
    public class ParseTreeTranslator
    {
        private Dictionary<ParseTreeNode, ParseTreeTranslatorItem> translatorItems = new Dictionary<ParseTreeNode, ParseTreeTranslatorItem>();
        private Dictionary<string, Func<List<ParseTreeTranslatorItem>, SpiceObject>> translators = new Dictionary<string, Func<List<ParseTreeTranslatorItem>, SpiceObject>>();

        public ParseTreeTranslator()
        {
            translators.Add(SpiceGrammarSymbol.START, (List<ParseTreeTranslatorItem> nt) => CreateNetList(nt));
            translators.Add(SpiceGrammarSymbol.STATEMENTS, (List<ParseTreeTranslatorItem> nt) => CreateStatements(nt));
            translators.Add(SpiceGrammarSymbol.STATEMENT, (List<ParseTreeTranslatorItem> nt) => CreateStatement(nt));
            translators.Add(SpiceGrammarSymbol.MODEL, (List<ParseTreeTranslatorItem> nt) => CreateModel(nt));
            translators.Add(SpiceGrammarSymbol.CONTROL, (List<ParseTreeTranslatorItem> nt) => CreateControl(nt));
            translators.Add(SpiceGrammarSymbol.COMPONENT, (List<ParseTreeTranslatorItem> nt) => CreateComponent(nt));
            translators.Add(SpiceGrammarSymbol.PARAMETERS, (List<ParseTreeTranslatorItem> nt) => CreateParameters(nt));
            translators.Add(SpiceGrammarSymbol.PARAMETER, (List<ParseTreeTranslatorItem> nt) => CreateParameter(nt));
            translators.Add(SpiceGrammarSymbol.PARAMETER_SINGLE, (List<ParseTreeTranslatorItem> nt) => CreateParameterSingle(nt));
            translators.Add(SpiceGrammarSymbol.SUBCKT, (List<ParseTreeTranslatorItem> nt) => CreateSubCircuit(nt));
            translators.Add(SpiceGrammarSymbol.COMMENT_LINE, (List<ParseTreeTranslatorItem> nt) => CreateComment(nt));
            translators.Add(SpiceGrammarSymbol.NEW_LINE_OR_EOF, (List<ParseTreeTranslatorItem> nt) => null);
            translators.Add(SpiceGrammarSymbol.SUBCKT_ENDING, (List<ParseTreeTranslatorItem> nt) => null);
        }

        /// <summary>
        /// Translates a spice parse tree to a context (SpiceNetlist library)
        /// </summary>
        /// <param name="root">A parse tree root</param>
        /// <returns>A net list</returns>
        public NetList GetNetList(ParseTreeNode root)
        {
            var travelsal = new ParseTreeTravelsal();
            var treeNodes = travelsal.GetIterativePostOrder(root);

            foreach (var treeNode in treeNodes)
            {
                if (treeNode is ParseTreeNonTerminalNode nt)
                {
                    var items = new List<ParseTreeTranslatorItem>();

                    foreach (var child in nt.Children)
                    {
                        items.Add(translatorItems[child]);
                    }

                    var treeNodeResult = translators[nt.Name](items);
                    translatorItems[treeNode] = new ParseTreeTranslatorItem()
                    {
                        SpiceObject = treeNodeResult,
                        Node = treeNode
                    };
                }
                else
                {
                    translatorItems[treeNode] = new ParseTreeTranslatorItem()
                    {
                        Node = treeNode,
                        Token = ((ParseTreeTerminalNode)treeNode).Token
                    };
                }
            }

            return translatorItems[root].SpiceObject as NetList;
        }

        private SpiceObject CreateParameter(List<ParseTreeTranslatorItem> childrenItems)
        {
            Parameter parameter = null;

            if (childrenItems.Count > 0)
            {
                if (childrenItems[0].SpiceObject is SingleParameter sp)
                {
                    parameter = sp;
                }
                else
                {
                    if (childrenItems[0].IsToken
                        && childrenItems[0].Token.TokenType == (int)SpiceToken.WORD
                        && childrenItems[1].IsToken
                        && childrenItems[1].Token.TokenType == (int)SpiceToken.DELIMITER
                        && childrenItems[1].Token.Lexem == "("
                        && childrenItems[2].IsSpiceObject
                        && childrenItems[2].SpiceObject is ParameterCollection
                        && childrenItems[3].Token.TokenType == (int)SpiceToken.DELIMITER
                        && childrenItems[3].Token.Lexem == ")")
                    {
                        parameter = new ComplexParameter()
                        {
                            Name = childrenItems[0].Token.Lexem,
                            Parameters = childrenItems[2].SpiceObject as ParameterCollection
                        };
                    }

                    if (childrenItems[0].IsToken
                        && childrenItems[0].Token.TokenType == (int)SpiceToken.WORD
                        && childrenItems[1].Token.Lexem == "="
                        && childrenItems[2].Token.TokenType == (int)SpiceToken.VALUE)
                    {
                        parameter = new AssignmentParameter()
                        {
                            Name = childrenItems[0].Token.Lexem,
                            Value = childrenItems[2].Token.Lexem,
                        };
                    }
                }
            }

            return parameter;
        }

        private SpiceObject CreateParameterSingle(List<ParseTreeTranslatorItem> childrenItems)
        {
            if (!childrenItems[0].IsToken)
            {
                throw new ParseException();
            }

            switch (childrenItems[0].Token.TokenType)
            {
                case (int)SpiceToken.REFERENCE:
                    return new ReferenceParameter() { RawValue = childrenItems[0].Token.Lexem };
                case (int)SpiceToken.VALUE:
                    return new ValueParameter() { RawValue = childrenItems[0].Token.Lexem };
                case (int)SpiceToken.WORD:
                    return new WordParameter() { RawValue = childrenItems[0].Token.Lexem };
                case (int)SpiceToken.IDENTIFIER:
                    return new IdentifierParameter() { RawValue = childrenItems[0].Token.Lexem };
                case (int)SpiceToken.EXPRESSION:
                    return new ExpressionParameter() { RawValue = childrenItems[0].Token.Lexem };
            }

            throw new ParseException();
        }

        private SpiceObject CreateParameters(List<ParseTreeTranslatorItem> childrenItems)
        {
            var parameters = new ParameterCollection();

            if (childrenItems.Count == 2)
            {
                if (childrenItems[0].SpiceObject is Parameter p)
                {
                    parameters.Add(p);
                }

                if (childrenItems[1].SpiceObject is ParameterCollection ps2)
                {
                    parameters.Merge(ps2);
                }
            }

            return parameters;
        }

        private SpiceObject CreateComponent(List<ParseTreeTranslatorItem> childrenItems)
        {
            if (childrenItems.Count != 2 && childrenItems.Count != 3)
            {
                throw new ParseException();
            }

            var component = new Component();
            component.Name = childrenItems[0].Token.Lexem;
            component.PinsAndParameters = childrenItems[1].SpiceObject as ParameterCollection;
            return component;
        }

        private SpiceObject CreateControl(List<ParseTreeTranslatorItem> childrenItems)
        {
            var control = new Control();
            control.Name = childrenItems[1].Token.Lexem;
            control.Parameters = childrenItems[2].SpiceObject as ParameterCollection;
            return control;
        }

        private SpiceObject CreateSubCircuit(List<ParseTreeTranslatorItem> childrenItems)
        {
            if (childrenItems.Count < 3)
            {
                throw new ParseException();
            }

            var subCkt = new SubCircuit();
            subCkt.Name = childrenItems[2].Token.Lexem;

            var allParameters = childrenItems[3].SpiceObject as ParameterCollection;

            // Parse nodes and parameters
            bool mode = true; // true = nodes, false = parameters
            foreach (var parameter in allParameters)
            {
                if (mode)
                {
                    // After this, only parameters will follow
                    if (parameter is SingleParameter s && s.RawValue.ToLower() == "params:")
                    {
                        mode = false;
                    }

                    // Parameters have started, so we will keep reading parameters
                    else if (parameter is AssignmentParameter a)
                    {
                        mode = false;
                        subCkt.DefaultParameters.Add(a);
                    }

                    // Still reading nodes
                    else if (parameter is SingleParameter s2)
                    {
                        if (s2 is WordParameter
                            || s2 is IdentifierParameter
                            || int.TryParse(s2.RawValue, out _))
                        {
                            subCkt.Pins.Add(s2.RawValue);
                        }
                    }
                }
                else if (parameter is AssignmentParameter a2)
                {
                    subCkt.DefaultParameters.Add(a2);
                }
            }

            subCkt.Statements = childrenItems[5].SpiceObject as Statements;
            return subCkt;
        }

        private SpiceObject CreateComment(List<ParseTreeTranslatorItem> childrenItems)
        {
            var comment = new CommentLine();
            comment.Text = childrenItems[1].Token.Lexem;
            return comment;
        }

        private SpiceObject CreateStatement(List<ParseTreeTranslatorItem> childrenItems)
        {
            if (childrenItems.Count == 1 && childrenItems[0].IsSpiceObject)
            {
                return childrenItems[0].SpiceObject as Statement;
            }

            throw new ParseException();
        }

        private SpiceObject CreateModel(List<ParseTreeTranslatorItem> childrenItems)
        {
            var model = new Model();
            model.Name = childrenItems[2].Token.Lexem;
            model.Parameters = childrenItems[3].SpiceObject as ParameterCollection;
            return model;
        }

        private SpiceObject CreateStatements(List<ParseTreeTranslatorItem> childrenItems)
        {
            var statements = new Statements();

            if (childrenItems.Count == 2)
            {
                if (childrenItems[0].SpiceObject is Statement st)
                {
                    statements.Add(st);
                }

                if (childrenItems[1].SpiceObject is Statements sts)
                {
                    statements.Merge(sts);
                }
            }
            else
            {
                if (childrenItems.Count == 1)
                {
                    if (childrenItems[0].SpiceObject is Statements sts)
                    {
                        statements.Merge(sts);
                    }
                    else
                    {
                        if (childrenItems[0].IsToken && childrenItems[0].Token.TokenType == (int)SpiceToken.END)
                        {
                            // skip
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }
                }
            }

            return statements;
        }

        private SpiceObject CreateNetList(List<ParseTreeTranslatorItem> childrenItems)
        {
            return new NetList()
            {
                Title = childrenItems[0].Token.Lexem,
                Statements = childrenItems[1].SpiceObject as Statements
            };
        }
    }
}
