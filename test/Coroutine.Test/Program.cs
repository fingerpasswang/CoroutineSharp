using System;
using System.Collections;
using System.Threading;
using CoroutineSharp;

namespace Coroutine.Test
{
    class Program
    {
        private static TimeManager timeManager;
        private static CoroutineManager coroutineManager;

        static void Main(string[] args)
        {
            timeManager = new TimeManager();
            coroutineManager = new CoroutineManager(timeManager);

            TestCoroutine();
        }

        static void TestCoroutine()
        {
            Console.WriteLine(DateTime.Now);

            var coroutine = coroutineManager.StartCoroutine(Routine2(100));

            int n = 0;
            while (true)
            {
                //Console.WriteLine("tick:{0}", timeManager.FixedTicks);
                timeManager.Update();

                Thread.Sleep(100);
                n++;
                if (n == 20)
                {
                    Console.WriteLine("stop");
                    coroutine.Stop();
                }
            }
        }

        static IEnumerator Routine(float time)
        {
            yield return new WaitForSeconds(time);

            Console.WriteLine("end");
            Console.WriteLine(DateTime.Now);
        }

        static IEnumerator Routine2(int seconds)
        {
            while (seconds > 0)
            {
                yield return new WaitForSeconds(0.5f);

                Console.WriteLine("once now:{0}", DateTime.Now);
                seconds--;
            }
            
            Console.WriteLine("end");
            Console.WriteLine(DateTime.Now);
        }
    }
}
