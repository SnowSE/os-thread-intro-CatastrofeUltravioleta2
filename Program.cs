using System.Diagnostics;

namespace os_first_thread_demo;

class Program
{
    static ulong globalCounter = 0;
    static object x = new object(); //for locking
    static System.Collections.Concurrent.ConcurrentQueue<string> cq = new();
    static System.Collections.Concurrent.ConcurrentStack<string> cs = new();
    static System.Collections.Concurrent.ConcurrentBag<string> cb = new();

    static void Main(string[] args)
    {
        Console.WriteLine("Demo - how threads behave (using counting as a workload)");
        //Low-Level Thread Interaction
        Unprotected(100_000);
        UsingInterlocked(100_000);
        UsingLock(100_000);
        UsingMonitor(100_000_00);
        UsingMonitorTimed(100_000_00);

        //TPL (Task Parallel Library)
        unsafeVarThreads(500_000); //Can have variable # of threads in a collection
        UnsafeParForEachVarThreads(500_000); //can start threads using ParallelForeach
        SafeParForEachVarThreads(500_000);
        TaskBasedUnsafeCounter(500_000);
        TaskBasedILCounter(500_000);
        TaskBasedLCounter(500_000);
        ParallelInvokeUnsafeCounter(500_000);
        ParallelInvokeILCounter(500_000);
        ParallelInvokeLCounter(500_000);

        //concurrent datatype
        TowelSimulationFIFO(10);
        TowelSimulationLIFO(10);

        //FUTURE:
        //Cancelation Tokens
        //Semaphore / manual reset events

    }
    static void TowelSimulationLIFO(ulong makeTo)
    {
        List<Task> tasks = new();
        Stopwatch sw = Stopwatch.StartNew();
        tasks.Add(new Task(() => TaskToCreateTowelsLIFO(makeTo)));
        tasks.Add(new Task(() => TaskToConsumeTowelsLIFO(makeTo)));
        Parallel.ForEach(tasks, task => task.Start());
        Task.WaitAll(tasks.ToArray());
        Console.WriteLine($"TowelLIFO done.  Time Elapsed: {sw.Elapsed}");
    }
    static void TowelSimulationFIFO(ulong makeTo)
    {
        List<Task> tasks = new();
        Stopwatch sw = Stopwatch.StartNew();
        tasks.Add(new Task(() => TaskToCreateTowelsFIFO(makeTo)));
        tasks.Add(new Task(() => TaskToConsumeTowelsFIFO(makeTo)));
        Parallel.ForEach(tasks, task => task.Start());
        Task.WaitAll(tasks.ToArray());
        Console.WriteLine($"TowelFIFO done.  Time Elapsed: {sw.Elapsed}");
    }
    static Task<int> TaskToCreateTowelsLIFO(ulong makeTo)
    {
        for (ulong i = 1; i <= makeTo; i++)
        {
            cs.Push($"Towel {i}");
            Console.WriteLine($"I made Towel {i}");
        }
        return Task.FromResult(0);
    }
    static Task<int> TaskToConsumeTowelsLIFO(ulong makeTo)
    {
        for (ulong i = 1; i <= makeTo; i++)
        {
            if (cs.TryPop(out string? towel))
            {
                Console.WriteLine($"I used {towel??"<no towel>"}");
            }
            else { Console.WriteLine("No towel available"); i--; }
        }
        return Task.FromResult(0);
    }

    static Task<int> TaskToCreateTowelsFIFO(ulong makeTo)
    {
        for (ulong i = 1; i <= makeTo; i++)
        {
            cq.Enqueue($"Towel {i}");
            Console.WriteLine($"I made Towel {i}");
        }
        return Task.FromResult(0);
    }
    static Task<int> TaskToConsumeTowelsFIFO(ulong makeTo)
    {
        for (ulong i = 1; i <= makeTo; i++)
        {
            if (cq.TryDequeue(out string? towel) == true)
            {
                Console.WriteLine($"I used {towel??"<no towel>"}");
            }
            else { Console.WriteLine("No towel available"); i--; }
        }
        return Task.FromResult(0);
    }

