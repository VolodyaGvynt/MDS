using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using FftSharp;
using FftSharp.Windows;

namespace SignalViewerWpf
{
    public partial class MainWindow : System.Windows.Window
    {
        private List<double[]> signalData = new();
        private int channelCount = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "CSV Files|*.csv" };
            if (dlg.ShowDialog() == true)
            {
                FilePathBox.Text = dlg.FileName;
                LoadCsv(dlg.FileName);
            }
        }

        private void LoadCsv(string path)
        {
            signalData.Clear();

            foreach (var line in File.ReadLines(path))
            {
                var parts = line.Split(';').Select(p => p.Trim()).ToArray();
                if (parts.Length < 1) continue;

                double[] values = parts.Select(p =>
                    double.TryParse(p.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var d)
                        ? d
                        : 0.0
                ).ToArray();

                if (signalData.Count > 0 && values.Length != signalData[0].Length)
                    continue;

                signalData.Add(values);
            }

            if (signalData.Count > 0)
            {
                channelCount = signalData[0].Length;
                ChannelCombo.Items.Clear();
                for (int i = 0; i < channelCount; i++)
                    ChannelCombo.Items.Add($"Channel {i + 1}");
                ChannelCombo.SelectedIndex = 0;
            }
        }

        private void Visualize_Click(object sender, RoutedEventArgs e)
        {
            if (signalData.Count == 0) return;

            if (!int.TryParse(StartBox.Text, out int start)) start = 0;
            if (!int.TryParse(LengthBox.Text, out int length)) length = 1000;
            if (!int.TryParse(SampleRateBox.Text, out int sampleRate)) sampleRate = 1000;
            if (ChannelCombo.SelectedIndex < 0) return;

            int ch = ChannelCombo.SelectedIndex;

            var subSignal = signalData
                .Skip(start)
                .Take(length)
                .Select(row => row.Length > ch ? row[ch] : 0.0)
                .ToArray();

            var time = Enumerable.Range(0, subSignal.Length)
                .Select(i => i / (double)sampleRate)
                .ToArray();

            PlotOscillogram(time, subSignal);
            PlotSpectrum(subSignal, sampleRate);
        }

        private void PlotOscillogram(double[] time, double[] values)
        {
            var model = new PlotModel { Title = "Oscillogram" };
            model.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "Time (s)" });
            model.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "Amplitude" });

            var series = new LineSeries();
            for (int i = 0; i < time.Length; i++)
                series.Points.Add(new DataPoint(time[i], values[i]));

            model.Series.Add(series);
            PlotOsc.Model = model;
        }

        private void PlotSpectrum(double[] signal, int sampleRate)
        {
            double[] windowed = ApplyWindow(signal);

            int n = windowed.Length;
            int nextPow2 = 1;
            while (nextPow2 < n) nextPow2 <<= 1;
            if (n != nextPow2)
            {
                Array.Resize(ref windowed, nextPow2);
            }

            var fftResult = FftSharp.FFT.ForwardReal(windowed);
            double[] magnitudes = FftSharp.FFT.Magnitude(fftResult);
            double[] freqs = FftSharp.FFT.FrequencyScale(windowed.Length, sampleRate);

            int usablePoints = magnitudes.Length;

            var model = new PlotModel { Title = "Frequency Spectrum" };
            model.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "Frequency (Hz)" });
            model.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "Magnitude" });

            var series = new LineSeries();
            for (int i = 0; i < usablePoints; i++)
                series.Points.Add(new DataPoint(freqs[i], magnitudes[i]));

            model.Series.Add(series);
            PlotSpec.Model = model;
        }

        private double[] ApplyWindow(double[] input)
        {
            string selected = ((ComboBoxItem)WindowCombo.SelectedItem)?.Content?.ToString() ?? "Rectangular";

            return selected switch
            {
                "Hann" => new Hanning().Apply(input),
                "Hamming" => new Hamming().Apply(input),
                _ => input
            };
        }
    }
}
