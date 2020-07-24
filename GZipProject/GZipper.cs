using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace GZipTest
{
    abstract class GZipper 
    {
        protected static bool success = false;
        protected static bool cancel = false;

        protected static int bufferSize = 32 * 1024;
        protected static int queueSize = 20;

        protected ByteChunkQueue readBuffer  = new ByteChunkQueue(queueSize);
        protected ByteChunkQueue writeBuffer = new ByteChunkQueue(queueSize);

        protected String inputFile;
        protected String outputFile;

        protected static int compressionThreads = (Environment.ProcessorCount - 2) > 0 ? Environment.ProcessorCount - 2 : 1;

        protected ManualResetEvent[] exitCompressionThread;

        ///constructor
        public GZipper(String input, String output)
        {
            inputFile = input;
            outputFile = output;
        }

        abstract public void Execute(); // why not protected?
        abstract protected void Read();
        abstract protected void Write();

        public void Cancel()
        {
            cancel = true;
        }

        public int GetSuccessValue()
        {
            return success ? 1 : 0;
        }
    }
}
