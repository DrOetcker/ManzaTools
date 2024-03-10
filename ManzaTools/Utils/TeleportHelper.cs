using CounterStrikeSharp.API.Modules.Utils;

namespace ManzaTools.Utils
{
    public static class TeleportHelper
    {

        public static QAngle GetAngleFromJsonString(string playerAngle)
        {
            var coordinates = playerAngle.Split(' ');
            return new QAngle(float.Parse(coordinates[0]), float.Parse(coordinates[1]), float.Parse(coordinates[2]));
        }

        public static Vector GetVectorFromJsonString(string playerPosition)
        {
            var coordinates = playerPosition.Split(' ');
            return new Vector(float.Parse(coordinates[0]), float.Parse(coordinates[1]), float.Parse(coordinates[2]));
        }
    }
}
