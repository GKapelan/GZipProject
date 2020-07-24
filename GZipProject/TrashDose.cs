/// old trash
/// 

/*
static int blockForCompress = (int)Math.Pow(2, 20);

public static byte[][] dataSource = new byte[threadNumber][];
public static byte[][] dataSourceZip = new byte[threadNumber][];
 */

// easy way to compress/decompress
/*
public static void Compress(String sourceFile, String destinationFile)
{
    FileStream sInput = new FileStream(sourceFile, FileMode.OpenOrCreate);
    FileStream sOutput = new FileStream(destinationFile + ".gz", FileMode.Create);

    byte[] buffer = new byte[1024 * 1024]; // 1 MB

    using (GZipStream zStream = new GZipStream(sOutput, CompressionMode.Compress))
    {
        //sInput.CopyTo(zStream); // it's enougth to work

        int bytesRead = 0;
        while (bytesRead < sInput.Length)
        {
            int ReadLength = sInput.Read(buffer, 0, buffer.Length);
            zStream.Write(buffer, 0, ReadLength);
            zStream.Flush();
            bytesRead += ReadLength;
        }
        sOutput.Flush();
    }
            
    Console.WriteLine("Compress was successfully completed!");

    sInput.Close();
    sOutput.Close();
}
public static void Decompress(String sourceFile, String destinationFile)
{
    FileStream sInput = new FileStream(sourceFile + ".gz", FileMode.OpenOrCreate);
    FileStream sOutput = new FileStream(destinationFile, FileMode.Create);

    using (GZipStream zStream = new GZipStream(sInput, CompressionMode.Decompress))
    {
        zStream.CopyTo(sOutput);
    }

    Console.WriteLine("Decompress was successfully completed!");
}
*/

// hard version to compress/decompress with threads
/*
public static void CompressZip(String sourceFile, String resultFile)
{
    Console.WriteLine("Working ... Please wait.");
    try
    {
        using (FileStream sInput = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, 64 * 1024, FileOptions.Asynchronous))
        using (BufferedStream bStreamInput = new BufferedStream(sInput, 64 * 1024))
        {
            using (FileStream sOutput = new FileStream(resultFile + ".gz", FileMode.Append))
            {
                Thread[] pool;
                while (bStreamInput.Position < bStreamInput.Length)
                {
                    pool = new Thread[threadNumber];
                    ReadBuffer(bStreamInput, pool);
                    CreateZipFile(sOutput, pool);
                }
            }
        }
    }
    catch (Exception e)
    {
        if (e.Data == null)
            Console.WriteLine("File was successfully compressed!");
        else
            Console.WriteLine("Error! " + e.Message);
        Console.WriteLine("Press any key...");
    }

}

public static void ReadBuffer(BufferedStream Buff, Thread[] pool)
{
    int FileBlock;
    for (int N = 0; (N < threadNumber) && (Buff.Position < Buff.Length); N++)
    {
        if (Buff.Length - Buff.Position >= blockForCompress)
            FileBlock = blockForCompress;
        else
            FileBlock = (int)(Buff.Length - Buff.Position);
        dataSource[N] = new byte[FileBlock];
        Buff.Read(dataSource[N], 0, FileBlock); // читаем блока файла длинной FileBlock и пишем в буфер dataSource[N] || int i = File.Read, i сколько прочитали
        pool[N] = new Thread(CompressBlock); //
        pool[N].Name = "tr_" + N;
        pool[N].Start(N);
    }
}

private static void CompressBlock(object i)
{
    using (MemoryStream output = new MemoryStream(dataSource[(int)i].Length))
    {
        using (GZipStream cs = new GZipStream(output, CompressionMode.Compress))
        {
            cs.Write(dataSource[(int)i], 0, dataSource[(int)i].Length);
        }
        dataSourceZip[(int)i] = output.ToArray();   // dataSourceZip
    }
}

private static void CreateZipFile(FileStream FileZip, Thread[] tPool)
{
    for (int N = 0; (N < threadNumber) && (tPool[N] != null); )
    {
        tPool[N].Join();                                    // waiting thread
        BitConverter.GetBytes(dataSourceZip[N].Length + 1)
                    .CopyTo(dataSourceZip[N], 4);
        FileZip.Write(dataSourceZip[N], 0, dataSourceZip[N].Length);
        N++;
    }
}

public static void DecompressZip(String sourceFile, String resultFile)
{
    Console.WriteLine("Working ... Please wait.");
    try
    {

        using (FileStream sInput = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, 64 * 1024, FileOptions.Asynchronous))
        using (BufferedStream bStreamInput = new BufferedStream(sInput, 64 * 1024))
        {
            using (FileStream sOutput = new FileStream(resultFile, FileMode.Append))
            {
                Thread[] pool;
                while (bStreamInput.Position < bStreamInput.Length)
                {
                    pool = new Thread[threadNumber];
                    ReadZipFile(sInput, pool);
                    CreateUnzipFile(sOutput, pool);
                }
            }
        }
    }
    catch (Exception e)
    {
        if (e.Data == null)
            Console.WriteLine("File was successfully decompressed!");
        else
            Console.WriteLine("Error! " + e.Message);
        Console.WriteLine("Press any key...");
    }
}

private static void ReadZipFile(FileStream Zip, Thread[] tPool)
{
    for (int N = 0; (N < threadNumber) && (Zip.Position < Zip.Length); N++)
    {
        byte[] buffer = new byte[8];

        Zip.Read(buffer, 0, 8);// 8 byte (4 byte CRC32, 4 byte ISIZE)
        int ZipBlockLength = BitConverter.ToInt32(buffer, 4);

        dataSourceZip[N] = new byte[ZipBlockLength - 1];
        buffer.CopyTo(dataSourceZip[N], 0);
        Zip.Read(dataSourceZip[N], 8, dataSourceZip[N].Length - 8);
        int dataPortionSize = BitConverter.ToInt32(dataSourceZip[N], dataSourceZip[N].Length - 4);
        dataSource[N] = new byte[dataPortionSize];

        tPool[N] = new Thread(DecompressBlock);
        tPool[N].Name = "Tr_" + N;
        tPool[N].Start(N);
    }
}

public static void DecompressBlock(object i)
{
    using (MemoryStream input = new MemoryStream(dataSourceZip[(int)i]))
    using (GZipStream ds = new GZipStream(input, CompressionMode.Decompress))
    {
        ds.Read(dataSource[(int)i], 0, dataSource[(int)i].Length);
    }
}

private static void CreateUnzipFile(FileStream outFile, Thread[] tPool)
{
    for (int N = 0; (N < threadNumber) && (tPool[N] != null); )
    {
        tPool[N].Join();
        outFile.Write(dataSource[N], 0, dataSource[N].Length);
        N++;
    }
}
*/

