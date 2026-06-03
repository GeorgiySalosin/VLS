namespace VLSGame.Config.SingleplayerSpawn
{
    // Vector3 wrapper for working with JSON
    internal class TargetPosition
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public TargetPosition() { }

        public TargetPosition(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public System.Numerics.Vector3 ToVector3() => new System.Numerics.Vector3(X, Y, Z);
    }
}
