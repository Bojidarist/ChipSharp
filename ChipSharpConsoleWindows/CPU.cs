using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ChipSharpConsoleWindows
{
    /// <summary>
    /// The CPU for Chip-8
    /// </summary>
    public class CPU
    {
        #region Public properties

        /// <summary>
        /// The memory of the machine
        /// </summary>
        public byte[] Memory { get; set; }

        /// <summary>
        /// The registers of the CPU
        /// </summary>
        public byte[] Registers { get; set; }

        /// <summary>
        /// Program Counter
        /// </summary>
        public ushort PC { get; set; }

        /// <summary>
        /// 16bit register (For memory address) (Similar to void pointer)
        /// </summary>
        public ushort IAddress { get; set; }

        /// <summary>
        /// The stack is only used to store return addresses when subroutines are called.
        /// </summary>
        public Stack<ushort> Stack { get; set; }

        /// <summary>
        /// This timer is intended to be used for timing the events of games. Its value can be set and read.
        /// </summary>
        public byte DelayTimer { get; set; }

        /// <summary>
        /// This timer is used for sound effects. When its value is nonzero, a beeping sound is made.
        /// </summary>
        public byte SoundTimer { get; set; }

        /// <summary>
        /// The keyboard
        /// </summary>
        public byte Keyboard { get; set; }

        /// <summary>
        /// Original CHIP-8 Display resolution is 64×32 pixels, and color is monochrome.
        /// </summary>
        public byte[] Display { get; set; }

        /// <summary>
        /// Random number generator
        /// </summary>
        public Random RNGenerator { get; set; }

        /// <summary>
        /// Indicates if the program is waiting for a key press
        /// </summary>
        public bool WaitingForKeyPress { get; set; }

        /// <summary>
        /// The current loaded program
        /// </summary>
        public ushort[] Program { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public CPU()
        {
            this.Display = new byte[64 * 32];
            this.Memory = new byte[4096];
            this.Registers = new byte[16];
            this.Stack = new Stack<ushort>(24);
            this.PC = 0x0000;
            this.IAddress = 0x0000;
            this.RNGenerator = new Random();
            this.WaitingForKeyPress = false;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Load a program
        /// </summary>
        /// <param name="program">The program to load</param>
        public void LoadProgram(byte[] program)
        {
            Array.Clear(this.Memory, 0, this.Memory.Length);
            for (int i = 0; i < program.Length; i++)
            {
                this.Memory[512 + i] = (byte)program[i];
            }
            this.PC = 512;
        }

        /// <summary>
        /// Executes an opcode
        /// </summary>
        /// <param name="opcode">The opcode to execute</param>
        public void Step()
        {
            ushort opcode = (ushort)((this.Memory[this.PC] << 8) | this.Memory[this.PC + 1]);
            ushort nibble = (ushort)(opcode & 0xF000);
            byte vx = (byte)((opcode & 0x0F00) >> 8);
            byte vy = (byte)((opcode & 0x00F0) >> 4);
            if (this.WaitingForKeyPress)
            {
                this.Registers[vx] = this.Keyboard;
                return;
            }

            this.PC += 2;

            switch (nibble)
            {
                case 0x0000:
                    if (opcode == 0x00E0)
                    {
                        // Clears the screen.
                        Array.Clear(this.Display, 0, this.Display.Length);
                    }
                    else if (opcode == 0x00EE)
                    {
                        // Returns from a subroutine.
                        this.PC = this.Stack.Pop();
                    }
                    else
                    {
                        throw new FormatException($"Unsupported opcode: { opcode.ToString("X4") }");
                    }
                    break;
                case 0x1000:
                    this.PC = (ushort)(opcode & 0x0FFF);
                    break;
                case 0x2000:
                    this.Stack.Push(PC);
                    this.PC = (ushort)(opcode & 0x0FFF);
                    break;
                case 0x3000:
                    if (this.Registers[(opcode & 0x0F00) >> 8] == (opcode & 0x00FF))
                    {
                        this.PC += 2;
                    }
                    break;
                case 0x4000:
                    if (this.Registers[(opcode & 0x0F00) >> 8] != (opcode & 0x00FF))
                    {
                        this.PC += 2;
                    }
                    break;
                case 0x5000:
                    if ((this.Registers[(opcode & 0x0F00) >> 8]) == (this.Registers[(opcode & 0x00F0) >> 4]))
                    {
                        this.PC += 2;
                    }
                    break;
                case 0x6000:
                    (this.Registers[(opcode & 0x0F00) >> 8]) = (byte)(opcode & 0x00FF);
                    break;
                case 0x7000:
                    (this.Registers[(opcode & 0x0F00) >> 8]) += (byte)(opcode & 0x00FF);
                    break;
                case 0x8000:
                    switch (opcode & 0x000F)
                    {
                        case 0:
                            this.Registers[vx] = this.Registers[vy];
                            break;
                        case 1:
                            this.Registers[vx] = (byte)(this.Registers[vx] | this.Registers[vy]);
                            break;
                        case 2:
                            this.Registers[vx] = (byte)(this.Registers[vx] & this.Registers[vy]);
                            break;
                        case 3:
                            this.Registers[vx] = (byte)(this.Registers[vx] ^ this.Registers[vy]);
                            break;
                        case 4:
                            this.Registers[15] = (byte)(this.Registers[vx] + this.Registers[vy] > 255 ? 1 : 0);
                            this.Registers[vx] = (byte)((this.Registers[vx] + this.Registers[vy]) & 0x00FF);
                            break;
                        case 5:
                            this.Registers[15] = (byte)(this.Registers[vx] > this.Registers[vy] ? 1 : 0);
                            this.Registers[vx] = (byte)((this.Registers[vx] - this.Registers[vy]) & 0x00FF);
                            break;
                        case 6:
                            this.Registers[15] = (byte)(this.Registers[vx] & 0x0001);
                            this.Registers[vx] >>= 1;
                            break;
                        case 7:
                            this.Registers[15] = (byte)(this.Registers[vy] > this.Registers[vx] ? 1 : 0);
                            this.Registers[vx] = (byte)((this.Registers[vy] - this.Registers[vx]) & 0x00FF);
                            break;
                        case 14:
                            this.Registers[15] = (byte)(((this.Registers[vx] & 0x80) == 0x80) ? 1 : 0);
                            this.Registers[vx] <<= 1;
                            break;
                        default:
                            throw new FormatException($"Unsupported opcode: { opcode.ToString("X4") }");
                    }
                    break;
                case 0x9000:
                    if (this.Registers[(opcode & 0x0F00) >> 8] != (this.Registers[(opcode & 0x00F0) >> 4]))
                    {
                        this.PC += 2;
                    }
                    break;
                case 0xA000:
                    this.IAddress = (ushort)(opcode & 0x0FFF);
                    break;
                case 0xB000:
                    this.PC = (ushort)((opcode & 0x0FFF) + (this.Registers[0]));
                    break;
                case 0xC000:
                    this.Registers[vx] = (byte)((this.RNGenerator.Next(0, 255)) & (byte)(opcode & 0xFF));
                    break;
                case 0xD000:
                    int x = this.Registers[(opcode & 0x0F00) >> 8];
                    int y = this.Registers[(opcode & 0x00F0) >> 4];
                    int n = opcode & 0x000F;
                    this.Registers[15] = 0;

                    for (int i = 0; i < n; i++)
                    {
                        byte mem = this.Memory[this.IAddress + i];
                        for (int j = 0; j < 8; j++)
                        {
                            byte pixel = (byte)((mem >> (7 - j)) & 0x01);
                            int index = x + j + (y + i) * 64;
                            if (pixel == 1 && this.Display[index] == 1)
                            {
                                this.Registers[15] = 1;
                            }
                            this.Display[index] = (byte)(this.Display[index] ^ pixel);
                        }
                    }

                    this.DrawDisplay();
                    break;
                case 0xE000:
                    if ((opcode & 0x00FF) == 0x009E)
                    {
                        if (((this.Keyboard >> this.Registers[vx]) & 0x01) == 0x01)
                        {
                            this.PC += 2;
                            break;
                        }
                    }
                    else if ((opcode & 0x00FF) == 0x00A1)
                    {
                        if (((this.Keyboard >> this.Registers[vx]) & 0x01) != 0x01)
                        {
                            this.PC += 2;
                            break;
                        }
                    }
                    else
                    {
                        throw new FormatException($"Unsupported opcode: { opcode.ToString("X4") }");
                    }
                    break;
                case 0xF000:
                    switch (opcode & 0x00FF)
                    {
                        case 0x07:
                            this.Registers[vx] = this.DelayTimer;
                            break;
                        case 0x0A:
                            this.WaitingForKeyPress = true;
                            this.PC -= 2;
                            break;
                        case 0x15:
                            this.DelayTimer = this.Registers[vx];
                            break;
                        case 0x18:
                            this.SoundTimer = this.Registers[vx];
                            break;
                        case 0x1E:
                            this.IAddress = (ushort)(this.IAddress + this.Registers[vx]);
                            break;
                        case 0x29:
                            this.IAddress = (ushort)(this.Registers[vx] * 5);
                            break;
                        case 0x33:
                            this.Memory[this.IAddress] = (byte)(this.Registers[vx] / 100);
                            this.Memory[this.IAddress + 1] = (byte)((this.Registers[vx] % 100) / 10);
                            this.Memory[this.IAddress + 2] = (byte)(this.Registers[vx] % 10);
                            break;
                        case 0x55:
                            for (int i = 0; i <= vx; i++)
                            {
                                this.Memory[this.IAddress + i] = this.Registers[i];
                            }
                            break;
                        case 0x65:
                            for (int i = 0; i <= vx; i++)
                            {
                                this.Registers[i] = this.Memory[this.IAddress + i];
                            }
                            break;
                        default:
                            throw new FormatException($"Unsupported opcode: { opcode.ToString("X4") }");
                    }
                    break;
                default:
                    throw new FormatException($"Unsupported opcode: { opcode.ToString("X4") }");
            }
        }

        /// <summary>
        /// Draw a frame on the screen
        /// </summary>
        public void DrawDisplay()
        {
            Console.Clear();
            StringBuilder line = new StringBuilder(64);
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    if (Display[x + y * 64] != 0)
                    {
                        line.Append('*');
                    }
                    else
                    {
                        line.Append(' ');
                    }
                }
                Console.WriteLine(line);
                line.Clear();
            }
        }

        #endregion
    }
}
