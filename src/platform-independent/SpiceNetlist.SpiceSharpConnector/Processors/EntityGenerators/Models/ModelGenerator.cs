﻿using System;
using SpiceNetlist.SpiceObjects;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceNetlist.SpiceSharpConnector.Context;
using SpiceNetlist.SpiceSharpConnector.Extensions;

namespace SpiceNetlist.SpiceSharpConnector.Processors.EntityGenerators
{
    public abstract class ModelGenerator : EntityGenerator
    {
        public override Entity Generate(Identifier id, string originalName, string type, ParameterCollection parameters, IProcessingContext context)
        {
            var model = GenerateModel(id.ToString(), type);
            if (model == null)
            {
                throw new Exception();
            }

            context.SetParameters(model, parameters);
            return model;
        }

        internal abstract Entity GenerateModel(string name, string type);
    }
}
