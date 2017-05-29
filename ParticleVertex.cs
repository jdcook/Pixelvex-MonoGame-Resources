using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace MGB
{
    /// <summary>
    /// Custom vertex structure for drawing particles.
    /// </summary>
    struct ParticleVertex
    {
        // Stores which corner of the particle quad this vertex represents.
        public Vector2 Corner;

        // Stores the starting position of the particle.
        public Vector3 Position;

        // Stores the starting velocity of the particle.
        public Vector3 Velocity;

        // Four random values, used to make each particle look slightly different.
        public Color Random;

        // The time (in seconds) at which this particle was created.
        public float Time;

        //Stores the percent of the normal size this particle will be
        public float SizePercent;

        // Describe the layout of this vertex structure.
        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),

            new VertexElement(8, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),

            new VertexElement(20, VertexElementFormat.Vector3, VertexElementUsage.Normal, 1),

            new VertexElement(32, VertexElementFormat.Color, VertexElementUsage.Color, 0),

            new VertexElement(36, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 0),

            new VertexElement(40, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 1)
        );


        // Describe the size of this vertex structure.
        public const int SizeInBytes = 44;
    }
}
