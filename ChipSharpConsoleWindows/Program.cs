using SDL2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace ChipSharpConsoleWindows
{
    class Program
    {
        static void Main()
        {
            if (SDL.SDL_Init(SDL.SDL_INIT_EVERYTHING) < 0)
            {
                Console.WriteLine("SDL failed to initialize.");
                return;
            }

            int windowScale = 10;
            IntPtr window = SDL.SDL_CreateWindow("Chip-8 Interpreter",
                                     SDL.SDL_WINDOWPOS_CENTERED,
                                     SDL.SDL_WINDOWPOS_CENTERED,
                                     64 * windowScale,
                                     32 * windowScale,
                                     SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);

            IntPtr renderer = SDL.SDL_CreateRenderer(window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);

            if (renderer == IntPtr.Zero)
            {
                Console.WriteLine("SDL failed to initialize renderer.");
                return;
            }

            bool quit = false;

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

            IntPtr sdlSurface, sdlTexture = IntPtr.Zero;
            // Main loop
            while (!quit)
            {
                try
                {
                    cpu.Step();
                    while (SDL.SDL_PollEvent(out SDL.SDL_Event sdlEvent) != 0)
                    {
                        if (sdlEvent.type == SDL.SDL_EventType.SDL_QUIT)
                        {
                            quit = true;
                        }
                        else if (sdlEvent.type == SDL.SDL_EventType.SDL_KEYDOWN)
                        {
                            int key = KeyCodeToKey((int)sdlEvent.key.keysym.sym);
                            Console.WriteLine(key);
                            cpu.Keyboard |= (ushort)key;
                        }
                        else if (sdlEvent.type == SDL.SDL_EventType.SDL_KEYUP)
                        {
                            int key = KeyCodeToKey((int)sdlEvent.key.keysym.sym);
                            Console.WriteLine(key);
                            cpu.Keyboard &= (ushort)~key;
                        }
                    }
                    var displayHandle = GCHandle.Alloc(cpu.Display, GCHandleType.Pinned);
                    if (sdlTexture != IntPtr.Zero)
                    {
                        SDL.SDL_DestroyTexture(sdlTexture);
                    }
                    sdlSurface = SDL.SDL_CreateRGBSurfaceFrom(displayHandle.AddrOfPinnedObject(), 64, 32, 32, 64 * 4,
                        0x000000FF,
                        0x0000FF00,
                        0x00FF0000,
                        0xFF000000);
                    sdlTexture = SDL.SDL_CreateTextureFromSurface(renderer, sdlSurface);

                    displayHandle.Free();

                    SDL.SDL_RenderClear(renderer);
                    SDL.SDL_RenderCopy(renderer, sdlTexture, IntPtr.Zero, IntPtr.Zero);
                    SDL.SDL_RenderPresent(renderer);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            SDL.SDL_DestroyRenderer(renderer);
            SDL.SDL_DestroyWindow(window);
        }

        /// <summary>
        /// Converts a keycode to a key
        /// </summary>
        private static int KeyCodeToKey(int keycode)
        {
            int keyIndex;
            if (keycode < 58)
            {
                keyIndex = keycode - 48;
            }
            else
            {
                keyIndex = keycode - 87;
            }

            return (1 << keyIndex);
        }
    }
}
