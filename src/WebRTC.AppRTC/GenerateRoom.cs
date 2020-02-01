using System;
using System.Text;

namespace WebRTC.AppRTC
{
    public static class GenerateRoom
    {
        private static readonly Random Random = new Random();

        public static string GenerateRoomName(int length = 6)
        {
            var sb = new StringBuilder();

            for (var i = 0; i < 10; i++)
            {
                sb.Append(Random.Next(10).ToString());
            }
            
            return sb.ToString();
        }
    }
}