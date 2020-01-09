using System;

namespace AirjME
{
    public struct Position
    {
        public float x, y, z, f;

        public Position(float x, float y, float z = 0, float f = 0)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.f = f;
        }

        public float FaceTo(Position position)
        {
            return MathF.Atan2((position.y - y), (position.x - x));
        }

        public float DistanceTo2d(Position position)
        {
            return MathF.Sqrt((position.x - x) * (position.x - x) + (position.y - y) * (position.y - y));
        }
    }
}