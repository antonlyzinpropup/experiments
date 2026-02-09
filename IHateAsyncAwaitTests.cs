using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Threading;

namespace BusinessRules.Tests
{
    [TestClass]
    public class IHateAsyncAwaitTests
    {
        [TestMethod]
        public void AsyncVsRegularCallTest()
        {
            //A test to compare a generic code with async/await VS proper task usage, vs sync mode
            //The test is measuring 100k requests like async Controller::method -> calls async "Service"::method -> calls few other "async" and await some long-running operations (like db, IO etc)
            var sw = new Stopwatch();

            var tasks = new Task[100000];

            sw.Start();

            for (var i = 0; i < tasks.Length; i++)
            {
                tasks[i] = SimulateSomeWorkAndCallItSelfAsync(150, 10);
            }

            Task.WaitAll(tasks);

            Console.WriteLine("Execution time async: " + sw.Elapsed.TotalSeconds); sw.Start();

            var tasks2 = new Task[100000];

            sw.Restart();

            for (var i = 0; i < tasks2.Length; i++)
            {
                tasks2[i] = SimulateSomeWorkAndCallItSelfAltAsync(150, 10);
            }

            Task.WaitAll(tasks2);

            Console.WriteLine("Execution time async2: " + sw.Elapsed.TotalSeconds); sw.Start();

            sw.Restart();

            Parallel.For(0, 100000, (i) =>
            {
                SimulateSomeWorkAndCallItSelf(150, 10);
            });

            Console.WriteLine("Execution time sync: " + sw.Elapsed.TotalSeconds);

            //Execution time async: 1,26s
            //Execution time async2: 2,29s
            //Execution time sync: ???s
        }

        private static async Task SimulateSomeWorkAndCallItSelfAsync(int msDelay, int moreDeepCalls)
        {
            var subtask = Task.Delay(TimeSpan.FromMilliseconds(msDelay));

            if (moreDeepCalls > 0)
            {
                await SimulateSomeWorkAndCallItSelfAsync(msDelay, moreDeepCalls - 1);
            }

            await subtask;
        }

        private static Task SimulateSomeWorkAndCallItSelfAltAsync(int msDelay, int moreDeepCalls)
        {
            if (moreDeepCalls <= 0)
            {
                return Task.Delay(msDelay);
            }

            return Task.Delay(msDelay)
                .ContinueWith(_ => SimulateSomeWorkAndCallItSelfAltAsync(msDelay, moreDeepCalls - 1), TaskContinuationOptions.ExecuteSynchronously)
                .Unwrap();
        }

        private static void SimulateSomeWorkAndCallItSelf(int msDelay, int moreDeepCalls)
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(msDelay));

            if (moreDeepCalls > 0)
            {
                SimulateSomeWorkAndCallItSelf(msDelay, moreDeepCalls - 1);
            }
        }
    }
}
