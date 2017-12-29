using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;

namespace mihemulator8080
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private SpriteBatch spriteBatch2;
        private int ticks = 0;
        private SpriteFont font;
        private int score = 0;

        private Texture2D screenBitmap;
        private Rectangle screenArea;
        private Vector2 pos;
        private const int screenStartX = 300;
        private const int screenStartY = 200;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            Rectangle screenArea = new Rectangle(screenStartX, screenStartY, 256, 224); // size of space invaders screen. Create consts
            pos = new Vector2(screenStartX, screenStartY);
        }

        protected override void Initialize()
        {
            this.IsFixedTimeStep = false;
            this.TargetElapsedTime = new TimeSpan(0, 0, 0, 0, 128);

            CPU.instructionFecther.LoadSourceFile(@".\ROM\SpaceInvaders1978\INVADERS-H.json", SourceFileFormat.JSON_HEX);
            //Debug.Write("Next file2\n");
            CPU.instructionFecther.LoadSourceFile(@".\ROM\SpaceInvaders1978\INVADERS-G.json", SourceFileFormat.JSON_HEX);
            //Debug.Write("Next file3\n");
            CPU.instructionFecther.LoadSourceFile(@".\ROM\SpaceInvaders1978\INVADERS-f.json", SourceFileFormat.JSON_HEX);
            //Debug.Write("Next file4\n");
            CPU.instructionFecther.LoadSourceFile(@".\ROM\SpaceInvaders1978\INVADERS-E.json", SourceFileFormat.JSON_HEX);
            CPU.instructionFecther.ParseCurrentContent();

            List<string> spaceInvadersAsm = new List<string>();
            string SpaceInvadersAsmPath = @"..\..\..\..\Misc\OutputFiles\SpaceInvaders.8080asm";
            if (File.Exists(SpaceInvadersAsmPath))
            {
                File.Delete(SpaceInvadersAsmPath);
            }
            File.WriteAllLines(SpaceInvadersAsmPath, CPU.instructionFecther.FetchAllCodeLines());

            //Memory.RAM2File(); NO WRITING FOR THE TIME BEING
            //Memory.RAM2FileHTML();
            int memoryPointer = 0;
            foreach (byte _byte in CPU.instructionFecther.FetchAllCodeBytes())
            {
                Memory.RAMMemory[memoryPointer] = _byte;
                memoryPointer++;
                Memory.TextSectionSize++;
            }

            //CPU.instructionFecther.ResetInstructionIterator(); //this is overly complicated, better get it from RAM

            CPU.programCounter = 0x00;
            screenBitmap = DisplayBuffer.GenerateDisplay(GraphicsDevice); // First screencap
            base.Initialize();
        }

        protected override void LoadContent()
        {
            font = Content.Load<SpriteFont>("defaultfont");
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteBatch2 = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            //do while here

            ticks++;

            CPU.Cycle();

            screenBitmap = DisplayBuffer.GenerateDisplay(GraphicsDevice);

            //Debug.WriteLine("Ticks:" + ticks + " Pc: " + CPU.programCounter);
            //Read a instruction
            //Execute instruction

            //string instruction = CPU.instructionFecther.FetchNextInstruction().Item1;

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            
            

            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();
            spriteBatch.Draw(screenBitmap, pos, Color.White);
            string registers = "A: " + BitConverter.ToString(new byte[] { CPU.registerA })
                + "\nB: " + BitConverter.ToString(new byte[] { CPU.registerB })
                + "\nC: " + BitConverter.ToString(new byte[] { CPU.registerC })
                + "\nD: " + BitConverter.ToString(new byte[] { CPU.registerD })
                + "\nE: " + BitConverter.ToString(new byte[] { CPU.registerE })
                + "\nH: " + BitConverter.ToString(new byte[] { CPU.registerH })
                + "\nL: " + BitConverter.ToString(new byte[] { CPU.registerL });

            spriteBatch.DrawString(font, "ProgramCounter(PC): $" + CPU.programCounter.ToString() + " (Memory address of current instruction)", new Vector2(100, 100), Color.Black);
            spriteBatch.DrawString(font, "Cycle: " + ticks.ToString(), new Vector2(100, 200), Color.Black);
            spriteBatch.DrawString(font, CPU.InstructionExecuting, new Vector2(10, 250), Color.Black);
            spriteBatch.DrawString(font, registers, new Vector2(10, 300), Color.Black);

            //public static byte registerA, registerB, registerC, registerD, registerE, registerH, registerL;
            //public static bool SignFlag, ZeroFlag, AuxCarryFlag, ParityFlag, CarryFlag;
            //programcounter
            spriteBatch.End();

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}