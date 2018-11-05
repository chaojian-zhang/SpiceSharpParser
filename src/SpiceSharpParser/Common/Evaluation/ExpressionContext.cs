﻿using System;
using System.Collections.Generic;
using SpiceSharpParser.Common.Evaluation.Expressions;
using SpiceSharpParser.Common.Evaluation.Functions;

namespace SpiceSharpParser.Common.Evaluation
{
    public class ExpressionContext
    {
        private readonly bool isParameterNameCaseSensitive;
        private readonly bool isFunctionNameCaseSensitive;
        private readonly bool isExpressionNameCaseSensitive;
        private int? _seed;

        public ExpressionContext()
            : this(string.Empty, false, false, false)
        {
        }

        public ExpressionContext(string name, bool isParameterNameCaseSensitive, bool isFunctionNameCaseSensitive, bool isExpressionNameCaseSensitive)
        {
            Name = name;
            this.isParameterNameCaseSensitive = isParameterNameCaseSensitive;
            this.isFunctionNameCaseSensitive = isFunctionNameCaseSensitive;
            this.isExpressionNameCaseSensitive = isExpressionNameCaseSensitive;
            Parameters = new Dictionary<string, Expression>(StringComparerProvider.Get(isParameterNameCaseSensitive));
            Functions = new Dictionary<string, Function>(StringComparerProvider.Get(isFunctionNameCaseSensitive));
            Children = new List<ExpressionContext>();
            ExpressionRegistry = new ExpressionRegistry(isParameterNameCaseSensitive, isExpressionNameCaseSensitive);

            CreateCommonFunctions();
            Randomizer = new Randomizer();
        }

        /// <summary>
        /// Gets the name of the context.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets or sets the random seed for the evaluator.
        /// </summary>
        public int? Seed
        {
            get => _seed;

            set
            {
                _seed = value;

                foreach (var child in Children)
                {
                    child.Seed = value;
                }
            }
        }

        public Randomizer Randomizer { get; set; }

        /// <summary>
        /// Gets or sets data of the context.
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// Gets or sets the parameters.
        /// </summary>
        public Dictionary<string, Expression> Parameters { get; protected set; }

        /// <summary>
        /// Gets or sets custom functions.
        /// </summary>
        public Dictionary<string, Function> Functions { get; protected set; }

        /// <summary>
        /// Gets or sets expression registry for the context.
        /// </summary>
        public ExpressionRegistry ExpressionRegistry { get; set; }

        /// <summary>
        /// Gets or sets the children simulationEvaluators.
        /// </summary>
        public List<ExpressionContext> Children { get; set; }

        /// <summary>
        /// Sets the parameter.
        /// </summary>
        /// <param name="parameterName">A name of parameter.</param>
        /// <param name="value">A value of parameter.</param>
        public void SetParameter(string parameterName, double value)
        {
            var parameter = new ConstantExpression(value);
            Parameters[parameterName] = parameter;

            ExpressionRegistry.AddOrUpdate(parameterName, parameter);
            ExpressionRegistry.InvalidateDependentParameters(parameterName);
            ExpressionRegistry.InvalidateExpressions(parameterName);

            foreach (var child in Children)
            {
                child.SetParameter(parameterName, value);
            }
        }

        /// <summary>
        /// Sets the parameter.
        /// </summary>
        /// <param name="parameterName">A name of parameter.</param>
        /// <param name="expression">An expression of parameter.</param>
        public void SetParameter(string parameterName, string expression, ICollection<string> expressionParameters)
        {
            var parameter = new Expression(expression);
            SetParameter(parameterName, expression, expressionParameters, parameter);
        }

        /// <summary>
        /// Sets the cached parameter.
        /// </summary>
        /// <param name="parameterName">A name of parameter.</param>
        /// <param name="expression">An expression of parameter.</param>
        public void SetCachedParameter(string parameterName, string expression, ICollection<string> expressionParameters)
        {
            var parameter = new CachedExpression(expression);
            SetParameter(parameterName, expression, expressionParameters, parameter);
        }

        /// <summary>
        /// Gets the expression names.
        /// </summary>
        /// <returns>
        /// Enumerable of strings.
        /// </returns>
        public IEnumerable<string> GetExpressionNames()
        {
            return ExpressionRegistry.GetExpressionNames();
        }

