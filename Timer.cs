//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Recorder
//{
//    public static class Timer
//    {
//        // Stopwatches for different operations
//        private static readonly Stopwatch featureExtractionStopwatch = new Stopwatch();
//        private static readonly Stopwatch matchingStopwatch = new Stopwatch();
//        private static readonly Stopwatch loadingStopwatch = new Stopwatch();
//        private static readonly Stopwatch totalOperationStopwatch = new Stopwatch();

//        // Performance metrics
//        public static double FeatureExtractionTime { get; private set; }
//        public static double MatchingTime { get; private set; }
//        public static double LoadingTime { get; private set; }
//        public static double TotalOperationTime { get; private set; }

//        // Operation counters for debugging
//        public static int FeatureExtractionCalls { get; private set; }
//        public static int MatchingCalls { get; private set; }
//        public static int LoadingCalls { get; private set; }

//        // Thread safety locks
//        private static readonly object featureLock = new object();
//        private static readonly object matchingLock = new object();
//        private static readonly object loadingLock = new object();
//        private static readonly object totalLock = new object();

//        /// <summary>
//        /// Start timing the feature extraction process
//        /// </summary>
//        public static void StartFeatureExtraction()
//        {
//            featureExtractionStopwatch.Reset();
//            featureExtractionStopwatch.Start();
//        }

//        /// <summary>
//        /// Stop timing the feature extraction process
//        /// </summary>
//        public static void StopFeatureExtraction()
//        {
//            featureExtractionStopwatch.Stop();
//            FeatureExtractionTime = featureExtractionStopwatch.ElapsedMilliseconds / 1000.0;
//        }

//        /// <summary>
//        /// Start timing the matching process
//        /// </summary>
//        public static void StartMatching()
//        {
//            matchingStopwatch.Reset();
//            matchingStopwatch.Start();
//        }

//        /// <summary>
//        /// Stop timing the matching process
//        /// </summary>
//        public static void StopMatching()
//        {
//            matchingStopwatch.Stop();
//            MatchingTime = matchingStopwatch.ElapsedMilliseconds / 1000.0;
//        }

//        /// <summary>
//        /// Start timing the loading process
//        /// </summary>
//        public static void StartLoading()
//        {
//            //loadingStopwatch.Reset();
//            loadingStopwatch.Start();
//        }

//        /// <summary>
//        /// Stop timing the loading process
//        /// </summary>
//        public static void StopLoading()
//        {
//            loadingStopwatch.Stop();
//            LoadingTime = loadingStopwatch.ElapsedMilliseconds / 1000.0;
//        }

//        /// <summary>
//        /// Start timing the entire operation
//        /// </summary>
//        public static void StartTotalOperation()
//        {
//            totalOperationStopwatch.Reset();
//            totalOperationStopwatch.Start();
//        }

//        /// <summary>
//        /// Stop timing the entire operation
//        /// </summary>
//        public static void StopTotalOperation()
//        {
//            totalOperationStopwatch.Stop();
//            TotalOperationTime = totalOperationStopwatch.ElapsedMilliseconds / 1000.0;
//        }

//        /// <summary>
//        /// Reset all timers
//        /// </summary>
//        public static void ResetAll()
//        {
//            featureExtractionStopwatch.Reset();
//            matchingStopwatch.Reset();
//            loadingStopwatch.Reset();
//            totalOperationStopwatch.Reset();

//            FeatureExtractionTime = 0;
//            MatchingTime = 0;
//            LoadingTime = 0;
//            TotalOperationTime = 0;
//        }

