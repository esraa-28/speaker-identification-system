
using Recorder.MFCC;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Recorder.Recorder
{
    internal class DTW
    {
        public static double EuclideanDistance(MFCCFrame frameA, MFCCFrame frameB)
        {
            double sum = 0.0;
            for (int i = 0; i < 13; i++)
            {
                double diff = frameA.Features[i] - frameB.Features[i];
                sum += diff * diff;
            }
            return Math.Sqrt(sum);
        }

        public static double ComputeDTWDistance(Sequence input, Sequence template)
        {
            Timer.StartMatching();

            int M = input.Frames.Length;
            int N = template.Frames.Length;

            double[] row0 = new double[N + 1];
            double[] row1 = new double[N + 1];
             
            for (int j = 0; j <= N; j++)
                row0[j] = double.PositiveInfinity;
            row0[0] = 0.0;

            for (int i = 1; i <= M; i++)
            {
                double[] currentRow;
                double[] prevRow;

                if (i % 2 == 1)
                {
                    currentRow = row1;
                    prevRow = row0;
                }
                else
                {
                    currentRow = row0;
                    prevRow = row1;
                }

                currentRow[0] = double.PositiveInfinity;

                for (int j = 1; j <= N; j++)
                {
                    double cost = EuclideanDistance(input.Frames[i - 1], template.Frames[j - 1]);

                    double match = prevRow[j - 1];   // normal match
                    double stretch = prevRow[j];     // template frame stretched
                    double shrink;

                    if (j >= 2)
                        shrink = prevRow[j - 2];     // template frame shrunk
                    else
                        shrink = double.PositiveInfinity;

                    currentRow[j] = cost + Math.Min(match, Math.Min(stretch, shrink));
                }
            }

            double[] finalRow;
            if (M % 2 == 1)
                finalRow = row1;
            else
                finalRow = row0;

            Timer.StopMatching();

            return finalRow[N];
        }

        public static double DTW_With_Pruned(Sequence input, Sequence template, int windowSize)
        {
            Timer.StartMatching();

            int M = input.Frames.Length;
            int N = template.Frames.Length;

            if (windowSize < 2 * Math.Abs(M - N))
                windowSize = 2 * Math.Abs(M - N);

            double[] row0 = new double[N + 1];
            double[] row1 = new double[N + 1];

            for (int j = 0; j <= N; j++)
            {
                row0[j] = double.PositiveInfinity;
                row1[j] = double.PositiveInfinity;
            }
            row0[0] = 0.0;

            for (int i = 1; i <= M; i++)
            {
                double[] currentRow;
                double[] prevRow;

                if (i % 2 == 1)
                {
                    currentRow = row1;
                    prevRow = row0;
                }
                else
                {
                    currentRow = row0;
                    prevRow = row1;
                }

                for (int j = 0; j <= N; j++)
                    currentRow[j] = double.PositiveInfinity;

                int jStart = i - windowSize / 2;
                if (jStart < 1)
                    jStart = 1;

                int jEnd = i + windowSize / 2;
                if (jEnd > N)
                    jEnd = N;

                for (int j = jStart; j <= jEnd; j++)
                {
                    double cost = EuclideanDistance(input.Frames[i - 1], template.Frames[j - 1]);

                    double match = prevRow[j - 1];
                    double stretch = prevRow[j];
                    double shrink;

                    if (j >= 2)
                        shrink = prevRow[j - 2];
                    else
                        shrink = double.PositiveInfinity;

                    currentRow[j] = cost + Math.Min(match, Math.Min(stretch, shrink));
                }
            }

            double[] finalRow;
            if (M % 2 == 1)
                finalRow = row1;
            else
                finalRow = row0;

            Timer.StopMatching();

            return finalRow[N];
        }
        public static double DTW_BeamSearch(Sequence input, Sequence template, int windowSize, double beamThreshold)
        {
            Timer.StartMatching();
            Console.WriteLine($"DTW_BeamSearch called - Template Count: {template.Frames.Length}, Input Count: {input.Frames.Length}");

            int M = input.Frames.Length;
            int N = template.Frames.Length;

            windowSize = Math.Max(windowSize, Math.Abs(M - N));
            int halfWindow = windowSize / 2;

            double[,] costMatrix = new double[3, N + 1];

            for (int j = 0; j <= N; j++)
                costMatrix[0, j] = double.PositiveInfinity;
            costMatrix[0, 0] = 0;

            for (int i = 1; i <= M; i++)
            {
                int current = i % 3;
                int prev1 = (i - 1) % 3;
                int prev2 = (i - 2 + 3) % 3;

                for (int j = 0; j <= N; j++)
                    costMatrix[current, j] = double.PositiveInfinity;

                int diagonalPos = (int)Math.Round((double)i * N / M);
                int start = Math.Max(1, diagonalPos - halfWindow);
                int end = Math.Min(N, diagonalPos + halfWindow);

                double minCostThisRow = double.PositiveInfinity;

                for (int j = start; j <= end; j++)
                {
                    double cost = EuclideanDistance(input.Frames[i - 1], template.Frames[j - 1]);

                    double minPrev = Math.Min(
                        costMatrix[prev1, j],
                        costMatrix[prev1, j - 1]
                    );

                    if (i >= 3 && j >= 2)
                        minPrev = Math.Min(minPrev, costMatrix[prev2, j - 2]);

                    double totalCost = cost + minPrev;
                    costMatrix[current, j] = totalCost;

                    if (totalCost < minCostThisRow)
                        minCostThisRow = totalCost;
                }

                for (int j = start; j <= end; j++)
                {
                    if (costMatrix[current, j] > minCostThisRow + beamThreshold)
                        costMatrix[current, j] = double.PositiveInfinity;
                }
            }
            Timer.StopMatching();
            return costMatrix[M % 3, N];
        }

        public static int TimeSynchronousDTWSearch(Sequence input, List<Sequence> templates)
        {
            if (input == null || templates == null || templates.Count == 0)
                throw new ArgumentException("Input or templates cannot be null or empty.");

            double minDistance = double.PositiveInfinity;
            int bestMatchIndex = -1;

            for (int i = 0; i < templates.Count; i++)
            {
                double distance = ComputeDTWDistance(input, templates[i]);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    bestMatchIndex = i;
                }
            }

            return bestMatchIndex;
        }
    }
}
