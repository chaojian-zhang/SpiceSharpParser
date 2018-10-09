﻿using SpiceSharpParser.ModelReaders.Netlist.Spice.Mappings;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Exporters;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Controls.Simulations;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Semiconductors;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Components.Sources;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.EntityGenerators.Models;
using SpiceSharpParser.ModelReaders.Netlist.Spice.Readers.Waveforms;

namespace SpiceSharpParser.ModelReaders.Netlist.Spice
{
    public class SpiceObjectMappings : ISpiceObjectMappings
    {
        public SpiceObjectMappings()
        {
            Controls = new ControlMapper();
            Components = new EntityGeneratorMapper();
            Models = new ModelGeneratorMapper();
            Waveforms = new WaveformMapper();
            Exporters = new ExporterMapper();

            // Register waveform generators
            Waveforms.Map("SINE", new SineGenerator());
            Waveforms.Map("PULSE", new PulseGenerator());

            // Register exporters
            Exporters.Map(new string[] { "V", "VI", "VR", "VM", "VDB", "VPH", "VP" }, new VoltageExporter());
            Exporters.Map(new string[] { "I", "IR", "II", "IM", "IDB", "IP" }, new CurrentExporter());
            Exporters.Map("@", new PropertyExporter());

            // Register model generators
            Models.Map(new string[] { "R", "L", "C", "K" }, new RLCModelGenerator());
            Models.Map("D", new DiodeModelGenerator());
            Models.Map(new string[] { "NPN", "PNP" }, new BipolarModelGenerator());
            Models.Map(new string[] { "SW", "CS" }, new SwitchModelGenerator());
            Models.Map(new string[] { "PMOS", "NMOS" }, new MosfetModelGenerator());

            // Register controls
            Controls.Map("ST_R", new StRegisterControl());
            Controls.Map("STEP_R", new StepRegisterControl());
            Controls.Map("PARAM", new ParamControl());
            Controls.Map("FUNC", new FuncControl());
            Controls.Map("GLOBAL", new GlobalControl());
            Controls.Map("CONNECT", new ConnectControl());
            Controls.Map("OPTIONS", new OptionsControl());
            Controls.Map("TEMP", new TempControl());
            Controls.Map("ST", new StControl());
            Controls.Map("STEP", new StepControl());
            Controls.Map("MC", new McControl());
            Controls.Map("TRAN", new TransientControl());
            Controls.Map("AC", new ACControl());
            Controls.Map("DC", new DCControl());
            Controls.Map("OP", new OPControl());
            Controls.Map("NOISE", new NoiseControl());
            Controls.Map("LET", new LetControl());
            Controls.Map("SAVE", new SaveControl(Exporters));
            Controls.Map("PLOT", new PlotControl(Exporters));
            Controls.Map("PRINT", new PrintControl(Exporters));
            Controls.Map("IC", new ICControl());
            Controls.Map("NODESET", new NodeSetControl());

            // Register component generators
            Components.Map(new string[] { "R", "L", "C", "K" }, new RLCGenerator());
            Components.Map(new string[] { "V", "H", "E" }, new VoltageSourceGenerator());
            Components.Map(new string[] { "I", "G", "F" }, new CurrentSourceGenerator());
            Components.Map(new string[] { "S", "W" }, new SwitchGenerator());
            Components.Map("Q", new BipolarJunctionTransistorGenerator());
            Components.Map("D", new DiodeGenerator());
            Components.Map("M", new MosfetGenerator());
            Components.Map("X", new SubCircuitGenerator());
        }

        /// <summary>
        /// Gets or sets the control mapper.
        /// </summary>
        public IMapper<BaseControl> Controls { get; set; }

        /// <summary>
        /// Gets or sets the waveform mapper.
        /// </summary>
        public IMapper<WaveformGenerator> Waveforms { get; set; }

        /// <summary>
        /// Gets or sets the exporter mapper.
        /// </summary>
        public IMapper<Exporter> Exporters { get; set; }

        /// <summary>
        /// Gets or sets the components mapper.
        /// </summary>
        public IMapper<IComponentGenerator> Components { get; set; }

        /// <summary>
        /// Gets or sets the models mapper.
        /// </summary>
        public IMapper<IModelGenerator> Models { get; set; }
    }
}