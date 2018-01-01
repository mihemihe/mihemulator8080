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

        //private SpriteBatch spriteBatch2;
        

        private SpriteFont font;


        private Texture2D screenBitmap;
        private Vector2 pos;
        private const int screenStartX = 400;
        private const int screenStartY = 450;

        private Texture2D oneCycle;
        private Texture2D loopCycles;
        private Rectangle positionOneCycleButton;
        private bool oneCycleHover;

        private KeyboardState keyboardState;
        private MouseState mouseState;
        private MouseState oldMouseState;

        private int leftButtonPressed;
        bool clickOneCycle;

        private float RotationAngle;
        private Vector2 origin;


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            Rectangle screenArea = new Rectangle(screenStartX, screenStartY, 256, 224); // size of space invaders screen. Create consts
            pos = new Vector2(screenStartX, screenStartY);
            positionOneCycleButton = new Rectangle(10, 10, 40, 50);
            oneCycleHover = false;
            mouseState = Mouse.GetState();
            oldMouseState = Mouse.GetState();
            leftButtonPressed = 0;
            clickOneCycle = false;
            origin.X = 0;
            origin.Y = 0;
            RotationAngle = -3.14F / 2;
        }

        protected override void Initialize()
        {
            this.IsMouseVisible = true;

            this.IsFixedTimeStep = true;
            this.TargetElapsedTime = new TimeSpan(800); // only if IsFixedTimeStep is true

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

            Memory.RAM2File(); //NO WRITING FOR THE TIME BEING
            Memory.RAM2FileHTML();
            int memoryPointer = 0;
            foreach (byte _byte in CPU.instructionFecther.FetchAllCodeBytes())
            {
                Memory.RAMMemory[memoryPointer] = _byte;
                memoryPointer++;
                Memory.TextSectionSize++;
            }

            //CPU.instructionFecther.ResetInstructionIterator(); //this is overly complicated, better get it from RAM

            DisplayBuffer.Init(GraphicsDevice, Color.Red, Color.White);
            DisplayBuffer.GenerateDisplay(); //Inverted colours. This is bad test, first frame will come emtpy always
            screenBitmap = DisplayBuffer.videoTexture; // First screencap



            base.Initialize();
        }

        protected override void LoadContent()
        {
            font = Content.Load<SpriteFont>("defaultfont");
            oneCycle = Content.Load<Texture2D>("oneCycle");
            loopCycles = Content.Load<Texture2D>("loopCycles");
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            //spriteBatch2 = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            
            keyboardState = Keyboard.GetState();
            mouseState = Mouse.GetState();
            if (keyboardState.IsKeyDown(Keys.Escape))
                Exit();

            if (mouseState.X < positionOneCycleButton.X + positionOneCycleButton.Width &&
                mouseState.X > positionOneCycleButton.X &&
                mouseState.Y < positionOneCycleButton.Y + positionOneCycleButton.Height &&
                mouseState.Y > positionOneCycleButton.Y)
            {
                oneCycleHover = true;
            }
            else oneCycleHover = false;

            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                leftButtonPressed++;
            }
            else leftButtonPressed = 0;

            clickOneCycle = (oneCycleHover &&
                mouseState.LeftButton == ButtonState.Released &&
                oldMouseState.LeftButton == ButtonState.Pressed);
            
            if ( true || clickOneCycle || leftButtonPressed > 20 || keyboardState.IsKeyDown(Keys.P))
            {
                CPU.Cycle();
                CPU.cyclesCounter++;
                CPU.cyclesPerSecond++;
            }




            //Debug.WriteLine("Ticks:" + ticks + " Pc: " + CPU.programCounter);
            //Read a instruction
            //Execute instruction

            //string instruction = CPU.instructionFecther.FetchNextInstruction().Item1;

            // TODO: Add your update logic here
            oldMouseState = mouseState;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {

            //Update VideoBuffer
            DisplayBuffer.GenerateDisplay();
            screenBitmap = DisplayBuffer.videoTexture;

            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            // GUI elements
            spriteBatch.Draw(oneCycle, positionOneCycleButton, Color.White);
            spriteBatch.Draw(loopCycles, new Rectangle(280, 10, 80, 50), Color.White);
            spriteBatch.DrawString(font, "Step ONE Instruction          Cycle automatically", new Vector2(10, 70), Color.Black);
            // PC, cycles and next instruction
            spriteBatch.DrawString(font, "ProgramCounter(PC): $" + CPU.programCounter.ToString("X4") + " (Memory address of current instruction)", new Vector2(10, 100), Color.Black);
            spriteBatch.DrawString(font, "StackPointer(SP): $" + CPU.stackPointer.ToString("X4") + " (Memory address of stack pointer)", new Vector2(10, 120), Color.Black);
            spriteBatch.DrawString(font, "Cycle: " + CPU.cyclesCounter.ToString(), new Vector2(10, 140), Color.Black);
            spriteBatch.DrawString(font, "Last executed CPU instruction:\n" + CPU.InstructionExecuting, new Vector2(10, 160), Color.Black);



            // CPU Registers
            string registers = "REGISTERS"
                + "\nA: " + BitConverter.ToString(new byte[] { CPU.registerA })
                + "\nB: " + BitConverter.ToString(new byte[] { CPU.registerB })
                + "\nC: " + BitConverter.ToString(new byte[] { CPU.registerC })
                + "\nD: " + BitConverter.ToString(new byte[] { CPU.registerD })
                + "\nE: " + BitConverter.ToString(new byte[] { CPU.registerE })
                + "\nH: " + BitConverter.ToString(new byte[] { CPU.registerH })
                + "\nL: " + BitConverter.ToString(new byte[] { CPU.registerL });
            spriteBatch.DrawString(font, registers, new Vector2(10, 300), Color.Black);

            string flags = "FLAGS"
                 + "\nZERO:       " + CPU.ZeroFlag.ToString()
                 + "\nSIGN:       " + CPU.SignFlag.ToString()
                 + "\nPARITY:     " + CPU.ParityFlag.ToString()
                 + "\nCARRY:      " + CPU.CarryFlag.ToString()
                 + "\nAUX. CARRY: " + CPU.AuxCarryFlag.ToString();
            spriteBatch.DrawString(font, flags, new Vector2(120, 300), Color.Black);

            // Cycles per second
            spriteBatch.DrawString(font, "Cyles per second: " + CPU.CPS, new Vector2(140, 440), Color.Black);

            // Display!!
            
 
        
            spriteBatch.Draw(screenBitmap, pos, null, Color.White, RotationAngle, origin, 1.0f, SpriteEffects.None, 0f);
            spriteBatch.End();

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}