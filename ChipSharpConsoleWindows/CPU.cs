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
            switch (opcode)
            {
                case 0x0000:
                    break;
                case 0x00E0:
                    // Clears the screen.
                    Array.Clear(this.Display, 0, this.Display.Length);
                    break;
                case 0x00EE:
                    // Returns from a subroutine.
                    this.Stack.Pop();
                    break;
                default:
                    throw new FormatException($"Unsupported opcode: { opcode.ToString("X4") }");
            }
        }

        #endregion
    }
}
