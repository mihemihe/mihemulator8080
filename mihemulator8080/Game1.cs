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
        private Texture2D RAMBitmap;
        private Vector2 pos;
        private Vector2 posRAM;
        private const int screenStartX = 350;
        private const int screenStartY = 410;

        private Texture2D oneCycle;
        private Texture2D loopCycles;
        private Texture2D RAMMeter;
        private Rectangle positionOneCycleButton;
        private bool oneCycleHover;

        private KeyboardState keyboardState;
        private MouseState mouseState;
        private MouseState oldMouseState;

        private int leftButtonPressed;
        bool clickOneCycle;

        private float RotationAngle;
        private float rotationAngleRAM;
        private Vector2 origin;

        private Rectangle RAMarea;


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1850;  // set this value to the desired width of your window
            graphics.PreferredBackBufferHeight = 800;   // set this value to the desired height of your window
            graphics.ApplyChanges();
            Content.RootDirectory = "Content";
            //Rectangle screenArea = new Rectangle(screenStartX, screenStartY, 256, 224); // size of space invaders screen. Create consts //TODO is this use? otherwise remove
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
            rotationAngleRAM = ((float)Math.PI * 3) / 2.0f;

            //Rectangle RAMarea = new Rectangle(500, 100, 1024, 512); // area RAM
            posRAM = new Vector2(20, 780);
        }

        protected override void Initialize()
        {
            this.IsMouseVisible = true;

            this.IsFixedTimeStep = true;
            this.TargetElapsedTime = new TimeSpan(4000); // only if IsFixedTimeStep is true
            bool testROM = true;

            if(testROM)
            {
                CPU.instructionFecther.LoadSourceFile(@".\ROM\SpaceInvaders1978\TEST-ROM2.json", SourceFileFormat.JSON_HEX);
            }
            else
            {
                CPU.instructionFecther.LoadSourceFile(@".\ROM\SpaceInvaders1978\INVADERS-H.json", SourceFileFormat.JSON_HEX);
                //Debug.Write("Next file2\n");
                CPU.instructionFecther.LoadSourceFile(@".\ROM\SpaceInvaders1978\INVADERS-G.json", SourceFileFormat.JSON_HEX);
                //Debug.Write("Next file3\n");
                CPU.instructionFecther.LoadSourceFile(@".\ROM\SpaceInvaders1978\INVADERS-f.json", SourceFileFormat.JSON_HEX);
                //Debug.Write("Next file4\n");
                CPU.instructionFecther.LoadSourceFile(@".\ROM\SpaceInvaders1978\INVADERS-E.json", SourceFileFormat.JSON_HEX);
                CPU.instructionFecther.ParseCurrentContent();
            }


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

            DisplayBuffer.Init(GraphicsDevice, Color.Black, Color.White);
            DisplayBuffer.GenerateDisplay(); //Inverted colours. This is bad test, first frame will come emtpy always

            DisplayBuffer.GenerateRAMDisplay();

            screenBitmap = DisplayBuffer.videoTexture; // First screencap
            RAMBitmap = DisplayBuffer.RAMtexture; // First screencap



            base.Initialize();
        }

        protected override void LoadContent()
        {
            font = Content.Load<SpriteFont>("defaultfont");
            oneCycle = Content.Load<Texture2D>("oneCycle");
            loopCycles = Content.Load<Texture2D>("loopCycles");
            RAMMeter = Content.Load<Texture2D>("RAMBar2");
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

                if (CPU.cyclesCounter % 100 == 0)
                {
                    DisplayBuffer.GenerateRAMDisplay();
                }
                
                    
                
                CPU.Cycle();
                CPU.cyclesCounter++;
                CPU.cyclesPerSecond++;

                if (CPU.cyclesCounter == 49000)
                {
                    this.TargetElapsedTime = new TimeSpan(4000);
                }
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
            spriteBatch.Draw(RAMMeter, new Rectangle(20, 475, 800, 50), Color.White);
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
            spriteBatch.DrawString(font, registers, new Vector2(10, 240), Color.Black);

            string flags = "FLAGS"
                 + "\nZERO:       " + CPU.ZeroFlag.ToString()
                 + "\nSIGN:       " + CPU.SignFlag.ToString()
                 + "\nPARITY:     " + CPU.ParityFlag.ToString()
                 + "\nCARRY:      " + CPU.CarryFlag.ToString()
                 + "\nAUX. CARRY: " + CPU.AuxCarryFlag.ToString();
            spriteBatch.DrawString(font, flags, new Vector2(120, 240), Color.Black);

            // Cycles per second
            spriteBatch.DrawString(font, "Cyles per second: " + CPU.CPS, new Vector2(140, 400), Color.Black);
            spriteBatch.DrawString(font, "Subroutine: " + CPU.comment, new Vector2(140, 420), Color.Black);

            // Display!!
            spriteBatch.Draw(screenBitmap, pos, null, Color.White, RotationAngle, origin, 1.0f, SpriteEffects.None, 0f);


            //Display RAM
            spriteBatch.Draw(RAMBitmap, posRAM, null, Color.White, rotationAngleRAM, origin, 1.0f, SpriteEffects.None, 0f);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}