//        /// <summary>
//        /// Get a formatted summary of all timing information
//        /// </summary>
//        public static string GetTimingSummary()
//        {
//            return $"Performance Metrics:\n" +
//                   $"- Loading Time: {LoadingTime:F3} seconds\n" +
//                   $"- Feature Extraction Time: {FeatureExtractionTime:F3} seconds\n" +
//                   $"- Matching Time: {MatchingTime:F3} seconds\n" +
//                   $"- Total Operation Time: {TotalOperationTime:F3} seconds";
//        }
//    }
//}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recorder
{
    public static class Timer
    {
        // Stopwatches for different operations
        private static readonly Stopwatch featureExtractionStopwatch = new Stopwatch();
        private static readonly Stopwatch matchingStopwatch = new Stopwatch();
        private static readonly Stopwatch loadingStopwatch = new Stopwatch();
        private static readonly Stopwatch totalOperationStopwatch = new Stopwatch();

        // Performance metrics
        public static double FeatureExtractionTime { get; private set; }
        public static double MatchingTime { get; private set; }
        public static double LoadingTime { get; private set; }
        public static double TotalOperationTime { get; private set; }

        // Operation counters for debugging
        public static int FeatureExtractionCalls { get; private set; }
        public static int MatchingCalls { get; private set; }
        public static int LoadingCalls { get; private set; }

        // Thread safety locks
        private static readonly object featureLock = new object();
        private static readonly object matchingLock = new object();
        private static readonly object loadingLock = new object();
        private static readonly object totalLock = new object();

        /// <summary>
        /// Start timing the feature extraction process
        /// </summary>
        public static void StartFeatureExtraction()
        {
            lock (featureLock)
            {
                if (!featureExtractionStopwatch.IsRunning)
                {
                    featureExtractionStopwatch.Start();
                    Debug.WriteLine($"[Timer] Started Feature Extraction timing");
                }
                else
                {
                    Debug.WriteLine("[Timer] Warning: Feature extraction timer already running");
                }
            }
        }

        /// <summary>
        /// Stop timing the feature extraction process
        /// </summary>
        public static void StopFeatureExtraction()
        {
            lock (featureLock)
            {
                if (featureExtractionStopwatch.IsRunning)
                {
                    featureExtractionStopwatch.Stop();
                    double elapsed = featureExtractionStopwatch.Elapsed.TotalSeconds;
                    FeatureExtractionTime += elapsed;
                    FeatureExtractionCalls++;

                    Debug.WriteLine($"[Timer] Feature Extraction: {elapsed:F3}s (Total: {FeatureExtractionTime:F3}s, Calls: {FeatureExtractionCalls})");

                    featureExtractionStopwatch.Reset();
                }
                else
                {
                    Debug.WriteLine("[Timer] Warning: Feature extraction timer not running");
                }
            }
        }

        /// <summary>
        /// Start timing the matching process
        /// </summary>
        public static void StartMatching()
        {
            lock (matchingLock)
            {
                if (!matchingStopwatch.IsRunning)
                {
                    matchingStopwatch.Start();
                    Debug.WriteLine($"[Timer] Started Matching timing");
                }
                else
                {
                    Debug.WriteLine("[Timer] Warning: Matching timer already running");
                }
            }
        }

        /// <summary>
        /// Stop timing the matching process
        /// </summary>
        public static void StopMatching()
        {
            lock (matchingLock)
            {
                if (matchingStopwatch.IsRunning)
                {
                    matchingStopwatch.Stop();
                    double elapsed = matchingStopwatch.Elapsed.TotalSeconds;
                    MatchingTime += elapsed;
                    MatchingCalls++;

                    Debug.WriteLine($"[Timer] Matching: {elapsed:F3}s (Total: {MatchingTime:F3}s, Calls: {MatchingCalls})");

                    matchingStopwatch.Reset();
                }
                else
                {
                    Debug.WriteLine("[Timer] Warning: Matching timer not running");
                }
            }
        }

        /// <summary>
        /// Start timing the loading process
        /// </summary>
        public static void StartLoading()
        {
            lock (loadingLock)
            {
                if (!loadingStopwatch.IsRunning)
                {
                    loadingStopwatch.Start();
                    Debug.WriteLine($"[Timer] Started Loading timing");
                }
                else
                {
                    Debug.WriteLine("[Timer] Warning: Loading timer already running");
                }
            }
        }

        /// <summary>
        /// Stop timing the loading process
        /// </summary>
        public static void StopLoading()
        {
            lock (loadingLock)
            {
                if (loadingStopwatch.IsRunning)
                {
                    loadingStopwatch.Stop();
                    double elapsed = loadingStopwatch.Elapsed.TotalSeconds;
                    LoadingTime += elapsed;
                    LoadingCalls++;

                    Debug.WriteLine($"[Timer] Loading: {elapsed:F3}s (Total: {LoadingTime:F3}s, Calls: {LoadingCalls})");

                    loadingStopwatch.Reset();
                }
                else
                {
                    Debug.WriteLine("[Timer] Warning: Loading timer not running");
                }
            }
        }

        /// <summary>
        /// Start timing the entire operation
        /// </summary>
        public static void StartTotalOperation()
        {
            lock (totalLock)
            {
                totalOperationStopwatch.Reset();
                totalOperationStopwatch.Start();
                Debug.WriteLine("[Timer] Started Total Operation timing");
            }
        }

        /// <summary>
        /// Stop timing the entire operation
        /// </summary>
        public static void StopTotalOperation()
        {
            lock (totalLock)
            {
                if (totalOperationStopwatch.IsRunning)
                {
                    totalOperationStopwatch.Stop();
                    TotalOperationTime = totalOperationStopwatch.Elapsed.TotalSeconds;
                    Debug.WriteLine($"[Timer] Total Operation completed: {TotalOperationTime:F3}s");
                }
                else
                {
                    Debug.WriteLine("[Timer] Warning: Total operation timer not running");
                }
            }
        }

        /// <summary>
        /// Reset all timers
        /// </summary>
        public static void ResetAll()
        {
            lock (featureLock)
                lock (matchingLock)
                    lock (loadingLock)
                        lock (totalLock)
                        {
                            // Stop all running timers
                            if (featureExtractionStopwatch.IsRunning) featureExtractionStopwatch.Stop();
                            if (matchingStopwatch.IsRunning) matchingStopwatch.Stop();
                            if (loadingStopwatch.IsRunning) loadingStopwatch.Stop();
                            if (totalOperationStopwatch.IsRunning) totalOperationStopwatch.Stop();

                            // Reset stopwatches
                            featureExtractionStopwatch.Reset();
                            matchingStopwatch.Reset();
                            loadingStopwatch.Reset();
                            totalOperationStopwatch.Reset();

                            // Reset cumulative times
                            FeatureExtractionTime = 0;
                            MatchingTime = 0;
                            LoadingTime = 0;
                            TotalOperationTime = 0;

                            // Reset counters
                            FeatureExtractionCalls = 0;
                            MatchingCalls = 0;
                            LoadingCalls = 0;

                            Debug.WriteLine("[Timer] All timers reset");
                        }
        }

        /// <summary>
        /// Get current status of all timers
        /// </summary>
        public static string GetTimerStatus()
        {
            return $"Timer Status:\n" +
                   $"- Feature Extraction: {(featureExtractionStopwatch.IsRunning ? "RUNNING" : "STOPPED")} " +
                   $"(Total: {FeatureExtractionTime:F3}s, Calls: {FeatureExtractionCalls})\n" +
                   $"- Matching: {(matchingStopwatch.IsRunning ? "RUNNING" : "STOPPED")} " +
                   $"(Total: {MatchingTime:F3}s, Calls: {MatchingCalls})\n" +
                   $"- Loading: {(loadingStopwatch.IsRunning ? "RUNNING" : "STOPPED")} " +
                   $"(Total: {LoadingTime:F3}s, Calls: {LoadingCalls})\n" +
                   $"- Total Operation: {(totalOperationStopwatch.IsRunning ? "RUNNING" : "STOPPED")} " +
                   $"(Time: {TotalOperationTime:F3}s)";
        }

        /// <summary>
        /// Get a formatted summary of all timing information
        /// </summary>
        public static string GetTimingSummary()
        {
            return $"Performance Metrics:\n" +
                   $"- Loading Time: {LoadingTime:F3} seconds ({LoadingCalls} operations)\n" +
                   $"- Feature Extraction Time: {FeatureExtractionTime:F3} seconds ({FeatureExtractionCalls} operations)\n" +
                   $"- Matching Time: {MatchingTime:F3} seconds ({MatchingCalls} operations)\n" +
                   $"- Total Operation Time: {TotalOperationTime:F3} seconds\n" +
                   $"- Unaccounted Time: {Math.Max(0, TotalOperationTime - (LoadingTime + FeatureExtractionTime + MatchingTime)):F3} seconds";
        }

        /// <summary>
        /// Get detailed timing breakdown as percentages
        /// </summary>
        public static string GetTimingBreakdown()
        {
            if (TotalOperationTime == 0) return "No timing data available";

            double loadingPercent = (LoadingTime / TotalOperationTime) * 100;
            double featurePercent = (FeatureExtractionTime / TotalOperationTime) * 100;
            double matchingPercent = (MatchingTime / TotalOperationTime) * 100;
            double unaccountedTime = Math.Max(0, TotalOperationTime - (LoadingTime + FeatureExtractionTime + MatchingTime));
            double unaccountedPercent = (unaccountedTime / TotalOperationTime) * 100;

            return $"Timing Breakdown (Total: {TotalOperationTime:F3}s):\n" +
                   $"- Loading: {LoadingTime:F3}s ({loadingPercent:F1}%)\n" +
                   $"- Feature Extraction: {FeatureExtractionTime:F3}s ({featurePercent:F1}%)\n" +
                   $"- Matching: {MatchingTime:F3}s ({matchingPercent:F1}%)\n" +
                   $"- Other/Unaccounted: {unaccountedTime:F3}s ({unaccountedPercent:F1}%)";
        }

        /// <summary>
        /// Log current timing status to console
        /// </summary>
        public static void LogCurrentStatus()
        {
            Console.WriteLine("=== TIMER STATUS ===");
            Console.WriteLine(GetTimerStatus());
            Console.WriteLine();
            Console.WriteLine("=== TIMING SUMMARY ===");
            Console.WriteLine(GetTimingSummary());
            Console.WriteLine();
            Console.WriteLine("=== TIMING BREAKDOWN ===");
            Console.WriteLine(GetTimingBreakdown());
            Console.WriteLine("==================");
        }

        /// <summary>
        /// Measure execution time of an action
        /// </summary>
        public static double MeasureAction(Action action, string actionName = "Action")
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                action();
            }
            finally
            {
                stopwatch.Stop();
            }

            double elapsed = stopwatch.Elapsed.TotalSeconds;
            Debug.WriteLine($"[Timer] {actionName} took {elapsed:F3} seconds");
            return elapsed;
        }

        /// <summary>
        /// Measure execution time of a function
        /// </summary>
        public static T MeasureFunction<T>(Func<T> function, out double elapsedSeconds, string functionName = "Function")
        {
            var stopwatch = Stopwatch.StartNew();
            T result;
            try
            {
                result = function();
            }
            finally
            {
                stopwatch.Stop();
            }

            elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
            Debug.WriteLine($"[Timer] {functionName} took {elapsedSeconds:F3} seconds");
            return result;
        }
    }
}