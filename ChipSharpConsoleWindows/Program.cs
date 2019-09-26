using System.Collections.Generic;
using System.IO;

namespace ChipSharpConsoleWindows
{
    class Program
    {
        static void Main()
        {
            const string testROM = @"ROM\sample.ch8";
            CPU cpu = new CPU();

            // Read the binary file correctly
            using (BinaryReader reader = new BinaryReader(File.Open(testROM, FileMode.Open)))
            {
                List<ushort> program = new List<ushort>();
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    //ushort opcode = (ushort)(reader.ReadByte() << 8 | reader.ReadByte());
                    program.Add(reader.ReadByte());
                }

                cpu.LoadProgram(program.ToArray());
            }

            while (true)
            {
                try
                {
                    cpu.Step();
                }
                catch (System.Exception e)
                {
                    System.Console.WriteLine(e.Message);
                }
            }
        }
    }
}
