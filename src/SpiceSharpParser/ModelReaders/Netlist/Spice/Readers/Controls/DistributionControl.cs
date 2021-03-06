﻿using SpiceSharpParser.Common.Mathematics.Probability;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System.Linq;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls
{
    /// <summary>
    /// Reads .DISTRIBUTION <see cref="Control"/> from SPICE netlist object model.
    /// </summary>
    public class DistributionControl : BaseControl
    {
        /// <summary>
        /// Reads <see cref="Control"/> statement and modifies the context.
        /// </summary>
        /// <param name="statement">A statement to process.</param>
        /// <param name="context">A context to modify.</param>
        public override void Read(Control statement, ICircuitContext context)
        {
            var curve = new Curve();

            var distributionName = statement.Parameters.First().Image;

            foreach (var param in statement.Parameters.Skip(1))
            {
                if (param is Models.Netlist.Spice.Objects.Parameters.PointParameter pp)
                {
                    var x = pp.Values.Items[0];
                    var y = pp.Values.Items[1];

                    curve.Add(new Point(context.Evaluator.EvaluateDouble(x.Image), context.Evaluator.EvaluateDouble(y.Image)));
                }
            }

            context.Evaluator.GetEvaluationContext().Randomizer.RegisterPdf(distributionName, () => new Pdf(curve));
        }
    }
}