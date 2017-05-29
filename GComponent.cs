using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace MGB
{
    /// <summary>
    /// Base class for all components.
    /// Each type of component has its
    /// own manager to take care of it
    /// and call any extra methods needed
    /// (e.g. Draw() for DrawableComponent3D)
    /// </summary>
    public abstract class GComponent
    {
        protected MainGame Game;
        public GEntity Entity { get; private set; }
        public bool Remove { get; protected set; }

        /// <summary>
        /// kills this component by telling the manager to remove it on the next update
        /// </summary>
        public void KillComponent()
        {
            Remove = true;
        }
        public GComponent(MainGame game, GEntity entity)
        {
            this.Game = game;
            this.Entity = entity;
            Remove = false;
        }

        /// <summary>
        /// called when this component is added to a manager's entity list
        /// </summary>
        public virtual void Start()
        {
            Remove = false;
        }

        public virtual void Update(GameTime gameTime)
        {

        }

        /// <summary>
        /// called when this component is removed (on the update after KillComponent() has been called)
        /// </summary>
        public virtual void End()
        {

        }
    }
}
