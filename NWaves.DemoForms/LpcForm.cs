﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NWaves.Audio;
using NWaves.FeatureExtractors;
using NWaves.FeatureExtractors.Base;
using NWaves.Filters.Base;
using NWaves.Signals;
using NWaves.Transforms;
using LevelScale = NWaves.Utils.Scale;

namespace NWaves.DemoForms
{
    public partial class LpcForm : Form
    {
        private const double WindowSize = 0.032;
        private const double HopSize = 0.010;

        private DiscreteSignal _signal;
        private List<FeatureVector> _lpcVectors;

        private Fft _fft;


        public LpcForm()
        {
            InitializeComponent();
            lpcPanel.ForeColor = Color.SeaGreen;
            lpcPanel.Stride = 20;
            lpcPanel.Thickness = 2;
            lpcPanel.Gain = 50;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            using (var stream = new FileStream(ofd.FileName, FileMode.Open))
            {
                var waveFile = new WaveFile(stream);
                _signal = waveFile[Channels.Left];
            }

            _fft = new Fft(512);
            
            var lpcExtractor = new LpcExtractor(16, WindowSize, HopSize);

            _lpcVectors = lpcExtractor.ComputeFrom(_signal);

            FillFeaturesList(_lpcVectors, lpcExtractor.FeatureDescriptions);
            lpcListView.Items[0].Selected = true;

            spectrumPanel.Stride = 2;
            spectrumPanel.Line = ComputeSpectrum(0);
            spectrumPanel.Markline = EstimateSpectrum(0);
            spectrumPanel.ToDecibel();

            lpcPanel.Line = _lpcVectors[0].Features;
        }

        double[] ComputeSpectrum(int idx)
        {
            var pos = (int)(_signal.SamplingRate * HopSize * idx);

            return _fft.PowerSpectrum(_signal[pos, pos + 512], normalize: false).Samples;
        }

        double[] EstimateSpectrum(int idx)
        {
            var vector = _lpcVectors[idx].Features.ToArray();  // make new copy of array of features
            var gain = Math.Sqrt(vector[0]);
            vector[0] = 1.0;

            var lpcFilter = new IirFilter(new[] { gain }, vector);

            return lpcFilter.FrequencyResponse()
                            .Power
                            .Select(r => LevelScale.ToDecibel(r))
                            .ToArray();
        }

        private void FillFeaturesList(IEnumerable<FeatureVector> featureVectors, 
                                      IEnumerable<string> featureDescriptions)
        {
            lpcListView.Clear();
            lpcListView.Columns.Add("time", 50);

            foreach (var name in featureDescriptions)
            {
                lpcListView.Columns.Add(name, 70);
            }

            foreach (var vector in featureVectors)
            {
                var item = new ListViewItem { Text = vector.TimePosition.ToString() };
                item.SubItems.AddRange(vector.Features.Select(f => f.ToString("F4")).ToArray());

                lpcListView.Items.Add(item);
            }
        }

        private void lpcListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            var pos = e.ItemIndex;

            spectrumPanel.Line = ComputeSpectrum(pos);
            spectrumPanel.Markline = EstimateSpectrum(pos);
            spectrumPanel.ToDecibel();

            lpcPanel.Line = _lpcVectors[pos].Features;
        }
    }
}
