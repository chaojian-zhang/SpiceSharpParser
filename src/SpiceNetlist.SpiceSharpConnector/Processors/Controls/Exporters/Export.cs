﻿namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// Describes a quantity that can be exported using simulated data.
    /// </summary>
    public abstract class Export
    {
        /// <summary>
        /// Gets the type name
        /// </summary>
        public abstract string TypeName { get; }

        /// <summary>
        /// Gets the name based on the properties
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets the export unit
        /// </summary>
        public abstract string QuantityUnit { get; }

        /// <summary>
        /// Gets or sets the type of simulation that the export should act on
        /// eg. "tran", "dc", etc. Default is null (any simulation/optional)
        /// </summary>
        public string SimulationType { get; set; } = null;

        /// <summary>
        /// Extract the quantity from simulated data
        /// </summary>
        /// <returns>
        /// A quantity
        /// </returns>
        public abstract double Extract();

        /// <summary>
        /// Override string representation
        /// </summary>
        /// <returns>
        /// A string represenation of export
        /// </returns>
        public override string ToString()
        {
            return Name;
        }
    }
}