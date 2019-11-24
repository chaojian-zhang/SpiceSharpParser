﻿using SpiceSharpParser.Common.Evaluation;
using SpiceSharpParser.Common.Mathematics.Combinatorics;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Math
{
    public class PolyFunction : Function<double, double>
    {
        public PolyFunction()
        {
            Name = "poly";
            ArgumentsCount = -1;
        }

        public override double Logic(string image, double[] args, EvaluationContext context)
        {
            var dimension = (int)args[0];
            List<double> variables = new List<double>();

            if (args.Length < dimension + 1)
            {
                throw new Exception("Too less variables for poly");
            }

            for (var i = 1; i <= dimension; i++)
            {
                variables.Add(args[i]);
            }

            List<double> coefficients = new List<double>();

            for (var i = dimension + 1; i < args.Length; i++)
            {
                coefficients.Add(args[i]);
            }

            if (coefficients.Count == 0)
            {
                return 0;
            }

            return Get(coefficients, dimension, variables);
        }

        private static double Get(List<double> coefficients, int dimension, List<double> variables)
        {
            var combinations = CombinationCache.GetCombinations(coefficients.Count, dimension);
            double sum = 0.0;
            sum += coefficients[0];

            for (int i = 1; i < combinations.Count; i++)
            {
                sum += ComputeSumElementValue(variables, coefficients[i], combinations[i]);
            }

            return sum;
        }

        public static string GetExpression(int dimension, List<double> coefficients, List<string> variables)
        {
            var combinations = CombinationCache.GetCombinations(coefficients.Count, dimension);
            string expression = coefficients[0].ToString();

            for (int i = 1; i < combinations.Count; i++)
            {
                expression += $" + {ComputeSumElementString(variables, coefficients[i], combinations[i])}";
            }

            return expression;
        }

        private static double ComputeSumElementValue(List<double> variables, double coefficient, int[] combination)
        {
            double result = 1.0;

            for (int i = 0; i < combination.Length; i++)
            {
                result *= variables[combination[i] - 1];
            }

            return result * coefficient;
        }

        private static string ComputeSumElementString(List<string> variables, double coefficient, int[] combination)
        {
            string result = $"{coefficient.ToString(CultureInfo.InvariantCulture)}";

            for (int i = 0; i < combination.Length; i++)
            {
                result += " * ";
                result += variables[combination[i] - 1];
            }

            return result;
        }
    }
}