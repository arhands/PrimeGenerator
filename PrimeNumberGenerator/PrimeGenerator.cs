using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;

namespace PrimeNumberGenerator
{
    class PrimeGenerator
    {
        /// <summary>
        /// this is the span from the starting number to the ending number for example,
        /// if the span was 10 then the first set would be {2,3,5,7}
        /// </summary>
        public const int SetNumberSpan = 1000000;
        public const string SaveLocation = "C:\\tmp\\primes\\";
        public static List<long> _firstPrimeSet;
        public bool Finished { get; private set; }
        public PrimeSet Primes { get; private set; }
        System.Threading.Thread _thread;
        public void CollectBlock(int block)
        {
            _thread = new System.Threading.Thread(() =>
            {
                Finished = false;
                //setting span for generation
                long start = SetNumberSpan * block, end = start + SetNumberSpan;
                //generating primes
                var primes = new PrimeSet();
                primes.AverageGenerationTime = (((DateTime.Now.Day * 24 + DateTime.Now.Hour) * 60 + DateTime.Now.Minute) * 60 + DateTime.Now.Second) * 1000 + DateTime.Now.Millisecond;
                primes.Primes = new List<long>();  
                for (long j = start; j <= end; j++)
                {
                    bool prime = true;
                    for (int h = 0; _firstPrimeSet[h] * _firstPrimeSet[h] <= j; h++)
                    {
                        if (j % _firstPrimeSet[h] == 0)
                        {
                            prime = false;
                            break;
                        }
                    }
                    if (prime)
                        primes.Primes.Add(j);
                }
                primes.AverageGenerationTime = (((DateTime.Now.Day * 24 + DateTime.Now.Hour) * 60 + DateTime.Now.Minute) * 60 + DateTime.Now.Second) * 1000 + DateTime.Now.Millisecond - primes.AverageGenerationTime;
                primes.TotalGenerationTime = primes.AverageGenerationTime;
                primes.AverageGenerationTime = primes.AverageGenerationTime / primes.Primes.Count;
                Finished = true;
            });
            _thread.Start();
        }
        public static List<long> LoadPrimes(int title)
        {

            List<long> primes = new List<long>();
            string[] text = File.ReadAllLines(SaveLocation + title + ".csv", Encoding.ASCII);
            for (int i = 1; i < text.Length; i++)
            {
                string number = text[i].Substring(1);
                number = number.Substring(0, number.IndexOf("\""));
                primes.Add(Convert.ToInt64(number));
            }
            return primes;
        }
        public static PrimeSet GenerateInitialPrimeSet()
        {
            PrimeSet primes = new PrimeSet();
            int dt = (((DateTime.Now.Day * 24 + DateTime.Now.Hour) * 60 + DateTime.Now.Minute) * 60 + DateTime.Now.Second) * 1000 + DateTime.Now.Millisecond;
            primes.Primes = _firstPrimeSet = new List<long>();
            primes.Primes.Add(2);
            for (long j = 3; j <= SetNumberSpan; j++)
            {
                bool prime = true;
                for (int h = 0; _firstPrimeSet[h] * _firstPrimeSet[h] <= j; h++)
                {
                    if (j % _firstPrimeSet[h] == 0)
                    {
                        prime = false;
                        break;
                    }
                }
                if (prime)
                    primes.Primes.Add(j);
            }
            dt = (((DateTime.Now.Day * 24 + DateTime.Now.Hour) * 60 + DateTime.Now.Minute) * 60 + DateTime.Now.Second) * 1000 + DateTime.Now.Millisecond - dt;
            primes.TotalGenerationTime = dt;
            primes.AverageGenerationTime = dt / primes.Primes.Count;
            return primes;

        }
    }
    public class PrimeSet
    {
        public List<long> Primes { get; set; }
        /// <summary>
        /// this is in milliseconds
        /// </summary>
        public double TotalGenerationTime { get; set; }
        /// <summary>
        /// this is in milliseconds
        /// </summary>
        public double AverageGenerationTime { get; set; }
    }
}
