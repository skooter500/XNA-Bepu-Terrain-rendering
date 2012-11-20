using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using BEPUphysics;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.Constraints.TwoEntity.Joints;
using BEPUphysics.Constraints.SolverGroups;
using BEPUphysics.Collidables.MobileCollidables;

using System.Collections.Generic;
using BEPUphysics.DeactivationManagement;
using BEPUphysics.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUphysics.Settings;
using BEPUphysics.DataStructures;



namespace Steering
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class XNAGame : Microsoft.Xna.Framework.Game
    {
        static XNAGame instance = null;

        float splashTime = 1000;
        float scale = 1.0f;

        BEPUphysics.Entities.Entity pickedUp = null;
        Box groundBox;
        Terrain terrain;
        bool isSplash = true;
        SpriteBatch spriteBatch;
        Texture2D brTexture;
        Texture2D crosshairs;

        public BasicEffect LineDrawer;
        
        
        public static XNAGame Instance
        {
            get { return XNAGame.instance; }
            set { XNAGame.instance = value; }
        }
        GraphicsDeviceManager graphics;

        private Random random = new Random();

        public Random Random
        {
            get { return random; }
            set { random = value; }
        }
        Space space;

        public Space Space
        {
            get { return space; }
            set { space = value; }
        }
        Cylinder cameraCylindar;

        float lastFired = 1.0f;

        public GraphicsDeviceManager Graphics
        {
            get { return graphics; }
            set { graphics = value; }
        }

        public SpriteFont Font;

        public SpriteBatch SpriteBatch1
        {
            get { return spriteBatch; }
            set { spriteBatch = value; }
        }
        private Ground ground = null;

        public Ground Ground
        {
            get { return ground; }
            set { ground = value; }
        }

        public SpriteBatch SpriteBatch
        {
            get { return spriteBatch; }
            set { spriteBatch = value; }
        }
        private Camera camera;
        List<GameEntity> children = new List<GameEntity>();

        public List<GameEntity> Children
        {
            get { return children; }
            set { children = value; }
        }
        
        public XNAGame()
        {
            instance = this;
            graphics = new GraphicsDeviceManager(this);

            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 768;
            graphics.PreferMultiSampling = true;
            graphics.SynchronizeWithVerticalRetrace = true;
            graphics.ApplyChanges();

            Content.RootDirectory = "Content";
        }
        
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            camera = new Camera();
            camera.Position = new Vector3(45.7f, 45.65f, -495.88f);
            camera.Look = new Vector3(0, 0, 1);
            camera.Right = Vector3.Cross(camera.Look, Camera.Up);
            int midX = GraphicsDeviceManager.DefaultBackBufferHeight / 2;
            int midY = GraphicsDeviceManager.DefaultBackBufferWidth / 2;
            Mouse.SetPosition(midX, midY);

                                   
            children.Add(camera);
            ground = new Ground();                        
            children.Add(ground);

            base.Initialize();
        }


        void createTower()
        {
            for (float y = 100; y > 20; y -= 5)
            {
                createBox(new Vector3(0, y, 0), 4, 4, 10);
            }
        }

        void createWall()
        {
            for (float z = -20; z < 20; z += 5)
            {
                for (float y = 60; y > 0; y -= 5)
                {
                    createBox(new Vector3(-20, y, z), 4, 4, 4);
                }
            }
        }

        void jointDemo()
        {
            BepuEntity e1;
            BepuEntity e2;
            Joint joint;

            // Ball & socket joint
            e1 = createBox(new Vector3(20, 5, -20), 1, 1, 5);
            e1.body.BecomeKinematic();
            e2 = createBox(new Vector3(20, 5, -10), 1, 1, 5);
            joint = new BallSocketJoint(e1.body, e2.body, new Vector3(20, 5, -15));
            space.Add(joint);
            
            // Hinge
            e1 = createBox(new Vector3(30, 5, -20), 1, 1, 5);
            e1.body.BecomeKinematic();
            e2 = createBox(new Vector3(30, 5, -10), 1, 1, 5);

            RevoluteJoint hinge = new RevoluteJoint(e1.body, e2.body, new Vector3(20, 5, -15), new Vector3(1, 0, 0));
            space.Add(hinge);

            // Universal
            e1 = createBox(new Vector3(40, 5, -20), 1, 1, 5);
            
            e2 = createBox(new Vector3(40, 5, -10), 1, 1, 5);

            UniversalJoint uni = new UniversalJoint(e1.body, e2.body, new Vector3(40, 5, -15));
            space.Add(uni);

            // Weld Joint
            e1 = createBox(new Vector3(50, 5, -20), 1, 1, 5);
            e2 = createBox(new Vector3(50, 5, -10), 1, 1, 5);

            WeldJoint weld = new WeldJoint(e1.body, e2.body);
            space.Add(weld);

            // PointOnLine Joint
            // create the line
            e1 = createBox(new Vector3(60, 5, -20), 1, 1, 5);
            e1.body.BecomeKinematic();
            e2 = createBox(new Vector3(60, 10, -10), 1, 1, 1);
            PointOnLineJoint pol = new PointOnLineJoint(e1.body, e2.body, new Vector3(60, 5, -20), new Vector3(0, 0, -1), new Vector3(60, 5, -10));
            space.Add(pol);
        }

        BepuEntity fireBall()
        {
            BepuEntity ball = new BepuEntity();
            ball.modelName = "sphere";
            float size = 1;
            ball.localTransform = Matrix.CreateScale(new Vector3(size, size, size));
            ball.body = new Sphere(Camera.Position + (Camera.Look * 8), size, size * 100);
            ball.diffuse = new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());
            space.Add(ball.body);
            ball.LoadContent();
            ball.configureEvents();
            ball.body.ApplyImpulse(Vector3.Zero, Camera.Look * 500);
            children.Add(ball);
            return ball;
        }

        BepuEntity createBox(Vector3 position, float width, float height, float length)
        {
            BepuEntity theBox = new BepuEntity();
            theBox.modelName = "cube";
            theBox.localTransform = Matrix.CreateScale(new Vector3(width, height, length));
            theBox.body = new Box(position, width, height, length, 1);
            theBox.diffuse = new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());
            theBox.configureEvents();
            space.Add(theBox.body);
            children.Add(theBox);
            return theBox;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>       
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            space = new Space();
            space.ForceUpdater.Gravity = new Vector3(0, -9.8f, 0);
            LineDrawer = new BasicEffect(GraphicsDevice);
            //groundBox = new Box(Vector3.Zero, ground.width, 0.1f, ground.height);
            //space.Add(groundBox);

            cameraCylindar = new Cylinder(Camera.Position, 5, 2);
            space.Add(cameraCylindar);

            SkySphere skySphere = new SkySphere();
            children.Add(skySphere);
            terrain = new Terrain();
            children.Add(terrain);

            //createTower();
            //createWall();
            //jointDemo();

            Font = Content.Load<SpriteFont>("Verdana");
            brTexture = Content.Load<Texture2D>("saveMyBabies5");
            crosshairs = Content.Load<Texture2D>("sprites_crosshairs");

            foreach (GameEntity child in children)
            {
                child.LoadContent();
            }

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            foreach (GameEntity child in children)
            {
                child.UnloadContent();
            }

        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        bool didSpawn = false;
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            float timeDelta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            KeyboardState keyState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();

            
            splashTime += timeDelta;
            if (splashTime > 5.0f)
            {
                isSplash = false;
                if (keyState.IsKeyDown(Keys.Escape))
                {
                    this.Exit();
                }
                //if (keyState.IsKeyDown(Keys.F1))
                //{
                //    if (!didSpawn)
                //    {
                //        Ray ray;
                //        ray.Position = Camera.Position;
                //        ray.Direction = Camera.Look;
                //        Vector3 point;
                //        if (ground.rayIntersects(ray, out point))
                //        {
                //            point.Y = 3;
                //            BepuEntity box = createBox(point, 5, 5, 5);
                //            box.LoadContent();
                //            didSpawn = true;
                //        }
                //    }
                //}
                //else
                //{
                //    didSpawn = false;
                //}


                //if (mouseState.LeftButton == ButtonState.Pressed)
                //{
                //    if (pickedUp == null)
                //    {
                //        Ray ray;
                //        ray.Position = Camera.Position;
                //        ray.Direction = Camera.Look;
                //        RayCastResult result;
                //        bool didDit = space.RayCast(ray, out result);
                //        if (didDit)
                //        {
                //            pickedUp = ((EntityCollidable)result.HitObject).Entity;
                //            if (pickedUp == groundBox)
                //            {
                //                pickedUp = null;
                //            }
                //        }
                //    }
                //    if (pickedUp != null)
                //    {
                //        float fDistance = 15.0f;
                //        float powerfactor = 100.0f; // Higher values causes the targets moving faster to the holding point.
                //        float maxVel = 50.0f;      // Lower values prevent objects flying through walls.

                //        // Calculate the hold point in front of the camera
                //        Vector3 holdPos = Camera.Position + (Camera.Look * fDistance);

                //        Vector3 v = holdPos - pickedUp.Position; // direction to move the Target
                //        v *= powerfactor; // powerfactor of the GravityGun

                //        if (v.Length() > maxVel)
                //        {
                //            // if the correction-velocity is bigger than maximum
                //            v.Normalize();
                //            v *= maxVel; // just set correction-velocity to the maximum
                //        }
                //        pickedUp.LinearVelocity = v;
                //    }
                //}
                //else
                //{
                //    pickedUp = null;
                //}

                if (mouseState.LeftButton == ButtonState.Pressed & lastFired > 0.25f)
                {
                    fireBall();
                    lastFired = 0.0f;
                }
             
                lastFired += timeDelta;
             
                for (int i = 0; i < children.Count; i++)
                {
                    children[i].Update(gameTime);
                }

                cameraCylindar.Position = camera.Position;
                space.Update();
            }
            
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {

            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();

            float timeDelta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (isSplash)
            {
                Vector2 centre = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
                Vector2 imgTL = new Vector2(centre.X - (brTexture.Width / 2), centre.Y - (brTexture.Height / 2));
                Vector2 imgCentre = new Vector2(brTexture.Width / 2, brTexture.Height / 2);
                spriteBatch.Draw(brTexture, imgTL + imgCentre, new Rectangle(0, 0, brTexture.Width, brTexture.Height), Color.White, 0.0f, imgCentre, scale, SpriteEffects.None, 0);
                scale += (float)(timeDelta / 40.0f);
            }
            else
            {

                foreach (GameEntity child in children)
                {
                    DepthStencilState state = new DepthStencilState();
                    state.DepthBufferEnable = true;
                    GraphicsDevice.DepthStencilState = state;
                    child.Draw(gameTime);
                }

                // Draw any lines
                Line.Draw();

                // Draw the crosshairs
                Vector2 center, origin;
                center.X = graphics.PreferredBackBufferWidth / 2;
                center.Y = graphics.PreferredBackBufferHeight / 2;
                Rectangle spriteRect = new Rectangle(76, 28, 15, 15);
                origin.X = spriteRect.Width / 2;
                origin.Y = spriteRect.Height / 2;
                spriteBatch.Draw(crosshairs, center, spriteRect, Color.Orange, 0.0f, origin, 1.0f, SpriteEffects.None, 0.0f);

                


                /*
                 ModelDrawer.Draw(Camera.getView(), Camera.getProjection());

                ConstraintDrawer.Draw(Camera.getView(), Camera.getProjection());

                LineDrawer.LightingEnabled = false;
                LineDrawer.VertexColorEnabled = true;
                LineDrawer.World = Matrix.Identity;
                LineDrawer.View = Camera.getView();
                LineDrawer.Projection = Camera.getProjection();
                ContactDrawer.Draw(LineDrawer, Space);

            
                BoundingBoxDrawer.Draw(LineDrawer, Space);
            
                SimulationIslandDrawer.Draw(LineDrawer, Space);
                 */
            }
            spriteBatch.End();
        }

        public Camera Camera
        {
            get
            {
                return camera;
            }
            set
            {
                camera = value;
            }
        }

        public GraphicsDeviceManager GraphicsDeviceManager
        {
            get
            {
                return graphics;
            }
        }
    }
}
