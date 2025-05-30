using Recorder.MFCC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Recorder
{
    struct User
    {
        public string UserName;
        public List<AudioSignal> UserTemplates;
    }
    static class TestcaseLoader
    {
        //11 users. each user has ~100 small training samples (with silent parts removed).

        static public List<User> LoadTestcase1Training(string trainingListFileName)
        {
            return LoadDataset(trainingListFileName);
        }


        static public List<User> LoadTestcase1Testing(string testingListFileName)
        {
            return LoadDataset(testingListFileName);
        }

        //WARNING: this function in particular is not tested!!!!!
        static public double CheckTestcaseAccuracy(List<User> testCase, List<string> testcaseResult)
        {
            int misclassifiedSamples = 0;
            int resultIndex = 0;
            for (int i = 0; i < testCase.Count; i++)
            {
                for (int j = 0; j < testCase[i].UserTemplates.Count; j++)
                {
                    if (testCase[i].UserName != testcaseResult[resultIndex])
                        misclassifiedSamples++;

                    resultIndex++;
                }
            }

            return (double)misclassifiedSamples / testcaseResult.Count;
        }

        //11 users. each user has ~10 medium sized training samples (with silent parts removed).
        static public List<User> LoadTestcase2Training(string trainingListFileName)
        {
            var originalDataset = LoadDataset(trainingListFileName);

            //shrinkage factor should be larger than 1.
            return ConcatenateSamples(originalDataset, 10);
        }

        static public List<User> LoadTestcase2Testing(string testingListFileName)
        {
            var originalDataset = LoadDataset(testingListFileName);

            //shrinkage factor should be larger than 1.
            return ConcatenateSamples(originalDataset, 10);
        }

        //11 users. each user has ~2 large sized training samples (with silent parts removed).
        static public List<User> LoadTestcase3Training(string trainingListFileName)
        {
            var originalDataset = LoadDataset(trainingListFileName);

            //shrinkage factor should be larger than 1.
            return ConcatenateSamples(originalDataset, 40);
        }

        static public List<User> LoadTestcase3Testing(string testingListFileName)
        {
            var originalDataset = LoadDataset(testingListFileName);

            //shrinkage factor should be larger than 1.
            return ConcatenateSamples(originalDataset, 40);
        }

        static private List<User> LoadDataset(string datasetFileName)
        {
            //Get The dataset folder path.
            var splittedPath = datasetFileName.Split('\\');
            string folderPath = "";
            for (int i = 0; i < splittedPath.Length - 1; i++)
            {
                folderPath += splittedPath[i] + '\\';
            }
            folderPath += "audiofiles\\";


            //read the training samples files names
            Dictionary<string, User> users = new Dictionary<string, User>();
            StreamReader reader = new StreamReader(datasetFileName);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string userName = line.Split('/')[0];
                string fileName = line.Split('/')[1] + ".wav";
                //check if user already exists, if not add an entry in the dictionary.
                if (users.ContainsKey(userName) == false)
                {
                    User user = new User();
                    user.UserTemplates = new List<AudioSignal>();
                    user.UserName = userName;
                    users.Add(userName, user);
                }
                AudioSignal audio;
                string fullFileName = folderPath + userName + '\\' + fileName;
                try
                {
                    audio = openNISTWav(fullFileName);
                }
                catch (Exception)
                {
                    audio = AudioOperations.OpenAudioFile(folderPath + userName + '\\' + fileName);
                }
                //Console.WriteLine($"[Before] Length = {audio.data.Length}");
                audio = AudioOperations.RemoveSilence(audio);
                //Console.WriteLine($"[After] Length = {audio.data.Length}");
                if (audio.data.Length == 0)
                {
                    MessageBox.Show("Empty audio: " + fullFileName);
                }
                users[userName].UserTemplates.Add(audio);
            }
            reader.Close();

            //move the users to a list for convenience reasons only.
            List<User> usersList = new List<User>();
            foreach (var user in users)
            {
                usersList.Add(user.Value);
            }

            return usersList;
        }

        static private AudioSignal openNISTWav(string filename)
        {
            int sample_rate = 0, sample_count = 0, sample_n_bytes = 0;
            using (StreamReader reader = new StreamReader(filename))
            {
                while (true)
                {
                    string line = reader.ReadLine();
                    if (line == null) throw new Exception("Invalid NIST WAV file");
                    var splittedLine = line.Split(' ');
                    if (splittedLine[0] == "sample_count")
                        sample_count = int.Parse(splittedLine[2]);
                    else if (splittedLine[0] == "sample_rate")
                        sample_rate = int.Parse(splittedLine[2]);
                    else if (splittedLine[0] == "sample_n_bytes")
                        sample_n_bytes = int.Parse(splittedLine[2]);
                    else if (splittedLine[0] == "end_head")
                        break;
                }
            }

            byte[] wav = File.ReadAllBytes(filename);
            int pos = 1024; // header offset in bytes

            double[] data = new double[sample_count];
            int i = 0;
            while (pos < wav.Length && i < sample_count)
            {
                data[i] = bytesToDouble(wav[pos], wav[pos + 1]);
                pos += 2;
                i++;
            }

            AudioSignal signal = new AudioSignal
            {
                sampleRate = sample_rate,
                data = data,
                signalLengthInMilliSec = (double)1000.0 * sample_count / sample_rate
            };
            return signal;
        }



        static private double bytesToDouble(byte firstByte, byte secondByte)
        {
            short s = (short)((secondByte << 8) | firstByte);
            return s / 32768.0;
        }

        static private List<User> ConcatenateSamples(List<User> dataset, int shrinkagefactor)
        {
            List<User> newDataset = new List<User>();
            foreach (User user in dataset)
            {

                int numberOfSequences = user.UserTemplates.Count;
                //NOTE: i didn't handle the case if the number of sequences is not divisible by the shrinkage factor :)
                int newNumberOfSequences = numberOfSequences / shrinkagefactor;
                User concUser = new User();
                concUser.UserName = user.UserName;
                concUser.UserTemplates = new List<AudioSignal>(newNumberOfSequences);
                int startIndex = 0;
                for (int i = 0; i < newNumberOfSequences; i++)
                {
                    int currentConcSeqLength = 0;
                    double currentConcSeqDuration = 0;
                    for (int j = startIndex; j < startIndex + shrinkagefactor; j++)
                    {
                        currentConcSeqLength += user.UserTemplates[j].data.Length;
                        currentConcSeqDuration += user.UserTemplates[j].signalLengthInMilliSec;
                    }
                    concUser.UserTemplates.Add(new AudioSignal());
                    concUser.UserTemplates[i].sampleRate = user.UserTemplates[0].sampleRate;
                    concUser.UserTemplates[i].signalLengthInMilliSec = currentConcSeqDuration;
                    concUser.UserTemplates[i].data = new double[currentConcSeqLength];
                    int concIndex = 0;
                    for (int j = startIndex; j < startIndex + shrinkagefactor; j++)
                    {
                        user.UserTemplates[j].data.CopyTo(concUser.UserTemplates[i].data, concIndex);
                        concIndex += user.UserTemplates[j].data.Length;
                    }

                    startIndex += shrinkagefactor;
                }

                newDataset.Add(concUser);
            }
            return newDataset;
        }
        //public static void SaveFeatures(List<(string UserName, Sequence Seq)> features, string folderPath)
        //{
        //    if (!Directory.Exists(folderPath))
        //        Directory.CreateDirectory(folderPath);

        //    foreach (var (UserName, Seq) in features)
        //    {
        //        string filePath = Path.Combine(folderPath, $"{UserName}_features.txt");

        //        using (StreamWriter writer = new StreamWriter(filePath))
        //        {
        //            foreach (var frame in Seq.Frames)
        //            {
        //                string line = string.Join(",", frame.Features.Select(f => f.ToString("F6")));
        //                writer.WriteLine(line);
        //            }
        //        }
        //    }
        //}

        //public static List<(string UserName, Sequence Seq)> LoadFeatures(string folderPath)
        //{
        //    List<(string UserName, Sequence Seq)> loaded = new List<(string, Sequence)>();

        //    if (!Directory.Exists(folderPath))
        //        return loaded;

        //    var files = Directory.GetFiles(folderPath, "*_features.txt");

        //    foreach (var file in files)
        //    {
        //        string userName = Path.GetFileNameWithoutExtension(file).Replace("_features", "");
        //        var lines = File.ReadAllLines(file);
        //        List<MFCCFrame> frames = new List<MFCCFrame>();

        //        foreach (var line in lines)
        //        {
        //            var featureValues = line.Split(',').Select(double.Parse).ToArray();
        //            frames.Add(new MFCCFrame { Features = featureValues });
        //        }

        //        Sequence seq = new Sequence();
        //        seq.Frames = frames.ToArray();
        //        loaded.Add((userName, seq));
        //    }

        //    return loaded;
        //}

    }
}