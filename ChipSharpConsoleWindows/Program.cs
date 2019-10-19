using System;
using System.Collections.Generic;
using System.IO;

namespace ChipSharpConsoleWindows
{
    class Program
    {
        static void Main()
        {
            // Path to a test ROM
            const string testROM = @"ROM\HeartMonitor.ch8";
            CPU cpu = new CPU();

            // Read the binary file correctly
            using (BinaryReader reader = new BinaryReader(File.Open(testROM, FileMode.Open)))
            {
                List<byte> program = new List<byte>();
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    program.Add(reader.ReadByte());
                }

                cpu.LoadProgram(program.ToArray());
            }

            // Set the window size equal to the Chip-8 display
            Console.SetWindowSize(64, 32);

            // Main loop
            while (true)
            {
                try
                {
                    cpu.Step();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}
