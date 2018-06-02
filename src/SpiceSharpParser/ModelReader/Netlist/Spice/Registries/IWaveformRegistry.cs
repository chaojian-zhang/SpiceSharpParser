﻿using SpiceSharpParser.ModelReader.Netlist.Spice.Readers.Waveforms;

namespace SpiceSharpParser.ModelReader.Netlist.Spice.Registries
{
    /// <summary>
    /// Interface for all waveform registries
    /// </summary>
    public interface IWaveformRegistry : IRegistry
    {
        /// <summary>
        /// Gets a value indicating whether a specified waveform generator is in registry
        /// </summary>
        /// <param name="type">Type of waveform generator</param>
        /// <returns>
        /// A value indicating whether a specified waveform generator is in registry
        /// </returns>
        bool Supports(string type);

        /// <summary>
        /// Gets the generator by type
        /// </summary>
        /// <param name="type">Type of waveform generator</param>
        /// <returns>
        /// A reference to waveform generator
        /// </returns>
        WaveformGenerator Get(string type);

        /// <summary>
        /// Adds waveform generator to registry
        /// </summary>
        /// <param name="generator">A waveform generator to add</param>
        void Add(WaveformGenerator generator);
    }
}