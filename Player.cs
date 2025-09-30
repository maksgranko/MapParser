namespace MapParser
{
    public class Player
    {
        public string Name { get; set; }
        public string KeyUUID { get; set; }
        public float RotationAngle { get; set; }
        public float[] Position { get; set; }
        public float Health { get; set; }
        public float Armor { get; set; }
        public string Dimension { get; set; }
    }
}