/*
public class SuperQueue<T> : IDisposable where T : class
{
    readonly object _locker = new object();             // locker
    readonly List<Thread> _workers;                     // threads list
    readonly Queue<T> _taskQueue = new Queue<T>();      // queue
    readonly Action<T> _dequeueAction;

    /// <summary>
    /// Initializes a new instance of the <see cref="SuperQueue{T}"/> class.
    /// </summary>
    /// <param name="workerCount">The worker count.</param>
    /// <param name="dequeueAction">The dequeue action.</param>

    public SuperQueue(int workerCount, Action<T> dequeueAction)
    {
        _dequeueAction = dequeueAction;
        _workers = new List<Thread>(workerCount);

        for (int i = 0; i < workerCount; i++)
        {
            Thread t = new Thread(Consume)
            {
                IsBackground = true,
                Name = string.Format("SuperQueue worker {0}", i)
            };
            _workers.Add(t);
            t.Start();
            Console.WriteLine("{0},{1}", i, t.Name);
        }
    }

    /// <summary>
    /// Enqueues the task. 
    /// </summary>
    /// <param name="task">The task.</param>
    public void EnqueueTask(T task)
    {
        lock (_locker)
        {
            _taskQueue.Enqueue(task);
            Monitor.PulseAll(_locker);
        }
    }

    /// <summary>
    /// Consumes this instance.
    /// </summary>
    void Consume()
    {
        while (true)
        {
            T item;
            lock (_locker)
            {
                while (_taskQueue.Count == 0) Monitor.Wait(_locker);
                item = _taskQueue.Dequeue();
            }
            if (item == null) return;
            // run actual method
            _dequeueAction(item);
        }
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        // Enqueue one null task per worker to make each exit
        _workers.ForEach(thread => EnqueueTask(null));
        _workers.ForEach(thread => thread.Join());

    }
}
 */

/*
if (command.Equals("compress"))
    //Compress(inputFile, outputFile);
    //CompressZip(inputFile, outputFile);
if (command.Equals("decompress"))
    //Decompress(inputFile, outputFile);
    //DecompressZip(inputFile, outputFile);
    //DecompressZip(outputFile + ".gz", "1_" + inputFile);
 */