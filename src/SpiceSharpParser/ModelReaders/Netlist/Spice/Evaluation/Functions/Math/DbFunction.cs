﻿using SpiceSharpParser.Common.Evaluation;
using System;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Evaluation.Functions.Math
{
    public class DbFunction : Function<double, double>
    {
        public DbFunction(SpiceExpressionMode mode)
        {
            Name = "db";
            ArgumentsCount = 1;
            Mode = mode;
        }

        public SpiceExpressionMode Mode { get; }

        public override double Logic(string image, double[] args, EvaluationContext context)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("db() function expects one argument");
            }

            double x = args[0];

            if (Mode == SpiceExpressionMode.SmartSpice)
            {
                return 20.0 * System.Math.Log10(System.Math.Abs(x));
            }

            return System.Math.Sign(x) * 20.0 * System.Math.Log10(System.Math.Abs(x));
        }
    }
}