        /// <summary>
        /// Sets the named expression.
        /// </summary>
        /// <param name="expressionName">Expression name.</param>
        /// <param name="expression">Expression.</param>
        public void SetNamedExpression(string expressionName, string expression, ICollection<string> parameters)
        {
            ExpressionRegistry.Add(new NamedExpression(expressionName, expression), parameters);
        }

        /// <summary>
        /// Gets the expression by name.
        /// </summary>
        /// <param name="expressionName">Name of expression.</param>
        /// <returns>
        /// Expression.
        /// </returns>
        public string GetExpression(string expressionName)
        {
            return ExpressionRegistry.GetExpression(expressionName)?.ValueExpression;
        }

        /// <summary>
        /// Creates a child context.
        /// </summary>
        /// <param name="name">Name of a context.</param>
        /// <param name="addToChildren">Specifies whether context should be added to children.</param>
        /// <returns>
        /// A child context.
        /// </returns>
        public virtual ExpressionContext CreateChildContext(string name, bool addToChildren)
        {
            var child = new ExpressionContext(name, isParameterNameCaseSensitive, isFunctionNameCaseSensitive, isExpressionNameCaseSensitive);

            child.Parameters = new Dictionary<string, Expression>(Parameters, StringComparerProvider.Get(isParameterNameCaseSensitive));
            child.Data = Data;
            child.Functions = new Dictionary<string, Function>(Functions, StringComparerProvider.Get(isFunctionNameCaseSensitive));
            child.ExpressionRegistry = ExpressionRegistry.Clone();
            child.Seed = Seed;
            child.Randomizer = Randomizer;

            if (addToChildren)
            {
                Children.Add(child);
            }

            return child;
        }

        public virtual ExpressionContext Clone()
        {
            ExpressionContext context = new ExpressionContext(
                Name,
                this.isParameterNameCaseSensitive,
                this.isFunctionNameCaseSensitive,
                this.isExpressionNameCaseSensitive);
            context.Parameters = new Dictionary<string, Expression>(StringComparerProvider.Get(isParameterNameCaseSensitive));

            foreach (var parameter in Parameters)
            {
                context.Parameters.Add(parameter.Key, parameter.Value.Clone());
            }

            context.ExpressionRegistry = ExpressionRegistry.Clone();
            context.Functions = new Dictionary<string, Function>(Functions, StringComparerProvider.Get(this.isFunctionNameCaseSensitive));

            context.Children = new List<ExpressionContext>();
            foreach (var child in Children)
            {
                context.Children.Add(child.Clone());
            }

            context.Seed = Seed;
            context.Data = Data;
            context.Randomizer = new Randomizer();

            return context;
        }

        public void SetParameters(
            Dictionary<string, string> parameters,
            Dictionary<string, ICollection<string>> parametersOfParameters)
        {
            foreach (var paramName in parameters)
            {
                SetParameter(paramName.Key, paramName.Value, parametersOfParameters[paramName.Key]);
            }
        }

        public ExpressionContext Find(string name)
        {
            if (Name == name)
            {
                return this;
            }

            foreach (var child in Children)
            {
                var res = child.Find(name);

                if (res != null)
                {
                    return res;
                }
            }

            return null;
        }

        protected void CreateCommonFunctions()
        {
            Functions.Add("acos", MathFunctions.CreateACos());
            Functions.Add("asin", MathFunctions.CreateASin());
            Functions.Add("atan", MathFunctions.CreateATan());
            Functions.Add("atan2", MathFunctions.CreateATan2());
            Functions.Add("cos", MathFunctions.CreateCos());
            Functions.Add("cosh", MathFunctions.CreateCosh());
            Functions.Add("sin", MathFunctions.CreateSin());
            Functions.Add("sinh", MathFunctions.CreateSinh());
            Functions.Add("tan", MathFunctions.CreateTan());
            Functions.Add("tanh", MathFunctions.CreateTanh());
        }

        protected void SetParameter(string parameterName, string expression, ICollection<string> expressionParameters, Expression parameter)
        {
            Parameters[parameterName] = parameter;

            ExpressionRegistry.AddOrUpdate(parameterName, parameter);
            ExpressionRegistry.AddOrUpdateParameterDependencies(parameterName, expressionParameters);
            ExpressionRegistry.InvalidateDependentParameters(parameterName);
            ExpressionRegistry.InvalidateExpressions(parameterName);

            foreach (var child in Children)
            {
                child.SetParameter(parameterName, expression, expressionParameters);
            }
        }
    }
}