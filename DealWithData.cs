using Recorder.MFCC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Recorder.Recorder;
using System.Windows.Forms;
using Accord.Audio;

namespace Recorder.MainFuctions
{
    public class ClosestMatch
    {
        public string Username;
        public double MinimumDistance;

        public ClosestMatch()
        {
            this.MinimumDistance = double.MaxValue;
            this.Username = "";
        }
    }

    public static class DealWithData
    {
        public static void SaveSequence(Sequence sequence, string username)
        {
            // Create 'template' folder if it doesn't exist
            string templateDir = Path.Combine(Application.StartupPath, "template");
            if (!Directory.Exists(templateDir))
            {
                Directory.CreateDirectory(templateDir);
            }

            // Full path for user's file
            string userFilePath = Path.Combine(templateDir, $"{username}.txt");

            using (StreamWriter writer = new StreamWriter(userFilePath, false))
            {
                // Write 13 lines of MFCC features (each line = one feature dimension over all frames)
                for (int i = 0; i < 13; i++)
                {
                    for (int j = 0; j < sequence.Frames.Length; j++)
                    {
                        double feature = sequence.Frames[j].Features[i];
                        writer.Write(feature.ToString("F6"));
                        if (j < sequence.Frames.Length - 1)
                            writer.Write("|");
                    }
                    writer.WriteLine();
                }
            }

            MessageBox.Show($"User '{username}' saved successfully in template folder.");
        }


        public static bool CheckIfUserExist(string username)
        {
            string templateDir = Path.Combine(Application.StartupPath, "template");
            string userFilePath = Path.Combine(templateDir, $"{username}.txt");

            return File.Exists(userFilePath);
        }

        public static ClosestMatch GetUserName(Sequence sequence, string pruned, int windowSize ,double beamThreshold)
        {
            Timer.StartTotalOperation();
            ClosestMatch user = new ClosestMatch
            {
                MinimumDistance = double.MaxValue,
                Username = "Unknown"
            };

            string templateDir = Path.Combine(Application.StartupPath, "template");

            if (!Directory.Exists(templateDir))
            {
                MessageBox.Show("Template directory not found. No users have been saved yet.");
                return user;
            }

            string[] templateFiles = Directory.GetFiles(templateDir, "*.txt");

            foreach (string filePath in templateFiles)
            {
                string username = Path.GetFileNameWithoutExtension(filePath);
                Sequence toBeCompared = new Sequence();
                List<Sequence> temp;
                bool initialized = false;
                int featureIndex = 0;

                using (StreamReader reader = new StreamReader(filePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null && featureIndex < 13)
                    {
                        string[] features = line.Split('|');

                        if (!initialized)
                        {
                            toBeCompared.Frames = new MFCCFrame[features.Length];
                            for (int i = 0; i < features.Length; i++)
                            {
                                toBeCompared.Frames[i] = new MFCCFrame();
                            }
                            initialized = true;
                        }

                        for (int frameIndex = 0; frameIndex < features.Length; frameIndex++)
                        {
                            // Parse feature, handle potential parsing errors (optional but good practice)
                            if (double.TryParse(features[frameIndex], out double parsedFeature))
                            {
                                toBeCompared.Frames[frameIndex].Features[featureIndex] = parsedFeature;
                            }
                            else
                            {

                                Console.WriteLine($"Warning: Could not parse feature '{features[frameIndex]}' from file '{filePath}'. Setting to 0.");
                                toBeCompared.Frames[frameIndex].Features[featureIndex] = 0.0;
                            }
                        }

                        featureIndex++;
                    }
                }

                double distance;
                if (pruned == "DTW With Pruning")
                {
                    distance = DTW.DTW_With_Pruned(toBeCompared, sequence, windowSize);
                }
                else if (pruned == "DTW_BeamSearch")
                {
                    distance = DTW.DTW_BeamSearch(toBeCompared, sequence, windowSize, beamThreshold);
                }
             
                else
                {
                    distance = DTW.ComputeDTWDistance(toBeCompared, sequence);
                }

                if (distance < user.MinimumDistance)
                {
                    user.MinimumDistance = distance;
                    user.Username = username;
                }
            }
            Timer.StopTotalOperation();
            return user;
        }
        public static ClosestMatch GetUserNameFromMemory(Sequence sequence, string pruned, int windowSize, double beamThreshold, List<(string username, Sequence seq)> trainingSet)
        {
            Timer.StartTotalOperation();

            ClosestMatch user = new ClosestMatch
            {
                MinimumDistance = double.MaxValue,
                Username = "Unknown"
            };

            foreach (var (username, toBeCompared) in trainingSet)
            {
                double distance;
                if (pruned == "DTW With Pruning")
                {
                    distance = DTW.DTW_With_Pruned(sequence, toBeCompared, windowSize);
                }
                else if (pruned == "DTW_BeamSearch")
                {
                    distance = DTW.DTW_BeamSearch(toBeCompared, sequence, windowSize, beamThreshold);
                }
                else
                {
                    distance = DTW.ComputeDTWDistance(sequence, toBeCompared);
                }
                if (distance < user.MinimumDistance)
                {
                    user.MinimumDistance = distance;
                    user.Username = username;
                }
            }
            Timer.StopTotalOperation();

            return user;
        }

    }
}