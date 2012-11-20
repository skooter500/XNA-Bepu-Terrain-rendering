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
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace Steering
{
    public abstract class GameEntity
    {
        public string modelName;
        public Model model = null;
        public Vector3 Position = Vector3.Zero;
        
        public Vector3 velocity = Vector3.Zero;
        public Vector3 diffuse = new Vector3(1, 1, 1);
        public Quaternion quaternion;      
        
        public Vector3 Right = new Vector3(1.0f, 0.0f, 0.0f);
        public Vector3 Up = new Vector3(0.0f, 1.0f, 0.0f);
        public Vector3 Look = new Vector3(0, 0, -1);
        public Vector3 basis = new Vector3(0, 0, -1);
        public Vector3 globalUp = new Vector3(0, 0, 1);
        public bool Alive = true;
        public float scale;
        float mass = 1.0f;

        public Matrix worldTransform = new Matrix();
        public Matrix localTransform = Matrix.Identity;
        public float Mass
        {
            get { return mass; }
            set { mass = value; }
        }
        public Vector3 force = Vector3.Zero;

        public virtual void LoadContent()
        {
            model = XNAGame.Instance.Content.Load<Model>(modelName);
        }

        public abstract void Update(GameTime gameTime);
       

        public virtual void Draw(GameTime gameTime)
        {
            if (model != null)
            {
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.EnableDefaultLighting();
                        effect.PreferPerPixelLighting = true;
                        effect.DiffuseColor = diffuse;
                        effect.World = localTransform * worldTransform;
                        effect.Projection = XNAGame.Instance.Camera.getProjection();
                        effect.View = XNAGame.Instance.Camera.getView();
                    }
                    mesh.Draw();
                }
            }
        }

        public virtual void UnloadContent()
        {
        }

        public void yaw(float angle)
        {
            Matrix T = Matrix.CreateRotationY(angle);
            Right = Vector3.Transform(Right, T);
            Look = Vector3.Transform(Look, T);
        }

        public void pitch(float angle)
        {
            Matrix T = Matrix.CreateFromAxisAngle(Right, angle);
            //_up = Vector3.Transform(_up, T);
            Look = Vector3.Transform(Look, T);
        }

        public void walk(float amount)
        {
            Position += Look * amount;
        }

        public void strafe(float amount)
        {
            
            Position += Right * amount;

        }

        public float getYaw()
        {

            Vector3 localLook = Look;
            localLook.Y = basis.Y;
            localLook.Normalize();
            float angle = (float)Math.Acos(Vector3.Dot(basis, localLook));

            if (Look.X > 0)
            {
                angle = (MathHelper.Pi * 2.0f) - angle;
            }
            return angle;

        }

        public float getPitch()
        {
            if (Look.Y == basis.Y)
            {
                return 0;
            }
            Vector3 localBasis = new Vector3(Look.X, 0, Look.Z);
            localBasis.Normalize();            
            float dot = Vector3.Dot(localBasis, Look);
            float angle = (float)Math.Acos(dot);            

            if (Look.Y < 0)
            {
                angle = (MathHelper.Pi * 2.0f) - angle;
            }

            return angle;
        }
    }
}