    static void ParallelInvokeUnsafeCounter(ulong countTo)
    {
        globalCounter = 0;
        List<Task> tasks = new();
        Stopwatch sw = Stopwatch.StartNew();
        tasks.Add(new Task(() => UNSAFECounter(countTo)));
        tasks.Add(new Task(() => UNSAFECounter(countTo)));
        tasks.Add(new Task(() => UNSAFECounter(countTo)));
        Parallel.ForEach(tasks.ToArray(), task => task.Start());
        Console.WriteLine($"All threads in {System.Reflection.MethodBase.GetCurrentMethod()?.Name ?? "*UNKNOWN*"} have been asked to start.");
        Task.WaitAll(tasks.ToArray());
        Console.WriteLine($"All threads are guaranteed to be done.  Time Elapsed: {sw.Elapsed}");
        Console.WriteLine($"Final counter value: {globalCounter}");
    }
    static void ParallelInvokeILCounter(ulong countTo)
    {
        globalCounter = 0;
        List<Task> tasks = new();
        Stopwatch sw = Stopwatch.StartNew();
        tasks.Add(new Task(() => ILCounter(countTo)));
        tasks.Add(new Task(() => ILCounter(countTo)));
        tasks.Add(new Task(() => ILCounter(countTo)));
        Parallel.ForEach(tasks.ToArray(), task => task.Start());
        Console.WriteLine($"All threads in {System.Reflection.MethodBase.GetCurrentMethod()?.Name ?? "*UNKNOWN*"} have been asked to start.");
        Task.WaitAll(tasks.ToArray());
        Console.WriteLine($"All threads are guaranteed to be done.  Time Elapsed: {sw.Elapsed}");
        Console.WriteLine($"Final counter value: {globalCounter}");
    }
    static void ParallelInvokeLCounter(ulong countTo)
    {
        globalCounter = 0;
        List<Task> tasks = new();
        Stopwatch sw = Stopwatch.StartNew();
        tasks.Add(new Task(() => LCounter(countTo)));
        tasks.Add(new Task(() => LCounter(countTo)));
        tasks.Add(new Task(() => LCounter(countTo)));
        Parallel.ForEach(tasks.ToArray(), task => task.Start());
        Console.WriteLine($"All threads in {System.Reflection.MethodBase.GetCurrentMethod()?.Name ?? "*UNKNOWN*"} have been asked to start.");
        Task.WaitAll(tasks.ToArray());
        Console.WriteLine($"All threads are guaranteed to be done.  Time Elapsed: {sw.Elapsed}");
        Console.WriteLine($"Final counter value: {globalCounter}");
    }

    static void TaskBasedUnsafeCounter(ulong countTo)
    {
        globalCounter = 0;
        List<Task> tasks = new();
        Stopwatch sw = Stopwatch.StartNew();
        tasks.Add(Task.Run(() => UNSAFECounter(countTo)));
        tasks.Add(Task.Run(() => UNSAFECounter(countTo)));
        tasks.Add(Task.Run(() => UNSAFECounter(countTo)));
        Console.WriteLine($"All threads in {System.Reflection.MethodBase.GetCurrentMethod()?.Name ?? "*UNKNOWN*"} have been asked to start.");
        Task.WaitAll(tasks.ToArray());
        Console.WriteLine($"All threads are guaranteed to be done.  Time Elapsed: {sw.Elapsed}");
        Console.WriteLine($"Final counter value: {globalCounter}");
    }

    static void TaskBasedILCounter(ulong countTo)
    {
        globalCounter = 0;
        List<Task> tasks = new();
        Stopwatch sw = Stopwatch.StartNew();
        tasks.Add(Task.Run(() => ILCounter(countTo)));
        tasks.Add(Task.Run(() => ILCounter(countTo)));
        tasks.Add(Task.Run(() => ILCounter(countTo)));
        Console.WriteLine($"All threads in {System.Reflection.MethodBase.GetCurrentMethod()?.Name ?? "*UNKNOWN*"} have been asked to start.");
        Task.WaitAll(tasks.ToArray());
        Console.WriteLine($"All threads are guaranteed to be done.  Time Elapsed: {sw.Elapsed}");
        Console.WriteLine($"Final counter value: {globalCounter}");
    }
    static void TaskBasedLCounter(ulong countTo)
    {
        globalCounter = 0;
        List<Task> tasks = new();
        Stopwatch sw = Stopwatch.StartNew();
        tasks.Add(Task.Run(() => LCounter(countTo)));
        tasks.Add(Task.Run(() => LCounter(countTo)));
        tasks.Add(Task.Run(() => LCounter(countTo)));
        Console.WriteLine($"All threads in {System.Reflection.MethodBase.GetCurrentMethod()?.Name ?? "*UNKNOWN*"} have been asked to start.");
        Task.WaitAll(tasks.ToArray());
        Console.WriteLine($"All threads are guaranteed to be done.  Time Elapsed: {sw.Elapsed}");
        Console.WriteLine($"Final counter value: {globalCounter}");
    }


