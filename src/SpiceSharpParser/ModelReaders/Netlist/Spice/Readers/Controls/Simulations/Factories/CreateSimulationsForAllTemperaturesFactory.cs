﻿using SpiceSharp;
using SpiceSharp.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Context;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations.Decorators;
using SpiceSharpParser.Models.Netlist.Spice.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations.Factories
{
    public class CreateSimulationsForAllTemperaturesFactory : ICreateSimulationsForAllTemperaturesFactory
    {
        public List<BaseSimulation> CreateSimulations(Control statement, ICircuitContext context, Func<string, Control, ICircuitContext, BaseSimulation> createSimulation)
        {
            var result = new List<BaseSimulation>();

            if (context.Result.SimulationConfiguration.TemperaturesInKelvins.Count > 0)
            {
                foreach (double temp in context.Result.SimulationConfiguration.TemperaturesInKelvins)
                {
                    result.Add(CreateSimulationForTemperature(statement, context, createSimulation, temp));
                }
            }
            else
            {
                result.Add(CreateSimulationForTemperature(statement, context, createSimulation, null));
            }

            return result;
        }

        protected BaseSimulation CreateSimulationForTemperature(Control statement, ICircuitContext context, Func<string, Control, ICircuitContext, BaseSimulation> createSimulation, double? temp)
        {
            var simulation = createSimulation(GetSimulationName(context, statement, temp), statement, context);

            SetTempVariable(context, temp, simulation);
            SetSimulationTemperatures(simulation, temp, context.Result.SimulationConfiguration.NominalTemperatureInKelvins);

            return simulation;
        }

        protected void SetTempVariable(ICircuitContext context, double? operatingTemperatureInKelvins, BaseSimulation simulation)
        {
            double temp = 0;
            if (operatingTemperatureInKelvins.HasValue)
            {
                temp = operatingTemperatureInKelvins.Value - Constants.CelsiusKelvin;
            }
            else
            {
                temp = Constants.ReferenceTemperature - Constants.CelsiusKelvin;
            }

            simulation.BeforeTemperature += (object sender, LoadStateEventArgs e) =>
            {
                context.Evaluator.SetParameter(simulation, "TEMP", temp);
            };
        }

        protected void SetSimulationTemperatures(BaseSimulation simulation, double? operatingTemperatureInKelvins, double? nominalTemperatureInKelvins)
        {
            if (operatingTemperatureInKelvins.HasValue)
            {
                var decorator = new CircuitTemperatureSimulationDecorator(operatingTemperatureInKelvins.Value);
                decorator.Decorate(simulation);
            }

            if (nominalTemperatureInKelvins.HasValue)
            {
                var decorator = new NominalTemperatureSimulationDecorator(nominalTemperatureInKelvins.Value);
                decorator.Decorate(simulation);
            }
        }

        protected string GetSimulationName(ICircuitContext context, Control statement, double? temperatureInKelvin = null)
        {
            if (temperatureInKelvin.HasValue)
            {
                return string.Format("#{0} {1} - at {2} Kelvins ({3} Celsius)", context.Result.Simulations.Count() + 1, statement.Name, temperatureInKelvin.Value, temperatureInKelvin.Value - Constants.CelsiusKelvin);
            }

            return string.Format("#{0} {1}", context.Result.Simulations.Count() + 1, statement.Name);
        }
    }
}