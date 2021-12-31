using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

// GEngine Stuff
using GEngine;
using GEngine.Engine;
using GEngine.Game;
using GEngine.Net;
using static GEngine.GEngine;

// SDL Static
using static SDL2.SDL;

namespace PingPong
{
    public static class Collision
    {
        public static int CheckCollision(SceneInstance scene, Instance caller, int offsetX, int offsetY, params string[] checkCollision_ObjectName)
        {
            if (checkCollision_ObjectName.Length == 0)
                return 0;
            caller.Position.X += offsetX;
            caller.Position.Y += offsetY;
            var ret = CheckCollision(scene, caller, out _, checkCollision_ObjectName);
            caller.Position.X -= offsetX;
            caller.Position.Y -= offsetY;
            return ret;
        }
        public static int CheckCollision(SceneInstance scene, Instance caller, out List<Instance> collisions, int offsetX, int offsetY, params string[] checkCollision_ObjectName)
        {
            collisions = null;
            if (checkCollision_ObjectName.Length == 0)
                return 0;
            caller.Position.X += offsetX;
            caller.Position.Y += offsetY;
            var ret = CheckCollision(scene, caller, out List<Instance> col, checkCollision_ObjectName);
            collisions = col;
            caller.Position.X -= offsetX;
            caller.Position.Y -= offsetY;
            return ret;
        }
        public static int CheckCollision(SceneInstance scene, Instance caller, params string[] checkCollision_ObjectName)
        {
            if (checkCollision_ObjectName.Length == 0)
                return 0;
            return CheckCollision(scene, caller, out _, checkCollision_ObjectName);
        }
        public static int CheckCollision(SceneInstance scene, Instance caller, out List<Instance> collisions, params string[] checkCollision_ObjectName)
        {
            collisions = null;
            if (checkCollision_ObjectName.Length == 0)
                return 0;
            List<Instance> matches = new();
            collisions = new();

            // get instances that match the target object name
            foreach (Instance instance in scene.Instances)
            {
                foreach (string name in checkCollision_ObjectName)
                {
                    if (instance.BaseReference.ObjectName == name)
                    {
                        matches.Add(instance);
                    }
                }
            }

            // if there are none, return false
            if (matches.Count == 0) return 0;

            Size phySize = caller.PhysicsAttributes.PhysicsBodySize;

            int oTop = caller.Position.Y - (phySize.H / 2);
            int oBottom = caller.Position.Y + (phySize.H / 2);
            int oLeft = caller.Position.X - (phySize.W / 2);
            int oRight = caller.Position.X + (phySize.W / 2);

            foreach (Instance instance in matches)
            {
                Size iSize = instance.PhysicsAttributes.PhysicsBodySize;
                int iTop = instance.Position.Y - (iSize.H / 2);
                int iBottom = instance.Position.Y + (iSize.H / 2);
                int iLeft = instance.Position.X - (iSize.W / 2);
                int iRight = instance.Position.X + (iSize.W / 2);

                bool colY = oTop < iBottom && oBottom > iTop;
                bool colX = oLeft < iRight && oRight > iLeft;

                if (colY && colX)
                {
                    collisions.Add(instance);
                }
            }

            return collisions.Count;
        }
    }
}
