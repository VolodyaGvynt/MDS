# MDS

A WPF application for loading, visualizing, and analyzing signals in the frequency domain using the Fast Fourier Transform (FFT).

## Features

- Load signals from `.csv` files
- Set signal parameters:
  - Sampling rate (Hz)
  - Start index
  - Signal length
- Choose analysis window:
  - Rectangular
  - Hann
  - Hamming
- Select signal channel (for multi-channel files)
- Perform FFT on a selected signal fragment
- Display:
  - Oscillogram (time-domain)
  - Spectrogram (frequency-domain)

## Requirements

- .NET 6.0 or newer
- OxyPlot.Wpf (install via NuGet)

## NuGet Installation

```shell
Install-Package OxyPlot.Wpf
Install-Package FFTSharp
```

## Usage

1. Clone the repository:

    ```bash
    git clone https://github.com/your-username/SignalViewer.git
    ```

2. Open the solution in Visual Studio.

3. Build and run the application.

4. Click **Browse** to load a CSV file.

5. Configure parameters such as sampling rate, window type, start index, and length.

6. Click **Visualize** to see both time and frequency domain plots.

## CSV File Format

- 1 column: single-channel signal (X = index)
- 2 columns: time/index, signal
- 3+ columns: time/index, multiple channels

The application allows you to choose the channel for visualization.

## License

MIT License
