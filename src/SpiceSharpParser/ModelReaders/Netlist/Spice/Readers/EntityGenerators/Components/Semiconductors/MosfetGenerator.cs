﻿using SpiceSharp.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System;
using System.Collections.Generic;
using SpiceSharpParser.Common.Validation;
using Model = SpiceSharp.Components.Model;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Semiconductors
{
    public class MosfetGenerator : ComponentGenerator
    {
        public MosfetGenerator()
        {
            // MOS1
            Mosfets.Add(typeof(Mosfet1Model), (string name) =>
            {
                var mosfet = new Mosfet1(name);
                return new MosfetDetails { Mosfet = mosfet, SetModelAction = (Model model) => mosfet.Model = model.Name };
            });

            // MOS2
            Mosfets.Add(typeof(Mosfet2Model), (string name) =>
            {
                var mosfet = new Mosfet2(name);
                return new MosfetDetails { Mosfet = mosfet, SetModelAction = (Model model) => mosfet.Model = model.Name };
            });

            // MOS3
            Mosfets.Add(typeof(Mosfet3Model), (string name) =>
            {
                var mosfet = new Mosfet3(name);
                return new MosfetDetails { Mosfet = mosfet, SetModelAction = (Model model) => mosfet.Model = model.Name };
            });
        }

        protected Dictionary<Type, Func<string, MosfetDetails>> Mosfets { get; } = new Dictionary<Type, Func<string, MosfetDetails>>();

        public override SpiceSharp.Components.Component Generate(string componentIdentifier, string originalName, string type, ParameterCollection parameters, ICircuitContext context)
        {
            // Errors
            switch (parameters.Count)
            {
                case 0:
                    context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader,
                        ValidationEntryLevel.Error,
                        $"Node expected for component {componentIdentifier}", parameters.LineInfo));
                    return null;
                case 1:
                case 2:
                case 3:
                    context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader,
                        ValidationEntryLevel.Error,
                        $"Node expected", parameters.LineInfo));
                    return null;
                case 4:
                    context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader,
                        ValidationEntryLevel.Error,
                        $"Model name expected", parameters.LineInfo));
                    return null;
            }

            // Get the model and generate a component for it
            SpiceSharp.Components.Component mosfet = null;
            var modelNameParameter = parameters.Get(4);
            Model model = context.ModelsRegistry.FindModel<Model>(modelNameParameter.Image);
            if (model == null)
            {
                context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader,
                    ValidationEntryLevel.Error,
                    $"Could not find model {modelNameParameter.Image} for mosfet {originalName}", parameters.LineInfo));

                return null;
            }

            if (Mosfets.ContainsKey(model.GetType()))
            {
                var mosfetDetails = Mosfets[model.GetType()].Invoke(componentIdentifier);
                mosfet = mosfetDetails.Mosfet;

                context.SimulationPreparations.ExecuteActionBeforeSetup((simulation) =>
                {
                    context.ModelsRegistry.SetModel(
                        mosfetDetails.Mosfet,
                        simulation,
                        modelNameParameter,
                        $"Could not find model {modelNameParameter.Image} for mosfet {componentIdentifier}",
                        mosfetDetails.SetModelAction,
                        context.Result);
                });
            }
            else
            {
                context.Result.Validation.Add(new ValidationEntry(ValidationEntrySource.Reader,
                    ValidationEntryLevel.Error,
                    $"Invalid model {model.GetType()} for {componentIdentifier}", parameters.LineInfo));

                return null;
            }

            // The rest is all just parameters
            context.CreateNodes(mosfet, parameters);
            SetParameters(context, mosfet, parameters.Skip(5), true);
            return mosfet;
        }

        protected class MosfetDetails
        {
            public SpiceSharp.Components.Component Mosfet { get; set; }

            public Action<Model> SetModelAction { get; set; }
        }
    }
}