using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GEngine;
using GEngine.Engine;
using GEngine.Game;
using static GEngine.GEngine;

using static SDL2.SDL;

namespace PingPong.GameObjects
{
    public class Paddle : GameObject
    {
        public static readonly Size PADDLE_SIZE = new Size(14, 80);
        public static readonly ColorRGBA PADDLE_COLOR = ColorRGBA.WHITE;
        public static readonly int COLLISION_WIDTH = PADDLE_SIZE.W;
        public Paddle()
        {
            // object definition
            ObjectName = "obj_paddle";
            Type = typeof(Paddle);
            DefaultImageIndex = 0;
            DefaultImageSpeed = 0;
            DefaultSprite = null;
            DefaultOffset = new(PADDLE_SIZE.W/2, PADDLE_SIZE.H/2);

            // physics
            DefaultPhysicsAttributes.BodyType = BodyType.Dynamic;
            DefaultPhysicsAttributes.PhysicsBodySize = new(COLLISION_WIDTH, PADDLE_SIZE.H);
            DefaultPhysicsAttributes.PhysicsBodyType = PhysicsBodyType.Box;
        }

        public override void OnCreate(Instance caller, SceneInstance scene)
        {
            caller["serving"] = false;
            base.OnCreate(caller, scene);
        }

        public override void OnDestroy(Instance caller, SceneInstance scene)
        {
            base.OnDestroy(caller, scene);
        }

        public override void OnDraw(Instance caller, SceneInstance scene, GraphicsEngine graphics)
        {
            int x1 = caller.Position.X - PADDLE_SIZE.W / 2;
            int y1 = caller.Position.Y - PADDLE_SIZE.H / 2;
            int x2 = caller.Position.X + PADDLE_SIZE.W / 2;
            int y2 = caller.Position.Y + PADDLE_SIZE.H / 2;
            var color = graphics.GetRendererDrawColor();
            graphics.SetRenderDrawColor(PADDLE_COLOR);
            graphics.DrawRectangleFilled(x1, y1, x2, y2);
            graphics.SetRenderDrawColor(color);
            base.OnDraw(caller, scene, graphics);
        }

        public override void Step(Instance caller, SceneInstance scene)
        {
            base.Step(caller, scene);
        }
    }
}
