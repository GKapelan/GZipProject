using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;

namespace GZipTest
{
    class Compression : GZipper
    {
        public Compression(String input, String output)
            : base(input, output)
        {
        }

        public override void Execute()
        {
            Console.WriteLine("Starting ... ");

            Thread reader = new Thread(new ThreadStart(Read));

            reader.Start();

            Thread[] compressor = new Thread[compressionThreads];

            exitCompressionThread = new ManualResetEvent[compressionThreads];

            for (int i = 0; i < compressionThreads; i++)
            {
                compressor[i] = new Thread(new ParameterizedThreadStart(Compress));
                exitCompressionThread[i] = new ManualResetEvent(false);
                compressor[i].Start(i);
            }

            Thread writer = new Thread(new ThreadStart(Write));
            writer.Start();

            WaitHandle.WaitAll(exitCompressionThread);
            writeBuffer.Close();

            Console.WriteLine("File {0} was successfully compressed! \nPress any key to close window ... ", inputFile);

            //throw new NotImplementedException();
        }

        protected override void Read()
        {
            int bytesRead;
            byte[] buffer = new byte[bufferSize];

            using (FileStream input = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
            {
                while ((bytesRead = input.Read(buffer, 0, bufferSize)) > 0 && !cancel)
                {
                    byte[] lastBuffer = new byte[bytesRead];
                    Buffer.BlockCopy(buffer, 0, lastBuffer, 0, bytesRead);
                    readBuffer.EnquequeBytes(lastBuffer);
                }
                readBuffer.Close();
            }
            //throw new NotImplementedException();
        }

        private void Compress(object threadNumber)
        {
            ByteChunk inputChunk;

            while (readBuffer.TryDequeue(out inputChunk) && !cancel)
            {
                using (MemoryStream ms = new MemoryStream())
                { 
                    using (GZipStream gz = new GZipStream(ms, CompressionMode.Compress))
                    using (BinaryWriter bw = new BinaryWriter(gz))
                    {
                        bw.Write(inputChunk.Content, 0, inputChunk.Content.Length);
                    }

                    byte[] outBuffer = ms.ToArray();
                    ByteChunk outputChunk = new ByteChunk(inputChunk.ID, outBuffer);
                    writeBuffer.Enqueue(outputChunk);
                }
            }
            ManualResetEvent exitThread = exitCompressionThread[(int)threadNumber];
            exitThread.Set();
        }

        protected override void Write()
        {
            using (FileStream output = new FileStream(outputFile, FileMode.Create, FileAccess.Write)) // is that good idea?
            {
                BinaryFormatter formatter = new BinaryFormatter();
                ByteChunk chunk;

                while (writeBuffer.TryDequeue(out chunk) && !cancel)
                {
                    formatter.Serialize(output, chunk);
                }
            }
            if (!cancel) success = true;
            //throw new NotImplementedException();
        }

    }
}
