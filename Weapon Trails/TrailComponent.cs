using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics;
using BEPUphysics.Entities;

namespace MGB.Trails
{
    public class TrailComponent : GComponent
    {
        public TrailComponent(MainGame game, GEntity entity)
            : base(game, entity)
        {

        }

        List<Trail> trails = new List<Trail>();
        public override void Update(GameTime gameTime)
        {
            for (int i = 0; i < trails.Count; ++i)
            {
                if (!trails[i].Dead)
                {
                    trails[i].Update(gameTime);
                }
            }
        }

        public void StopAllTrails()
        {
            for (int i = 0; i < trails.Count; ++i)
            {
                trails[i].KillTrail();
            }
        }

        public void AddTrail(Trail t)
        {
            trails.Add(t);
        }

        
        public Trail TryRecycle<T>()
        {
            for (int i = 0; i < trails.Count; ++i)
            {
                Trail temp = trails[i];
                if(temp.Dead && temp is T)
                {
                    trails[i].Reset();
                    return trails[i];
                }
            }

            return null;
        }

        public void Draw(CameraComponent camera)
        {
            foreach (Trail t in trails)
            {
                if (!t.Dead)
                {
                    t.Draw(camera);
                }
            }
        }
    }
}