    static void SafeParForEachVarThreads(ulong countTo)
    {
        globalCounter = 0;
        List<Thread> threads = new List<Thread>();
        threads.Add(new Thread(() => ILCounter(countTo)));
        threads.Add(new Thread(() => ILCounter(countTo)));
        threads.Add(new Thread(() => ILCounter(countTo)));
        Stopwatch sw = Stopwatch.StartNew();
        Parallel.ForEach(threads, thread => thread.Start());
        foreach (var thread in threads)
        {
            thread.Join();
        }
        Console.WriteLine($"All threads in {System.Reflection.MethodBase.GetCurrentMethod()?.Name ?? "*UNKNOWN*"} have been asked to start.");
        Console.WriteLine($"All threads are guaranteed to be done.  Time Elapsed: {sw.Elapsed}");
        Console.WriteLine($"Final counter value: {globalCounter}");
    }
    static void UnsafeParForEachVarThreads(ulong countTo)
    {
        globalCounter = 0;
        List<Thread> threads = new List<Thread>();
        threads.Add(new Thread(() => UNSAFECounter(countTo)));
        threads.Add(new Thread(() => UNSAFECounter(countTo)));
        threads.Add(new Thread(() => UNSAFECounter(countTo)));
        Stopwatch sw = Stopwatch.StartNew();
        Parallel.ForEach(threads, thread => thread.Start());
        foreach (var thread in threads)
        {
            thread.Join();
        }
        Console.WriteLine($"All threads in {System.Reflection.MethodBase.GetCurrentMethod()?.Name ?? "*UNKNOWN*"} have been asked to start.");
        Console.WriteLine($"All threads are guaranteed to be done.  Time Elapsed: {sw.Elapsed}");
        Console.WriteLine($"Final counter value: {globalCounter}");
    }
    static void unsafeVarThreads(ulong countTo)
    {
        globalCounter = 0;
        List<Thread> threads = new List<Thread>();
        threads.Add(new Thread(() => UNSAFECounter(countTo)));
        threads.Add(new Thread(() => UNSAFECounter(countTo)));
        threads.Add(new Thread(() => UNSAFECounter(countTo)));
        Stopwatch sw = Stopwatch.StartNew();
        foreach (var thread in threads)
        {
            thread.Start();
        }
        Console.WriteLine($"All threads in {System.Reflection.MethodBase.GetCurrentMethod()?.Name ?? "*UNKNOWN*"} have been asked to start.");
        foreach (var thread in threads)
        {
            thread.Join();
        }
        Console.WriteLine($"All threads are guaranteed to be done.  Time Elapsed: {sw.Elapsed}");
        Console.WriteLine($"Final counter value: {globalCounter}");
    }

