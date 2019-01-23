using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace IO
{
    class Program
    {
        public const int height = 1024;
        public const int width = 1024;
        public const int BLOCK = 256;
        public float[][] original = Jaggedgenerate(height, width, true);
        static float[,] generate(int height, int width)
        {
            float[,] tablica = new float[width + 2, height + 2];
            for (int i = 0; i < width + 2; i++) tablica[0, i] = 0;
            for (int i = 0; i < width + 2; i++) tablica[i, 0] = 0;
            for (int i = 0; i < width + 2; i++) tablica[width + 1, i] = 0;
            for (int i = 0; i < width + 2; i++) tablica[i, height + 1] = 0;

            Random random = new Random();
            for (int i = 1; i < width + 1; i++)
            {
                for (int j = 1; j < height + 1; j++)
                {
                    tablica[i, j] = random.Next(255);
                }
            }
            return tablica;
        }

        static float[][] Jaggedgenerate(int height, int width, bool fill)
        {
            float[][] tablica = new float[height + 2][];
            for (int i = 0; i < tablica.Length; i++) tablica[i] = new float[width + 2];

            for (int i = 0; i < width + 2; i++)
            {
                tablica[0][i] = 0;
            }
            for (int i = 0; i < width + 2; i++) tablica[i][0] = 0;
            for (int i = 0; i < width + 2; i++) tablica[width + 1][i] = 0;
            for (int i = 0; i < width + 2; i++) tablica[i][height + 1] = 0;
            if (fill)
            {
                Random random = new Random();
                for (int i = 1; i < width + 1; i++)
                {
                    for (int j = 1; j < height + 1; j++)
                    {
                        tablica[i][j] = random.Next(255);
                    }
                }
            }
            return tablica;
        }

        static void Convolution(float[,] t)
        {
            for (int i = 1; i < width + 1; i++)
            {
                for (int j = 1; j < height + 1; j++)
                {
                    t[i, j] = t[i, j] * 0.6f;
                    t[i, j] += t[i, j + 1] * 0.1f; // ^
                    t[i, j] += t[i, j - 1] * 0.1f; // .
                    t[i, j] += t[i + 1, j] * 0.1f; // >
                    t[i, j] += t[i - 1, j] * 0.1f; // <
                }
            }
        }

        static void JaggedConvolution(float[][] t)
        {
            for (int i = 1; i < width + 1; i++)
            {
                for (int j = 1; j < height + 1; j++)
                {
                    t[i][j] = t[i][j] * 0.6f + (t[i][j + 1] + t[i][j - 1] + t[i + 1][j] + t[i - 1][j]) * 0.1f;
                }
            }
        }

        static void OptimizedConvolution(float[,] t)
        {
            float middle, top, bottom, right, left;
            for (int i = 1; i < width + 1; i++)
            {
                for (int j = 1; j < height + 1; j++)
                {
                    middle = top = bottom = right = left = 0;
                    middle = t[i, j] * 0.6f;
                    top = t[i, j + 1] * 0.1f; // ^
                    bottom = t[i, j - 1] * 0.1f; // .
                    right = t[i + 1, j] * 0.1f; // >
                    left = t[i - 1, j] * 0.1f; // <
                    t[i, j] = middle + top + bottom + right + left;
                }
            }
        }

        static void ProcessImage(float[,] t)
        {
            var tasks = new[]
            {
                Task.Run(()=>OptimizedConvolution(t))
                };
            Task.WaitAll(tasks);
        }

        static void OptimizedJaggedConvolution(float[][] original, int x, int y)
        {
            {
                for (int i = 1 + x * BLOCK; i < x * BLOCK + BLOCK + 1; i++)
                {
                    for (int j = 1 + y * BLOCK; j < y * BLOCK + BLOCK; j++)
                    {
                        original[i][j] = original[i][j] * 0.6f + (original[i][j + 1] + original[i][j - 1] + original[i + 1][j] + original[i - 1][j]) * 0.1f;
                    }
                }
            }
        }

        static void JaggedProcessImage(float[][] original)
        {
            var tasks = new[]
            {
                Task.Run(()=>OptimizedJaggedConvolution(original,0,0)),
                Task.Run(()=>OptimizedJaggedConvolution(original,1,0)),
                Task.Run(()=>OptimizedJaggedConvolution(original,2,0)),
                Task.Run(()=>OptimizedJaggedConvolution(original,3,0)),
                Task.Run(()=>OptimizedJaggedConvolution(original,0,1)),
                Task.Run(()=>OptimizedJaggedConvolution(original,1,1)),
                Task.Run(()=>OptimizedJaggedConvolution(original,2,1)),
                Task.Run(()=>OptimizedJaggedConvolution(original,3,1)),
                Task.Run(()=>OptimizedJaggedConvolution(original,0,2)),
                Task.Run(()=>OptimizedJaggedConvolution(original,1,2)),
                Task.Run(()=>OptimizedJaggedConvolution(original,2,2)),
                Task.Run(()=>OptimizedJaggedConvolution(original,3,2)),
                Task.Run(()=>OptimizedJaggedConvolution(original,0,3)),
                Task.Run(()=>OptimizedJaggedConvolution(original,1,3)),
                Task.Run(()=>OptimizedJaggedConvolution(original,2,3)),
                Task.Run(()=>OptimizedJaggedConvolution(original,3,3)),
                };
            Task.WaitAll(tasks);
        }

        static void Main(string[] args)
        {
            int x = 0;
            long time = 0;
            while (x < 10)
            {
                // Convolution
                float[,] t = generate(height, width);
                var watch = System.Diagnostics.Stopwatch.StartNew();
                for (int i = 0; i < 200; i++) Convolution(t);
                watch.Stop();
                var elapsedMS = watch.ElapsedMilliseconds;
                Console.WriteLine("Convolution:" + elapsedMS);
                Console.WriteLine("");

                // OptimizedConvolution
                float[,] t1 = generate(height, width);
                var watch1 = System.Diagnostics.Stopwatch.StartNew();
                for (int i = 0; i < 200; i++) ProcessImage(t1);
                watch1.Stop();
                var elapsedMS1 = watch1.ElapsedMilliseconds;
                Console.WriteLine("OptimizedConvolution:" + elapsedMS1);
                Console.WriteLine("");

                // JaggedConvolution
                float[][] jaggedTable = Jaggedgenerate(height, width, true);
                var copy = jaggedTable.Select(a => a.ToArray()).ToArray();
                var watch3 = System.Diagnostics.Stopwatch.StartNew();
                for (int i = 0; i < 200; i++) JaggedConvolution(jaggedTable);
                watch.Stop();
                var elapsedMS3 = watch3.ElapsedMilliseconds;
                Console.WriteLine("JaggedConvolution:" + elapsedMS3);
                Console.WriteLine("");

                // OptimizedJaggedConvolution
                float[][] jaggedTask = Jaggedgenerate(height, width, true);
                var watch4 = System.Diagnostics.Stopwatch.StartNew();
                for (int i = 0; i < 200; i++) JaggedProcessImage(copy);
                watch4.Stop();
                var elapsedMS4 = watch4.ElapsedMilliseconds;
                Console.WriteLine("OptimizedJaggedConvolution:" + elapsedMS4);
                Console.WriteLine("=======================================");
                Console.WriteLine(jaggedTable[100][100]);
                Console.WriteLine(copy[100][100]);
                Console.WriteLine("=======================================");
                time = time + elapsedMS4;
                x++;
            }
            Console.WriteLine(time);
        }
    }
}
