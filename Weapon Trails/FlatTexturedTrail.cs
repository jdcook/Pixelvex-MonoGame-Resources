using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;


namespace MGB.Trails
{
    public abstract class FlatTexturedTrail : Trail
    {
        protected Vector3 normal = Vector3.Up;
        private float TextureRepetition = 1;
        public FlatTexturedTrail(MainGame game, int length, float radius, bool smooth, float textureRepetition = 1)
            : base(game, length, radius, smooth)
        {
            this.TextureRepetition = textureRepetition;
        }

        protected override void UpdateVertices()
        {
            TrailSegment curSegment;
            TrailSegment prevSegment;
            int vertexIndex = 0;

            LinkedListNode<TrailSegment> curNode = curHead.Previous;
            if (curNode == null)
            {
                curNode = segments.Last;
            }

            int segmentIndex = vertexIndex / 2;
            do
            {
                segmentIndex = vertexIndex / 2;
                curSegment = curNode.Value;
                prevSegment = GetNext(curNode).Value;
                /*
                if (isShrinking)
                {
                    float fRatio = 1 - (1 / ((float)trailLength) * (segmentIndex + 1));
                    curSegment.Radius = fRatio * radius;
                }
                */
                //Vector3 right = Vector3.Cross(curSegment.Right, curSegment.Normal);

                vertices[vertexIndex].Position = curSegment.Position - curSegment.Right * curSegment.Radius;
                vertices[vertexIndex + 1].Position = curSegment.Position + curSegment.Right * curSegment.Radius;

                float fStep = (segmentIndex + 1) * (TextureRepetition / trailLength);
                if (fStep == 1)
                {
                    fStep = .99f;
                }
                vertices[vertexIndex].TextureCoordinate.X = 0;
                vertices[vertexIndex].TextureCoordinate.Y = fStep;
                vertices[vertexIndex + 1].TextureCoordinate.X = .99f;// because for some reason, 1 started wrapping to the start of the texture
                vertices[vertexIndex + 1].TextureCoordinate.Y = fStep;

                vertexIndex += 2;

                if (curNode == curHead)
                {
                    curNode = null;
                    break;
                }

                curNode = GetPrev(curNode);
            }
            while (curNode != null && segmentIndex < usedSegments - 1);


            int indexToCopy = vertexIndex - 2;
            //if we've started destroying segments, udpate all discarded vertices to the front position
            while (vertexIndex < vertices.Length)
            {
                vertices[vertexIndex].Position = vertices[indexToCopy].Position;
                vertices[vertexIndex + 1].Position = vertices[indexToCopy + 1].Position;

                vertices[vertexIndex].TextureCoordinate.X = 0;
                vertices[vertexIndex].TextureCoordinate.Y = 1;
                vertices[vertexIndex + 1].TextureCoordinate.X = 1;
                vertices[vertexIndex + 1].TextureCoordinate.Y = 1;

                vertexIndex += 2;
            }
        }
    }
}
