using System.Security.Cryptography;
using System.Text;

namespace random_typewriter;

public class Result
{
    public Result()
    {
        Found = false;
    }

    public bool Found;
}


public static class Program
{

    // Main Method
    public static void Main()
    {
        var Words = new List<string>(){"A", "Be", "Cee", "Dead", "Edgie"};
        DateTime startTime;
        DateTime endTime;
        TimeSpan ts;

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

        var tc = 0;

        ThreadPool.GetMaxThreads(out var workerThreads, out _);

        ParallelOptions parallelOptions = new()
        {
            // MaxDegreeOfParallelism = 2
        };

        var programStart = DateTime.Now;

        for (var c = 1; c <= 5; c++)
        {
            Console.WriteLine($"Iteration {c}");
            foreach (var word in Words)
            {
                # region whole word
                Console.WriteLine("Whole Word Random Gen");
                startTime = DateTime.Now;
                Parallel.For(0, workerThreads,
                    parallelOptions,
                    // using shared variable in parallel loop
                    // https://stackoverflow.com/questions/43690168/using-shared-variable-in-parallel-for
                    () => new Result(),
                    (i, state, result) =>
                    {
                        // Console.WriteLine($"Started process on Thread {Thread.CurrentThread.ManagedThreadId}");
                        tc++;
                        while (!result.Found && !state.IsStopped)
                        {
                            result.Found = (word == KeyGenerator.GetUniqueKey(word.Length));
                        };



                        if (result.Found && !state.IsStopped)
                        {
                            Console.WriteLine($"Found {word} on Thread {Environment.CurrentManagedThreadId}");
                            // https://dotnettutorials.net/lesson/parallel-for-method-csharp/
                            state.Stop();
                            Console.WriteLine($"Stop Called on {Environment.CurrentManagedThreadId} \nTotal Threads Running: {tc}");
                        }

                        return result;
                    },
                    result => { }
                );

                endTime = DateTime.Now;
                ts = endTime - startTime;
                Console.WriteLine("Whole word run time {0}ms", ts.TotalMilliseconds);

                # endregion

                Console.WriteLine("----------------------------------------\n");

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
                        // Console.WriteLine($"Started process on Thread {Environment.CurrentManagedThreadId}");
                        tc++;

                        var currentWord = "";
                        while (!result.Found && !state.IsStopped)
                        {
                            currentWord += KeyGenerator.GetUniqueKey(1);

                            // check to see if match
                            if (currentWord == word)
                            {
                                Console.WriteLine(currentWord);
                                result.Found = true;
                            }

                            for (var j = 0; j < currentWord.Length; j++)
                            {
                                if (currentWord[j] != word[j])
                                {
                                    currentWord = "";
                                    break;
                                }
                            }
                        };

                        if (result.Found && !state.IsStopped)
                        {
                            Console.WriteLine($"Found {word} on Thread {Environment.CurrentManagedThreadId}");
                            // https://dotnettutorials.net/lesson/parallel-for-method-csharp/
                            state.Stop();
                            Console.WriteLine($"Stop Called on {Environment.CurrentManagedThreadId} \nTotal Threads Running: {tc}");
                        }

                        return result;
                    },
                    result => { }
                );
                endTime = DateTime.Now;
                ts = endTime - startTime;
                Console.WriteLine("Letter by letter run time {0}ms", ts.TotalMilliseconds);

                #endregion

            }
        
        }
        #region wrapup
        
        Console.WriteLine("----------------------------------------\n");
        
        var programEnd = DateTime.Now;

        ts = programEnd - programStart;
        Console.WriteLine("Total Run Time {0}ms", ts.TotalMilliseconds);
        Console.SetOut(oldOut);
        writer.Close();
        ostrm.Close();
        Console.WriteLine("Finished");
        #endregion
    }

}

public static class KeyGenerator
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