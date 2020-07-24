using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Compression;
using System.IO;
using System.Threading;

///TODO:
/// 1. Нет разбиения кода на логические компоненты: весь код в одном классе в статических функциях
/// 2. Нет обработки ошибок в потоках
/// 3. Нет реального распараллеливания работы:  обработка и запись группы блоков выполняются последовательно
/// 4. Для обработки каждого блока запускается отдельный поток, что неэффективно

namespace GZipTest
{
    class Program
    {
        // new version
  
        static GZipper gzipper;

        public static void ChkCmdLine(String[] args)
        {
            /// length == 3
            if (args.Length != 3)
                throw new ArgumentException("Cmd must contain with 3 arguments \nPlease press any key ... ");
            /// wrong command
            if (args[0].ToLower() != "compress" && args[0].ToLower() != "decompress")
                throw new ArgumentException("Wrong command name. Check please 'Possible command'");
            /// equal names of files
            if (args[1] == args[2])
                throw new ArgumentException("Input and Output files have equal names");
            /// input file does not exist
            if (!File.Exists(args[1]))
                throw new ArgumentException("File " + args[1] + " does't exist");
        }

        static void Main(string[] args)
        {
            Console.WriteLine("How to: GZipTest command input_file output_file");
            Console.WriteLine("Possible commands: compress/decompress");
            Console.WriteLine("-----------------------------------------------\n");

            try
            {
                ChkCmdLine(args);

                String command    = args[0]; // type
                String inputFile  = args[1]; // input file for comp/decomp
                String outputFile = args[2]; // output file 

                Console.WriteLine("Command:         " + command);
                Console.WriteLine("Input File:      " + inputFile);
                Console.WriteLine("Output File:     " + outputFile);

                if (command.ToLower().Equals("compress"))
                    gzipper = new Compression(inputFile, outputFile);
                if (command.ToLower().Equals("decompress"))
                    gzipper = new Decompression(inputFile, outputFile);

                gzipper.Execute();

            }
                catch (Exception e)
            {
                Console.WriteLine("Error! " + e.Message + "\nPress any key ... ");
            }

            Console.ReadKey();
        }
    }
}