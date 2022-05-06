using System;
using System.Threading;
using System.Security.Cryptography;
using System.Text;


public class Result
{
    public Result()
    {
        found = false;
    }

    public bool found;
}


public class Program
{
    const string WORD = "Monkey";

    // Main Method
    static public void Main()
    {

        #region set console output
        // https://stackoverflow.com/questions/4470700/how-to-save-console-writeline-output-to-text-file
        FileStream ostrm;
        StreamWriter writer;
        TextWriter oldOut = Console.Out;
        try
        {
            ostrm = new FileStream("./Output.txt", FileMode.OpenOrCreate, FileAccess.Write);
            writer = new StreamWriter(ostrm);
        }
        catch (Exception e)
        {
            Console.WriteLine("Cannot open Output.txt for writing");
            Console.WriteLine(e.Message);
            return;
        }

        Console.SetOut(writer);

        #endregion

        int workerThreads;
        int portThreads;
        var tc = 0;

        ThreadPool.GetMaxThreads(out workerThreads, out portThreads);

        ParallelOptions parallelOptions = new()
        {
            // MaxDegreeOfParallelism = 2
        };

        Console.WriteLine($"Total Threads available {workerThreads}");

        DateTime programStart = DateTime.Now;


        # region whole word
        Console.WriteLine("Whole Word Random Gen");
        DateTime startTime = DateTime.Now;
        Parallel.For(0, workerThreads,
        parallelOptions,
        // using shared variable in parallel loop
        // https://stackoverflow.com/questions/43690168/using-shared-variable-in-parallel-for
        () => new Result(),
        (i, state, result) =>
        {
            Console.WriteLine($"Started process on Thread {Thread.CurrentThread.ManagedThreadId}");
            tc++;
            while (!result.found && !state.IsStopped)
            {
                result.found = (WORD == KeyGenerator.GetUniqueKey(WORD.Length));
            };



            if (result.found && !state.IsStopped)
            {
                Console.WriteLine($"Found {WORD} on Thread {Thread.CurrentThread.ManagedThreadId}");
                // https://dotnettutorials.net/lesson/parallel-for-method-csharp/
                state.Stop();
                Console.WriteLine($"Stop Called on {Thread.CurrentThread.ManagedThreadId} \nTotal Threads Running: {tc}");
            }

            return result;
        },
            result => { }
        );

        DateTime endTime = DateTime.Now;
        TimeSpan ts = endTime - startTime;
        // Console.WriteLine("Run Time {0}ms", ts.TotalMilliseconds);

        # endregion


        # region Letter By Letter
        Console.WriteLine("Letter By Letter Random Gen");
        startTime = DateTime.Now;
        Parallel.For(0, workerThreads,
        parallelOptions,
        // using shared variable in parallel loop
        // https://stackoverflow.com/questions/43690168/using-shared-variable-in-parallel-for
        () => new Result(),
        (i, state, result) =>
        {
            Console.WriteLine($"Started process on Thread {Thread.CurrentThread.ManagedThreadId}");
            tc++;

            var currentWord = "";
            while (!result.found && !state.IsStopped)
            {
                currentWord += KeyGenerator.GetUniqueKey(1);

                // check to see if match
                if (currentWord == WORD)
                {
                    Console.WriteLine(currentWord);
                    result.found = true;
                }

                for (var j = 0; j < currentWord.Length; j++)
                {
                    if (currentWord[j] != WORD[j])
                    {
                        currentWord = "";
                        break;
                    }
                }
            };

            if (result.found && !state.IsStopped)
            {
                Console.WriteLine($"Found {WORD} on Thread {Thread.CurrentThread.ManagedThreadId}");
                // https://dotnettutorials.net/lesson/parallel-for-method-csharp/
                state.Stop();
                Console.WriteLine($"Stop Called on {Thread.CurrentThread.ManagedThreadId} \nTotal Threads Running: {tc}");
            }

            return result;
        },
            result => { }
        );


        #endregion

        DateTime programEnd = DateTime.Now;

        ts = programEnd - programStart;
        Console.WriteLine("Total Run Time {0}ms", ts.TotalMilliseconds);
        Console.SetOut(oldOut);
        writer.Close();
        ostrm.Close();
        Console.WriteLine("Finished");
    }

}

public class KeyGenerator
{
    // https://stackoverflow.com/questions/1344221/how-can-i-generate-random-alphanumeric-strings
    internal static readonly char[] chars =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();

    public static string GetUniqueKey(int size)
    {
        byte[] data = new byte[4 * size];
        using (var crypto = RandomNumberGenerator.Create())
        {
            crypto.GetBytes(data);
        }
        StringBuilder result = new StringBuilder(size);
        for (int i = 0; i < size; i++)
        {
            var rnd = BitConverter.ToUInt32(data, i * 4);
            var idx = rnd % chars.Length;

            result.Append(chars[idx]);
        }

        return result.ToString();
    }

    public static string GetUniqueKeyOriginal_BIASED(int size)
    {
        char[] chars =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
        byte[] data = new byte[size];
        using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
        {
            crypto.GetBytes(data);
        }
        StringBuilder result = new StringBuilder(size);
        foreach (byte b in data)
        {
            result.Append(chars[b % (chars.Length)]);
        }
        return result.ToString();
    }
}