﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;


namespace mihemulator8080
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        int ticks = 0;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            CPU.instructionFecther.LoadSourceFile(@".\ROM\SpaceInvaders1978\INVADERS-H.json", SourceFileFormat.JSON_HEX);
            Debug.Write("Next file2\n");
            CPU.instructionFecther.LoadSourceFile(@".\ROM\SpaceInvaders1978\INVADERS-G.json", SourceFileFormat.JSON_HEX);
            Debug.Write("Next file3\n");
            CPU.instructionFecther.LoadSourceFile(@".\ROM\SpaceInvaders1978\INVADERS-f.json", SourceFileFormat.JSON_HEX);
            Debug.Write("Next file4\n");
            CPU.instructionFecther.LoadSourceFile(@".\ROM\SpaceInvaders1978\INVADERS-E.json", SourceFileFormat.JSON_HEX);
            CPU.instructionFecther.ParseCurrentContent();

            List<string> spaceInvadersAsm = new List<string>();
            string SpaceInvadersAsmPath = @"..\..\..\..\Misc\OutputFiles\SpaceInvaders.8080asm";
            if (File.Exists(SpaceInvadersAsmPath))
            {
                File.Delete(SpaceInvadersAsmPath);
            }
            File.WriteAllLines(SpaceInvadersAsmPath, CPU.instructionFecther.FetchAllCodeLines());

            int memoryPointer = 0;
            foreach (byte _byte in CPU.instructionFecther.FetchAllCodeBytes())
            {
                Memory.RAMMemory[memoryPointer] = _byte;
                memoryPointer++;
            }

            do
            {
                ticks++;

                //string instruction = CPU.instructionFecther.FetchNextInstruction().Item1;

            } while (true);



            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}