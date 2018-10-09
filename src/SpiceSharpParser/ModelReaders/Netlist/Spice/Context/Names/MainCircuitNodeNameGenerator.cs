﻿using System;
using System.Collections.Generic;
using System.Linq;
using SpiceSharp;
using SpiceSharpParser.Common;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice.Context
{
    public class MainCircuitNodeNameGenerator : INodeNameGenerator
    {
        private HashSet<string> _globals;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainCircuitNodeNameGenerator"/> class.
        /// </summary>
        /// <param name="globals">Global pin names.</param>
        /// <param name="isNodeNameCaseSensitive">Is node name case-sensitive.</param>
        public MainCircuitNodeNameGenerator(IEnumerable<string> globals, bool isNodeNameCaseSensitive)
        {
            if (globals == null)
            {
                throw new ArgumentNullException(nameof(globals));
            }

            IsNodeNameCaseSensitive = isNodeNameCaseSensitive;
            InitGlobals(globals);
        }

        public bool IsNodeNameCaseSensitive { get; }

        /// <summary>
        /// Gets the globals.
        /// </summary>
        public IEnumerable<string> Globals => _globals;

        /// <summary>
        /// Gets or sets the root name.
        /// </summary>
        public string RootName { get; set; }

        /// <summary>
        /// Gets or sets the children.
        /// </summary>
        public List<INodeNameGenerator> Children { get; set; } = new List<INodeNameGenerator>();

        /// <summary>
        /// Generates node name.
        /// </summary>
        /// <param name="pinName">Pin name.</param>
        /// <returns>
        /// Node name.
        /// </returns>
        public string Generate(string pinName)
        {
            if (pinName is null)
            {
                throw new ArgumentNullException(nameof(pinName));
            }

            return pinName;
        }

        /// <summary>
        /// Makes a pin name a global pin name.
        /// </summary>
        /// <param name="pinName">Pin name.</param>
        public void SetGlobal(string pinName)
        {
            if (!_globals.Contains(pinName))
            {
                _globals.Add(pinName);
            }
        }

        /// <summary>
        /// Parses a path and generate a node name.
        /// </summary>
        /// <param name="path">Node path.</param>
        /// <returns>
        /// A node name.
        /// </returns>
        public string Parse(string path)
        {
            string[] parts = path.Split('.');

            if (parts.Length == 1)
            {
                return path;
            }
            else
            {
                string firstSubcircuit = parts[0];

                foreach (var child in Children)
                {
                    if (child.RootName == firstSubcircuit)
                    {
                        string restOfPath = string.Join(".", parts.Skip(1));
                        return child.Parse(restOfPath);
                    }
                }
            }

            return null;
        }

        private void InitGlobals(IEnumerable<string> globals)
        {
            _globals = new HashSet<string>(StringComparerProvider.Get(IsNodeNameCaseSensitive));

            foreach (var global in globals)
            {
                _globals.Add(global);
            }
        }
    }
}