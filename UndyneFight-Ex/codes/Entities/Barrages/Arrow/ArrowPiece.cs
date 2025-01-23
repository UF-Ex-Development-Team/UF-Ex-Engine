using Microsoft.Xna.Framework.Graphics;
using static UndyneFight_Ex.Fight.Functions;
using static UndyneFight_Ex.MathUtil;

namespace UndyneFight_Ex.Entities
{
    public partial class Arrow
    {
        protected class ArrowPiece : Entity
        {
            public override void Draw() => FormalDraw(Image, Centre, Color.White * 0.5f * alp, Scale, GetRadian(Rotation), ImageCentre);

            public override void Update()
            {
                Centre += speed *= 0.999f;
                Rotation += rotateSpeed;
                alp -= fadeSpeed;
                if (alp < 0)
                    Dispose();
            }

            private Vector2 speed;
            private readonly float rotateSpeed, fadeSpeed;
            private float alp = 1f;
            public ArrowPiece(Vector2 speed, Vector2 pos, float rotation, Texture2D image, float scale)
            {
                Scale = scale;
                UpdateIn120 = true;
                Depth = 0.5f;
                fadeSpeed = Rand(0.04f, 0.09f);
                Rotation = rotation;
                Image = image;
                Centre = pos;
                this.speed = speed;
                rotateSpeed = Rand(2.5f, 4.5f) * RandSignal();
            }
        }
    }
}