using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using bepu = BEPUutilities;
using MGB.Trails;

namespace MGB
{
    public class TrailManager : GameComponent
    {
        /*
         * A simple manager to update and draw all Trail components. Also initializes and stores
         * the Effects for each kind of trail.
         */
        public const string STR_SWORD = "sword";

        private Effect texturedEffect;
        private Effect vertexColorEffect;
        private Dictionary<string, Effect> effects;
        private CameraComponent camera;
        private LinkedList<TrailComponent> trails = new LinkedList<TrailComponent>();
        public TrailManager(MainGame game)
            : base(game)
        {
            this.camera = (CameraComponent)game.Services.GetService(typeof(CameraComponent));
        }

        public override void Initialize()
        {
            effects = new Dictionary<string, Effect>();

            texturedEffect = (Game as MainGame).LoadEffect("TrailTextureEffect");
            texturedEffect.Parameters["alpha"].SetValue(1.0f);

            Effect swordTrail = texturedEffect.Clone();
            swordTrail.Parameters["colorTint"].SetValue(new Vector3(1, 1, 1));
            swordTrail.Parameters["Texture"].SetValue(Game.Content.Load<Texture2D>("Textures\\sword_trail"));
            effects.Add(STR_SWORD, swordTrail);
        }

        public Effect GetEffect(string name)
        {
            return effects[name];
        }

        public override void Update(GameTime gameTime)
        {
            LinkedListNode<TrailComponent> curNode = trails.First;
            while (curNode != null)
            {
                curNode.Value.Update(gameTime);
                if (curNode.Value.Remove)
                {
                    LinkedListNode<TrailComponent> nodeToRemove = curNode;
                    curNode = curNode.Next;
                    nodeToRemove.Value.End();
                    trails.Remove(nodeToRemove);
                }
                else
                {
                    curNode = curNode.Next;
                }
            }

            base.Update(gameTime);
        }

        public void AddComponent(TrailComponent toAdd)
        {
            toAdd.Start();
            trails.AddLast(toAdd);
        }


        public void Draw()
        {
            Game.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            foreach (TrailComponent t in trails)
            {
                t.Draw(camera);
            }
        }
    }
}
