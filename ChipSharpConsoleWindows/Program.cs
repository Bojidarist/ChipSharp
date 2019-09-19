using System;
using System.IO;

namespace ChipSharpConsoleWindows
{
    class Program
    {
        static void Main()
        {
            const string testROM = @"ROM\IBMLogo.ch8";
            CPU cpu = new CPU();

            // Read the binary file correctly
            using (BinaryReader reader = new BinaryReader(File.Open(testROM, FileMode.Open)))
            {
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    ushort opcode = (ushort)(reader.ReadByte() << 8 | reader.ReadByte());
                    try
                    {
                        cpu.ExecuteOpcode(opcode);
                    }
                    catch (FormatException e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }

            Console.ReadKey();
        }
    }
}
