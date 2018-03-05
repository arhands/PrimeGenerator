using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeNumberGenerator
{
    class PartialSieve : PrimeProc
    {
        public PartialSieve(PrimeProc child)
        {
            Child = child;
        }
        static int Next { get; set; }
        static long LastPrime { get; set; }
        public static void Initialize()
        {
            Next = 0;
            for (; Next < 10 * 1000; Next++)
                if (!File.Exists(SaveLocation + Next + ".csv"))
                    break;
            if (Next == 0)
            {
                FirstMillionPrimes = new long[1000 * 1000];
                FirstMillionPrimes[0] = 2;
                int lastPrime = 2;
                for (int i = 1; i < FirstMillionPrimes.Length; i++)
                {
                    lastPrime++;
                    for (int j = 0; FirstMillionPrimes[j] * FirstMillionPrimes[j] <= lastPrime; j++)
                    {
                        if (lastPrime % FirstMillionPrimes[j] == 0)
                        {
                            lastPrime++;
                            j = -1;
                        }
                    }
                    FirstMillionPrimes[i] = lastPrime;
                }
                Save(FirstMillionPrimes.ToList(), 0, 0);
                Next++;
                LastPrime = FirstMillionPrimes.Last();
            }
            else
            {
                FirstMillionPrimes = LoadPrimes(0).ToArray();
                LastPrime = LoadPrimes(Next - 1).Last();
            }
        }
        public const string SaveLocation = "C:\\tmp\\primes\\";
        public static long[] FirstMillionPrimes { get; set; }
        public PrimeNumberBlock Input { get; set; }
        PrimeProc Child { get; set; }
        System.Threading.Thread Thread { get; set; }
        public bool IsReadyForNextSet { get; set; }
        public void Run()
        {
            Thread = new System.Threading.Thread(() =>
            {
                IsReadyForNextSet = true;
                while (Input == null) { System.Threading.Thread.Sleep(1); }

                while (true)
                {
                    PrimeNumberBlock newSet = new PrimeNumberBlock();
                    for (int i = 0; i < Input.Primes.Count; i++)
                        newSet.Primes.Add(Input.Primes[i]);
                    newSet.FirstNonAbsolutePrimeIndex = Input.FirstNonAbsolutePrimeIndex;

                    for (int i = newSet.NextPrimeIndex; i < 1000000; i++)
                    {
                        for (int j = newSet.FirstNonAbsolutePrimeIndex; j < newSet.Primes.Count; j++)
                        {
                            if (newSet.Primes[j] % FirstMillionPrimes[i] == 0)
                                newSet.Primes.RemoveAt(j);
                            else if (FirstMillionPrimes[i] * FirstMillionPrimes[i] >= newSet.Primes[j])
                            {
                                newSet.Primes.Add(Input.Primes[j]);
                                newSet.FirstNonAbsolutePrimeIndex++;
                            }
                        }
                        if (Child.IsReadyForNextSet)
                        {
                            newSet.NextPrimeIndex = i + 1;
                            Child.Input = newSet;
                            Input = null;
                            Child.IsReadyForNextSet = false;
                            IsReadyForNextSet = true;
                            while (Input == null) { System.Threading.Thread.Sleep(1); }
                            break;
                        }
                    }
                }
            });
            Thread.Start();
        }
        const int ProcBlockSize = 1000;
        const int SaveBlockSize = 1000 * 1000;
        /// <summary>
        /// this will be the first thread such that its parent = null
        /// </summary>
        public void Run_First()
        {
            Thread = new System.Threading.Thread(() =>
            {
                long NextNumber = LastPrime + 1;
                while (NextNumber >= 0)
                {
                    PrimeNumberBlock newSet = new PrimeNumberBlock();
                    newSet.Primes = new List<long>(ProcBlockSize);
                    for (int i = 1; i <= ProcBlockSize; i++)
                        newSet.Primes.Add(i + NextNumber);
                    NextNumber += ProcBlockSize;
                    newSet.FirstNonAbsolutePrimeIndex = 0;
                    bool tooFast = true;
                    for (int i = 0; i < FirstMillionPrimes.Length; i++)
                    {
                        for (int j = newSet.FirstNonAbsolutePrimeIndex; j < newSet.Primes.Count; j++)
                        {
                            if (newSet.Primes[j] % FirstMillionPrimes[i] == 0)
                            {
                                newSet.Primes.RemoveAt(j);
                                j--;
                            }
                            else if (FirstMillionPrimes[i] * FirstMillionPrimes[i] >= newSet.Primes[j])
                            {
                                newSet.FirstNonAbsolutePrimeIndex++;
                            }
                        }
                        if (Child.IsReadyForNextSet)
                        {
                            newSet.NextPrimeIndex = i + 1;
                            Child.Input = newSet;
                            while (Child.IsReadyForNextSet)
                            {
                                System.Threading.Thread.Sleep(1);
                            }
                            tooFast = false;
                            break;
                        }

                    }
                    if (tooFast)
                    {
                        while (!Child.IsReadyForNextSet) { System.Threading.Thread.Sleep(1); }
                        newSet.NextPrimeIndex = newSet.Primes.Count;
                        Child.Input = newSet;
                        while (Child.IsReadyForNextSet) { System.Threading.Thread.Sleep(1); }
                    }
                }
            });
            Thread.Start();
        }
        /// <summary>
        /// this will be the last thread such that Child = null
        /// </summary>
        public void Run_Last()
        {
            Thread = new System.Threading.Thread(() =>
            {
                PrimeNumberBlock newSet = new PrimeNumberBlock();
                while (true)
                {
                    IsReadyForNextSet = true;
                    while (Input == null) { System.Threading.Thread.Sleep(1); }
                    int oldLength = newSet.Primes.Count;
                    for (int i = 0; i < Input.Primes.Count; i++)
                        newSet.Primes.Add(Input.Primes[i]);
                    newSet.FirstNonAbsolutePrimeIndex = oldLength + Input.FirstNonAbsolutePrimeIndex;
                    newSet.NextPrimeIndex = Input.NextPrimeIndex;
                    Input = null;
                    IsReadyForNextSet = false;

                    for (int i = newSet.FirstNonAbsolutePrimeIndex; i < newSet.Primes.Count; i++)
                    {
                        for (int j = newSet.NextPrimeIndex; j < FirstMillionPrimes.Length; j++)
                        {
                            if (FirstMillionPrimes[j] * FirstMillionPrimes[j] > newSet.Primes[i])
                                break;
                            if (newSet.Primes[i] % FirstMillionPrimes[j] == 0)
                            {
                                newSet.Primes.RemoveAt(i);
                                i--;
                                break;
                            }
                        }

                    }
                    if (newSet.Primes.Count >= SaveBlockSize)
                    {
                        Save(newSet.Primes.GetRange(0, SaveBlockSize), Next * SaveBlockSize, Next);
                        Next++;
                        newSet.Primes.RemoveRange(0, SaveBlockSize);
                    }
                }
            });
            Thread.Start();
        }
        static void Save(List<long> primes, long firstPrimeIndex, int name)
        {
            string[] text = new string[primes.Count + 1];
            text[0] = "\"Index\", \" Prime \"";
            for (int i = 0; i < primes.Count; i++)
            {
                text[i + 1] = "\"" + (firstPrimeIndex + i) + "\", \"" + primes[i] + "\"";
            }
            File.WriteAllLines(SaveLocation + name + ".csv", text);
        }
        public static List<long> LoadPrimes(int title)
        {

            List<long> primes = new List<long>();
            string[] text = File.ReadAllLines(SaveLocation + title + ".csv", Encoding.ASCII);
            for (int i = 1; i < text.Length; i++)
            {
                string number = text[i].Substring(text[i].IndexOf(",") + 3);
                number = number.Substring(0, number.IndexOf("\""));
                primes.Add(Convert.ToInt64(number));
            }
            return primes;
        }
    }
    class PrimeNumberBlock
    {
        public PrimeNumberBlock()
        {
            Primes = new List<long>();
        }
        public List<long> Primes { get; set; }
        /// <summary>
        /// index for the first number in the next thread to test
        /// </summary>
        public int NextPrimeIndex { get; set; }
        public int FirstNonAbsolutePrimeIndex { get; set; }
    }
    interface PrimeProc
    {
        /// <summary>
        /// when the child is ready for the next prime block, it will set this to true
        /// </summary>
        bool IsReadyForNextSet { get; set; }
        PrimeNumberBlock Input { get; set; }
    }
}
