using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
namespace Converter
{
    public class WavConverter
    {



        public string CovertMp4ToWav(string sourceFile)
        {
            string direcctory = Path.GetDirectoryName(sourceFile);
            string baseFileName = Path.GetFileNameWithoutExtension(sourceFile);
            string Extension = ".wav";
            string output;
            using (var reader = new MediaFoundationReader(sourceFile))
            {
                output = direcctory + "\\" + baseFileName + Extension;
                var outFormat = new WaveFormat(reader.WaveFormat.SampleRate, 1);
                using (var resampler = new MediaFoundationResampler(reader, outFormat))
                {
                    WaveFileWriter.CreateWaveFile(output, resampler);
                }
            }

            return output;
        }

        public string CovertMp3ToWav(string sourceFile)
        {
            string direcctory = Path.GetDirectoryName(sourceFile);
            string baseFileName = Path.GetFileNameWithoutExtension(sourceFile);
            string Extension = ".wav";
            string output;
            using (Mp3FileReader reader = new Mp3FileReader(sourceFile))
            {
                output = direcctory + "\\" + baseFileName + Extension;
                //WaveFileWriter.CreateWaveFile(output, reader);
                var outFormat = new WaveFormat(reader.WaveFormat.SampleRate, 1);
                using (var resampler = new MediaFoundationResampler(reader, outFormat))
                {
                    WaveFileWriter.CreateWaveFile(output, resampler);
                }
            }

            return output;
        }
        public static string StereoToMono(string sourceFile)
        {
            string output = "";
            using (var waveFileReader = new WaveFileReader(sourceFile))
            {
                string direcctory = Path.GetDirectoryName(sourceFile);
                string baseFileName = Path.GetFileNameWithoutExtension(sourceFile);
                string Extension = Path.GetExtension(sourceFile);
                var outFormat = new WaveFormat(waveFileReader.WaveFormat.SampleRate, 1);
                using (var resampler = new MediaFoundationResampler(waveFileReader, outFormat))
                {
                    output = direcctory + "\\" + baseFileName + "mono" + Extension;
                    WaveFileWriter.CreateWaveFile(output, resampler);
                }
            }
            return output;
        }

        public static List<string> SpliteFile(string SourceFile)
        {

            List<string> result = new List<string>();
            try
            {
                string direcctory = Path.GetDirectoryName(SourceFile);
                string baseFileName = Path.GetFileNameWithoutExtension(SourceFile);
                string Extension = Path.GetExtension(SourceFile);

                TimeSpan cutFromStart = new TimeSpan(0, 0, 0);
                TimeSpan interval = new TimeSpan(0, 1, 0);
                TimeSpan duration = new WaveFileReader(SourceFile).TotalTime;
                int nNoofFiles = (int)duration.TotalSeconds / 60;
                if (nNoofFiles == 0)
                {
                    nNoofFiles = 1;
                }
                if (nNoofFiles * 60 < duration.TotalSeconds)
                {
                    nNoofFiles += 1;
                }
                SourceFile = StereoToMono(SourceFile);
                for (int i = 0; i < nNoofFiles; i++)
                {
                    if (i != 0)
                    {
                        cutFromStart = cutFromStart.Add(interval);

                    }

                    string outPath = direcctory + "\\" + baseFileName + (i + 1).ToString() + ".wav";

                    using (var reader = new AudioFileReader(SourceFile))
                    {
                        reader.CurrentTime = cutFromStart; // jump forward to the position we want to start from
                        WaveFileWriter.CreateWaveFile16(outPath, reader.Take(interval));
                        result.Add(outPath);
                    }



                }

            }
            catch (Exception Ex)
            {
                throw new ArgumentException(Ex.Message);
            }
            File.Delete(SourceFile);
            return result;
        }

    }
}
