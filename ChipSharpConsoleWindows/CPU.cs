using System;
using System.Collections.Generic;

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
            this.IAddress = 0x0000;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Executes an opcode
        /// </summary>
        /// <param name="opcode">The opcode to execute</param>
        public void ExecuteOpcode(ushort opcode)
        {
            ushort nibble = (ushort)(opcode & 0xF000);
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
                        this.Stack.Pop();
                    }
                    break;
                case 0x1000:
                    this.IAddress = (ushort)(opcode & 0x0FFF);
                    break;
                case 0x2000:
                    this.Stack.Push(IAddress);
                    this.IAddress = (ushort)(opcode & 0x0FFF);
                    break;
                case 0x3000:
                    if (this.Registers[(opcode & 0x0F00) >> 8] == (opcode & 0x00FF))
                    {
                        this.IAddress += 2;
                    }
                    break;
                case 0x4000:
                    if (this.Registers[(opcode & 0x0F00) >> 8] != (opcode & 0x00FF))
                    {
                        this.IAddress += 2;
                    }
                    break;
                case 0x5000:
                    if ((this.Registers[(opcode & 0x0F00) >> 8]) == (this.Registers[(opcode & 0x00F0) >> 4]))
                    {
                        this.IAddress += 2;
                    }
                    break;
                case 0x6000:
                    (this.Registers[(opcode & 0x0F00) >> 8]) = (byte)(opcode & 0x00FF);
                    break;
                case 0x7000:
                    (this.Registers[(opcode & 0x0F00) >> 8]) += (byte)(opcode & 0x00FF);
                    break;
                case 0x8000:
                    byte vx = (byte)((opcode & 0x0F00) >> 8);
                    byte vy = (byte)((opcode & 0x00F0) >> 4);
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
                    }
                    break;
                case 0x9000:
                    if (this.Registers[(opcode & 0x0F00) >> 8] != (this.Registers[(opcode & 0x00F0) >> 4]))
                    {
                        this.IAddress += 2;
                    }
                    break;
                case 0xA000:
                    this.IAddress = (ushort)(opcode & 0x0FFF);
                    break;
                default:
                    throw new FormatException($"Unsupported opcode: { opcode.ToString("X4") }");
            }
        }

        #endregion
    }
}
