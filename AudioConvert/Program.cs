using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using System.IO;
using Google.Cloud.Speech.V1;

namespace AudioConvert
{
    class Program
    {
        static void Main(string[] args)
        {


            string InputAudioFilePath = @"C:\Users\mahmoud\Documents\visual studio 2015\Projects\AudioConvert\AudioConvert\dahesh.wav";
            string outfolder = @"C:\Users\mahmoud\Documents\visual studio 2015\Projects\AudioConvert\AudioConvert";
            string OutputAudioFilePath = @"C:\Users\mahmoud\Documents\visual studio 2015\Projects\AudioConvert\AudioConvert\song_mono.wav";

            string prefix = "test";

            
            //SpliteFile(InputAudioFilePath);
            FileFromAPI(InputAudioFilePath);
        }

        public List<string> ListFormAPI(string sourceFile) {
            try
            {
                List<string> Result = new List<string>();
                string extention = Path.GetExtension(sourceFile);
                switch (extention)
                {
                    case ".mp3":
                        sourceFile = CovertMp3ToWav(sourceFile);
                        break;
                    case ".mp4":
                        sourceFile = CovertMp4ToWav(sourceFile);
                        break;

                    default:
                        break;
                }
                TimeSpan duration = new WaveFileReader(sourceFile).TotalTime;
                List<string> inputs = new List<string>();
                if (duration.TotalSeconds > 59)
                {
                    inputs = SpliteFile(sourceFile);
                    foreach (string item in inputs)
                    {
                        Result.AddRange(APIContact(item));
                    }
                }
                else
                {
                    Result.AddRange(APIContact(sourceFile));
                }

               

                return Result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public static string FileFromAPI(string sourceFile) 
        {
            try
            {
                List<string> Result = new List<string>();
            string directory = Path.GetDirectoryName(sourceFile);
            string baseFileName = Path.GetFileNameWithoutExtension(sourceFile);
            string extention = Path.GetExtension(sourceFile);
                switch (extention)
                {
                    case ".mp3":
                        sourceFile = CovertMp3ToWav(sourceFile);
                        break;
                    case ".mp4":
                        sourceFile = CovertMp4ToWav(sourceFile);
                        break;

                    default:
                        break;
                }
                TimeSpan duration = new WaveFileReader(sourceFile).TotalTime;
                List<string> inputs = new List<string>();
                if (duration.TotalSeconds>59)
                {
                    inputs=SpliteFile(sourceFile);
                    foreach (string item in inputs)
                    {
                        Result.AddRange(APIContact(item));
                    }
                }
                else
                {
                    Result.AddRange(APIContact(sourceFile));
                }

            string output = directory + "\\" + baseFileName + ".txt";
            using (var tw = new StreamWriter(output, true))
                {
                    foreach (var item in Result)
                    {
                        tw.Flush();
                        tw.WriteLine(item);
                    }
                    tw.Close();


                }

                return output;
            }
            catch (Exception)
            {

                throw;
            }
          
        }
        public static List<string> APIContact(string sourceFile) {
            var speech = SpeechClient.Create();
            List<string> Result = new List<string>();
            var response = speech.Recognize(new RecognitionConfig()
            {
                Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                // SampleRateHertz = 16000,
                LanguageCode = "ar-EG",
            }, RecognitionAudio.FromFile(sourceFile));
            foreach (var result in response.Results)
            {
                foreach (var alternative in result.Alternatives)
                {
                    // change what you need to do  after geting api response 
                    Result.Add(alternative.Transcript.ToString());
                }
            }
            return Result;
        }

        public static string CovertMp4ToWav(string sourceFile)
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

        public static string CovertMp3ToWav(string sourceFile) {
            string direcctory = Path.GetDirectoryName(sourceFile);
            string baseFileName = Path.GetFileNameWithoutExtension(sourceFile);
            string Extension = ".wav";
            string output;
            using (Mp3FileReader reader = new Mp3FileReader(sourceFile))
            {
                output = direcctory + "\\" + baseFileName  + Extension;
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
                    output = direcctory+"\\" + baseFileName + "mono" + Extension;
                    WaveFileWriter.CreateWaveFile(output, resampler);
                }
            }
            return output;
        }

        public static List<string> SpliteFile(string SourceFile) {

            List<string> result = new List<string>();
            try
            {
                string direcctory = Path.GetDirectoryName(SourceFile);
                string baseFileName = Path.GetFileNameWithoutExtension(SourceFile);
                string Extension = Path.GetExtension(SourceFile);

                TimeSpan cutFromStart = new TimeSpan(0, 0, 0);
                TimeSpan interval = new TimeSpan(0, 0, 58);
                TimeSpan duration = new WaveFileReader(SourceFile).TotalTime;
                int nNoofFiles = (int)duration.TotalSeconds / 58;
                if (nNoofFiles == 0)
                {
                    nNoofFiles = 1;
                }
                if (nNoofFiles * 58 < duration.TotalSeconds)
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

                    string outPath = direcctory+"\\" + baseFileName + (i + 1).ToString() + ".wav";

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
