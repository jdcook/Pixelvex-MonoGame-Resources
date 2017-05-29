using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MGB.Trails
{
    public abstract class Trail
    {
        private const float MAX_ENDING_LENGTH = .2f;
        private const int NUM_SMOOTH_POINTS = 4;
        private readonly int originalTrailLength;

        protected MainGame Game;
        public bool Dead { get; private set; }

        public static BlendState overlapCompensationBlend = new BlendState()
        {
            AlphaBlendFunction = BlendFunction.Max,
            ColorBlendFunction = BlendFunction.Max,
            AlphaSourceBlend = Blend.One,
            AlphaDestinationBlend = Blend.One,
            ColorSourceBlend = Blend.One,
            ColorDestinationBlend = Blend.One,
        };

        private bool visible = true;
        private bool dying = false;
        protected bool Dying { get { return dying; } }
        protected int primitiveCount;
        protected VertexPositionTexture[] vertices;
        protected Effect effect;
        private EffectParameter viewParam;
        private EffectParameter projectionParam;
        private EffectParameter depthMapParam;
        protected BlendState blend = BlendState.AlphaBlend;
        protected CameraComponent camera;
        private TrailSegment[] controlPoints = new TrailSegment[4];
        private bool smoothing = false;
        private double lifeCounter;
        private bool timed = false;
        protected int usedSegments = 0;
        protected int trailLength;//length of the trail, expressed in segments
        protected float radius = .25f;
        //queue of new segments to add
        private LinkedList<TrailSegment> segmentsToAdd = new LinkedList<TrailSegment>();
        private LinkedList<TrailSegment> recycledSegments = new LinkedList<TrailSegment>();
        protected LinkedList<TrailSegment> segments = new LinkedList<TrailSegment>();
        protected LinkedListNode<TrailSegment> curHead;
        public Trail(MainGame game, int length, float radius, bool smooth)
        {
            this.Game = game;
            this.Dead = false;
            this.smoothing = smooth;
            camera = (CameraComponent)game.Services.GetService(typeof(CameraComponent));

            this.trailLength = length;
            this.originalTrailLength = trailLength;
            this.radius = radius;
            Init();
        }

        protected void SetEffect(string effectName)
        {
            effect = (Game.Services.GetService(typeof(TrailManager)) as TrailManager).GetEffect(effectName);
            viewParam = effect.Parameters["View"];
            projectionParam = effect.Parameters["Projection"];
            depthMapParam = effect.Parameters["DepthMap"];
        }

        private void RecycleQueuedSegments()
        {
            LinkedListNode<TrailSegment> curNode = segmentsToAdd.First;
            while (curNode != null)
            {
                recycledSegments.AddLast(curNode.Value);
                curNode = curNode.Next;
            }
            segmentsToAdd.Clear();
        }
        private TrailSegment GetNewQueueSegment()
        {
            TrailSegment retSeg;
            if (recycledSegments.First != null)
            {
                retSeg = recycledSegments.First.Value;
                recycledSegments.RemoveFirst();
            }
            else
            {
                retSeg = new TrailSegment(radius);
            }
            return retSeg;
        }
        protected virtual void Init()
        {
            if (vertices == null)
            {
                vertices = new VertexPositionTexture[originalTrailLength * 2];
                primitiveCount = (originalTrailLength - 1) * 2;
            }

            RecycleQueuedSegments();

            int iIndex = 0;

            for (int i = 0; i < originalTrailLength; i++)
            {
                TrailSegment newSegment = new TrailSegment(radius);
                //newSegment.Position = new Vector3(0, 0, 100);
                segments.AddLast(newSegment);

                VertexPositionTexture vVertex = new VertexPositionTexture();

                vVertex.Position = newSegment.Position;
                vertices[iIndex] = vVertex;

                vVertex.Position = newSegment.Position;
                vertices[iIndex + 1] = vVertex;

                iIndex += 2;
            }
            curHead = segments.First;

            for (int i = 0; i < controlPoints.Length; ++i)
            {
                controlPoints[i] = new TrailSegment(radius);
            }
        }

        public void AddDeathTimer(double millis)
        {
            this.lifeCounter = millis;
            timed = true;
        }

        public virtual void Update(GameTime gameTime)
        {
            if (timed)
            {
                lifeCounter -= gameTime.ElapsedGameTime.TotalMilliseconds;
                if (lifeCounter <= 0)
                {
                    KillTrail();
                    timed = false;
                }
            }


            UpdateSegments();

            if (!Dead)
            {
                UpdateVertices();
            }
        }

        public void Move(Vector3 pos, Vector3 right)
        {
            TrailSegment seg = GetNewQueueSegment();
            seg.UpdateData(pos, right, Vector3.Up);
            segmentsToAdd.AddLast(seg);
        }

        public void Move(Vector3 pos, Vector3 right, Vector3 normal)
        {
            TrailSegment seg = GetNewQueueSegment();
            seg.UpdateData(pos, right, normal);
            segmentsToAdd.AddLast(seg);
        }

        public void HideTrail()
        {
            visible = false;
        }

        public void ShowTrail()
        {
            visible = true;
        }

        public void KillTrail()
        {
            dying = true;
        }

        public void Reset()
        {
            segments.Clear();
            Init();
            Dead = false;
            dying = false;
            trailLength = originalTrailLength;
        }

        private void UpdateSegments()
        {
            if (segments.Count == 0)
                return;

            if (dying && usedSegments >= trailLength)
            {
                LinkedListNode<TrailSegment> toRemove = curHead;
                curHead = curHead.Next;
                if (curHead == null)
                {
                    curHead = segments.First;
                    if (curHead == toRemove)
                    {
                        Dead = true;
                    }
                }
                segments.Remove(toRemove);
                --trailLength;
                --usedSegments;
            }

            if (segments.Count == 0)
            {
                return;
            }

            //allows the trail to close on its head when it is not moving anymore
            if (segmentsToAdd.Count == 0)
            {
                LinkedListNode<TrailSegment> f = curHead.Previous;
                if (f == null)
                {
                    f = segments.Last;
                }

                Move(f.Value.Position, f.Value.Right);
            }
            /*
             * smooths one vertex behind the head. It has to be this way to use a catmull rom spline, because you need
             * two control points before and after the interpolated point. This is rendered quickly enough that it isn't noticable.
             * (could also look into Bezier curves)
             */
            LinkedListNode<TrailSegment> curNode = segmentsToAdd.First;
            while(curNode != null)
            {
                if (smoothing)
                {
                    //control points for interpolation
                    controlPoints[0].Copy(controlPoints[1]);
                    controlPoints[1].Copy(controlPoints[2]);
                    controlPoints[2].Copy(controlPoints[3]);
                    controlPoints[3].UpdateData(curNode.Value.Position, curNode.Value.Right, curNode.Value.Normal);
                }
                TrailSegment seg;
                if (smoothing && usedSegments >= controlPoints.Length && controlPoints[1].Right != controlPoints[2].Right)
                {
                    //the head is currently one behind the newest point. move to the point before that, because we are adding
                    //vertices between control points 2 and 3.
                    curHead = GetPrev(curHead);
                    for (int j = 1; j <= NUM_SMOOTH_POINTS; ++j)
                    {
                        float lerpAmt = (float)j / ((float)(NUM_SMOOTH_POINTS + 1.0f));

                        seg = curHead.Value;
                        Vector3 lerpPos = Vector3.CatmullRom(controlPoints[0].Position, controlPoints[1].Position, controlPoints[2].Position, controlPoints[3].Position, lerpAmt);
                        Vector3 lerpDir = Vector3.CatmullRom(controlPoints[0].Right, controlPoints[1].Right, controlPoints[2].Right, controlPoints[3].Right, lerpAmt);
                        Vector3 lerpNormal = Vector3.CatmullRom(controlPoints[0].Normal, controlPoints[1].Normal, controlPoints[2].Normal, controlPoints[3].Normal, lerpAmt);
                        seg.UpdateData(lerpPos, lerpDir, lerpNormal);
                        curHead = GetNext(curHead);

                        if (usedSegments < trailLength)
                        {
                            ++usedSegments;
                        }
                    }

                    //add the head we overwrote back to the front
                    seg = curHead.Value;
                    seg.UpdateData(controlPoints[2].Position, controlPoints[2].Right, controlPoints[2].Normal);
                    curHead = GetNext(curHead);
                }

                //add the newest point to the front
                seg = curHead.Value;
                seg.UpdateData(curNode.Value.Position, curNode.Value.Right, curNode.Value.Normal);
                curHead = GetNext(curHead);

                if (usedSegments < trailLength)
                {
                    ++usedSegments;
                }

                curNode = curNode.Next;
            }

            RecycleQueuedSegments();
        }

        protected LinkedListNode<TrailSegment> GetNext(LinkedListNode<TrailSegment> cur)
        {
            LinkedListNode<TrailSegment> next = cur.Next;
            if (next == null)
            {
                next = segments.First;
            }
            return next;
        }

        protected LinkedListNode<TrailSegment> GetPrev(LinkedListNode<TrailSegment> cur)
        {
            if (cur == null)
            {
                return null;
            }

            LinkedListNode<TrailSegment> prev = cur.Previous;
            if (prev == null)
            {
                prev = segments.Last;
            }
            return prev;
        }

        protected abstract void UpdateVertices();

        public virtual void Draw(CameraComponent camera)
        {
            if (visible)
            {
                Game.GraphicsDevice.DepthStencilState = DepthStencilState.None;
                Game.GraphicsDevice.BlendState = blend;
                Game.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
                Game.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

                viewParam.SetValue(camera.View);
                projectionParam.SetValue(camera.Projection);
                depthMapParam.SetValue(camera.DepthRT);
                effect.CurrentTechnique.Passes[0].Apply();

                Game.GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>
                (
                    PrimitiveType.TriangleStrip,
                    vertices,
                    0,
                    primitiveCount
                );
            }
        }
    }

    public class TrailSegment
    {
        public float Radius { get; private set; }
        public Vector3 Position { get; private set; }
        public Vector3 Right { get; private set; }
        public Vector3 Normal { get; private set; }

        public TrailSegment(float fRadius)
        {
            Radius = fRadius;
            Position = new Vector3(0);
            Right = Vector3.Forward;
            Normal = Vector3.Up;
        }

        public TrailSegment(float radius, Vector3 position, Vector3 radiusDirection, Vector3 normal)
        {
            this.Radius = radius;
            this.Position = position;
            this.Right = radiusDirection;
            this.Normal = normal;
        }

        public TrailSegment(TrailSegment toCopy)
        {
            this.Radius = toCopy.Radius;
            this.Position = toCopy.Position;
            this.Right = toCopy.Right;
            this.Normal = toCopy.Normal;
        }

        public void Copy(TrailSegment other)
        {
            this.Radius = other.Radius;
            this.Position = other.Position;
            this.Right = other.Right;
            this.Normal = other.Normal;
        }

        public void UpdateData(Vector3 position, Vector3 radiusDirection, Vector3 normal)
        {
            this.Position = position;
            this.Right = radiusDirection;
            this.Normal = normal;
        }
    }
}
