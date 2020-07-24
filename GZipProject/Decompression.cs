using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.IO.Compression;

namespace GZipTest
{
    class Decompression : GZipper
    {
        public Decompression(String input, String output)
            : base(input, output)
        {
        }

        public override void Execute()
        {
            Console.WriteLine("Starting ... ");

            Thread reader = new Thread(new ThreadStart(Read));
            reader.Start();

            Thread[] decompressor = new Thread[compressionThreads];
            exitCompressionThread = new ManualResetEvent[compressionThreads];
            for (int i = 0; i < compressionThreads; i++)
            {
                decompressor[i] = new Thread(new ParameterizedThreadStart(Decompress));
                exitCompressionThread[i] = new ManualResetEvent(false);
                decompressor[i].Start(i);
            }

            Thread writer = new Thread(new ThreadStart(Write));
            writer.Start();

            WaitHandle.WaitAll(exitCompressionThread);
            writeBuffer.Close();

            Console.WriteLine("File {0} was successfully decompressed! \nPress any key to close window ... ", inputFile);
            //throw new NotImplementedException();
        }

        protected override void Read()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            ByteChunk chunk;

            using (FileStream input = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
            {
                while (input.Position < input.Length && !cancel)
                {
                    chunk = (ByteChunk)formatter.Deserialize(input);
                    readBuffer.Enqueue(chunk);
                }
                readBuffer.Close();
            }
            //throw new NotImplementedException();
        }

        private void Decompress(object threadNumber)
        {
            ByteChunk inputChunk;

            while (readBuffer.TryDequeue(out inputChunk) && !cancel)
            {
                using (MemoryStream ms = new MemoryStream(inputChunk.Content))
                {
                    using (GZipStream gz = new GZipStream(ms, CompressionMode.Decompress))
                    {
                        int bytesRead;
                        byte[] buffer = new byte[bufferSize];

                        bytesRead = gz.Read(buffer, 0, buffer.Length);

                        byte[] lastBuffer = new byte[bytesRead];
                        Buffer.BlockCopy(buffer, 0, lastBuffer, 0, bytesRead);
                        ByteChunk outputChunk = new ByteChunk(inputChunk.ID, lastBuffer);

                        writeBuffer.Enqueue(outputChunk);
                    }
                }
            }
            ManualResetEvent exitThreat = exitCompressionThread[(int)threadNumber];
            exitThreat.Set();
        }

        protected override void Write()
        {
            using (FileStream output = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
            {
                ByteChunk chunk;
                while (writeBuffer.TryDequeue(out chunk) && !cancel)
                {
                    byte[] buffer = chunk.Content;
                    output.Write(buffer, 0, buffer.Length);
                }
            }
            if (!cancel) success = true;
            //throw new NotImplementedException();
        }
    }
}
