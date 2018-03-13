﻿using System.Collections.Generic;
using SpiceSharp.Circuits;
using SpiceSharp.Components;

namespace SpiceNetlist.SpiceSharpConnector.Processors.EntityGenerators.Models
{
    public class DiodeModelGenerator : ModelGenerator
    {
        public override List<string> GetGeneratedSpiceTypes()
        {
            return new List<string>() { "d" };
        }

        internal override Entity GenerateModel(string name, string type)
        {
            return new DiodeModel(name);
        }
    }
}