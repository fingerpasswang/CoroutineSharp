using System;
using System.Collections.Generic;
using System.Linq;
using CoroutineSharp;

namespace Timer.Test
{
    class TestCase
    {
        public static uint CurrentId;

        public TestCase()
        {
            Id = CurrentId++;
        }

        public uint Id;
        public uint Tick;
    }

    class Program
    {
        static void Main(string[] args)
        {
            var firsts = new uint[] {10000, 100000, 1000000, 5000000, 10000000};
            var seconds = new uint[] {0, 100, 1000, 10000};

            foreach (var first in firsts)
            {
                foreach (var second in seconds)
                {
                    var cases = BuildTestCases(first, second).ToList();

                    Console.WriteLine("first:{0} second:{1} count:{2}", first, second, cases.Count);
                    TestAllOnce(cases);
                    Console.WriteLine("\n");
                }
            }
        }

        static void TestAllOnce(List<TestCase> cases)
        {
            var timeManager = new TimeManager();
            Test1(cases, timeManager);
            timeManager = new TimeManager();
            Test1(cases, timeManager);

            var mgr = new TrivialTimeManager();
            Test2(cases, mgr);
            mgr = new TrivialTimeManager();
            Test2(cases, mgr);
        }
        static IEnumerable<TestCase> BuildTestCases(uint first, uint second)
        {
            var rand = new Random();

            for (int i = 0; i < first; i++)
            {
                yield return new TestCase()
                {
                    Tick = (uint)rand.Next(256),
                };
            }

            for (int i = 0; i < 4; i++)
            {
                var begin = 1U << (8 + 6*i);
                var end = 1U << (14 + 6*i);

                for (int j = 0; j < second * (4 - i); j++)
                {
                    yield return new TestCase()
                    {
                        Tick = (uint)rand.Next((int)(begin+end)/2),
                    };
                }
            }
        }
        static void Test1(List<TestCase> cases, TimeManager mgr)
        {
            var maxTick = cases.Max(c => c.Tick);
            var results = new HashSet<uint>();

            foreach (var c in cases)
            {
                TestCase c1 = c;
                mgr.AddTimer(c.Tick, (timer, data) =>
                {
                    if (mgr.FixedTicks == c1.Tick)
                        results.Add((uint) data[0]);
                }, c.Id);
            }

            var begin = DateTime.Now;
            for (int i = 0; i < maxTick+1; i++)
            {
                mgr.FixedTick();
            }
            var end = DateTime.Now;

            Console.WriteLine("TimeManagerCount:{0}, Time:{1}", results.Count, end - begin);
        }
        static void Test2(List<TestCase> cases, TrivialTimeManager mgr)
        {
            var maxTick = cases.Max(c => c.Tick);
            var results = new HashSet<uint>();

            foreach (var c in cases)
            {
                TestCase c1 = c;
                mgr.AddTimer(c.Tick, (timer, data) =>
                {
                    if (mgr.FixedTicks == c1.Tick)
                        results.Add((uint)data[0]);
                }, c.Id);
            }

            var begin = DateTime.Now;
            for (int i = 0; i < maxTick + 1; i++)
            {
                mgr.FixedTick();
            }
            var end = DateTime.Now;

            Console.WriteLine("TrivialTimeManager:{0}, Time:{1}", results.Count, end - begin);
        }
    }
}
