using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics;
using BEPUphysics.Entities;
using SkinnedModelLib;

namespace MGB.Trails
{
    public class ExampleTrail : Trail
    {
        private const string STR_ATTACH_BONE = "sword";
        private AnimatedModelComponent model;
        public ExampleTrail(MainGame game, AnimatedModelComponent model)
            : base(game, 80, .75f, true)
        {
            this.model = model;
            SetEffect(TrailManager.STR_SWORD);
        }

        public override void Update(GameTime gameTime)
        {
            if (!controller.IsInHitLag())
            {
                if (!Dying)
                {
                    Matrix transform = model.GetBoneTransform(STR_ATTACH_BONE);
                    Vector3 newPos = transform.Translation + transform.Right * .6f;
                    Move(newPos, -transform.Right, transform.Forward);
                }

                base.Update(gameTime);
            }
        }
    }
}