    static void UsingInterlocked(ulong countTo)
    {
        globalCounter = 0;
        var th1 = new Thread(() => ILCounter(countTo));
        var th2 = new Thread(() => ILCounter(countTo));
        var th3 = new Thread(() => ILCounter(countTo));
        th1.Start();
        th2.Start();
        th3.Start();
        Console.WriteLine($"All threads in {System.Reflection.MethodBase.GetCurrentMethod()?.Name ?? "*UNKNOWN*"} have been asked to start.");
        Stopwatch sw = Stopwatch.StartNew(); ;
        th1.Join();
        th2.Join();
        th3.Join();
        Console.WriteLine($"All threads are guaranteed to be done.  Time Elapsed: {sw.Elapsed}");
        Console.WriteLine($"Final counter value: {globalCounter}");
    }
    static void UsingMonitorTimed(ulong countTo)
    {
        globalCounter = 0;
        var th1 = new Thread(() => MonitorCounterTimed(countTo));
        var th2 = new Thread(() => MonitorCounterTimed(countTo));
        var th3 = new Thread(() => MonitorCounterTimed(countTo));
        th1.Start();
        th2.Start();
        th3.Start();
        Console.WriteLine($"All threads in {System.Reflection.MethodBase.GetCurrentMethod()?.Name ?? "*UNKNOWN*"} have been asked to start.");
        Stopwatch sw = Stopwatch.StartNew(); ;
        th1.Join();
        th2.Join();
        th3.Join();
        Console.WriteLine($"All threads are guaranteed to be done.  Time Elapsed: {sw.Elapsed}");
        Console.WriteLine($"Final counter value: {globalCounter}");
    }
    static void UsingMonitor(ulong countTo)
    {
        globalCounter = 0;
        var th1 = new Thread(() => MonitorCounter(countTo));
        var th2 = new Thread(() => MonitorCounter(countTo));
        var th3 = new Thread(() => MonitorCounter(countTo));
        th1.Start();
        th2.Start();
        th3.Start();
        Console.WriteLine($"All threads in {System.Reflection.MethodBase.GetCurrentMethod()?.Name ?? "*UNKNOWN*"} have been asked to start.");
        Stopwatch sw = Stopwatch.StartNew(); ;
        th1.Join();
        th2.Join();
        th3.Join();
        Console.WriteLine($"All threads are guaranteed to be done.  Time Elapsed: {sw.Elapsed}");
        Console.WriteLine($"Final counter value: {globalCounter}");
    }
    static void UsingLock(ulong countTo)
    {
        globalCounter = 0;
        var th1 = new Thread(() => LCounter(countTo));
        var th2 = new Thread(() => LCounter(countTo));
        var th3 = new Thread(() => LCounter(countTo));
        th1.Start();
        th2.Start();
        th3.Start();
        Console.WriteLine($"All threads in {System.Reflection.MethodBase.GetCurrentMethod()?.Name ?? "*UNKNOWN*"} have been asked to start.");
        Stopwatch sw = Stopwatch.StartNew(); ;
        th1.Join();
        th2.Join();
        th3.Join();
        Console.WriteLine($"All threads are guaranteed to be done.  Time Elapsed: {sw.Elapsed}");
        Console.WriteLine($"Final counter value: {globalCounter}");
    }

    static void Unprotected(ulong countTo)
    {
        globalCounter = 0;
        var th1 = new Thread(() => UNSAFECounter(countTo));
        var th2 = new Thread(() => UNSAFECounter(countTo));
        var th3 = new Thread(() => UNSAFECounter(countTo));
        th1.Start();
        th2.Start();
        th3.Start();
        Console.WriteLine($"All threads in {System.Reflection.MethodBase.GetCurrentMethod()?.Name ?? "*UNKNOWN*"} have been asked to start.");
        Stopwatch sw = Stopwatch.StartNew(); ;
        th1.Join();
        th2.Join();
        th3.Join();
        Console.WriteLine($"All threads are guaranteed to be done.  Time Elapsed: {sw.Elapsed}");
        Console.WriteLine($"Final counter value: {globalCounter}");
    }

    static void ILCounter(ulong countFrom1ToN)
    {
        for (ulong i = 0; i < countFrom1ToN; i++)
        {
            Interlocked.Increment(ref globalCounter);
        }
    }

    static void UNSAFECounter(ulong countFrom1ToN)
    {
        for (ulong i = 0; i < countFrom1ToN; i++)
        {
            globalCounter++;
        }
    }

    static void LCounter(ulong countFrom1ToN)
    {
        for (ulong i = 0; i < countFrom1ToN; i++)
            lock (x)
            {
                globalCounter++;
            }
    }
    static void MonitorCounter(ulong countFrom1ToN)
    {
        for (ulong i = 0; i < countFrom1ToN; i++)
        {
            Monitor.Enter(x);
            globalCounter++;
            Monitor.Exit(x);
        }
    }
    static void MonitorCounterTimed(ulong countFrom1ToN)
    {
        for (ulong i = 0; i < countFrom1ToN; i++)
        {
            if (Monitor.TryEnter(x, TimeSpan.FromMilliseconds(10)))
            {
                try
                {
                    globalCounter++;
                }
                finally
                {
                    Monitor.Exit(x);
                }
            }
            else
            {
                // Handle the case where the lock was not acquired
                // For example, you might log this event or retry
                Console.WriteLine("Failed to acquire the lock within the timeout period.");
                i--;  //so I don't goof up on my actual count!
            }
        }
    }
}
