using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GEngine;
using GEngine.Engine;
using GEngine.Game;
using static GEngine.GEngine;
using static PingPong.Tools;

using static SDL2.SDL;

namespace PingPong.GameObjects
{
    public class Ball : GameObject
    {
        public static readonly int DEMO_MIN_SPEED = 10, DEMO_MAX_SPEED = 24;
        public static readonly Size BALL_SIZE = new Size(13, 13);
        public static readonly ColorRGBA BALL_COLOR = new ColorRGBA(255, 255, 255);//ColorRGBA(20, 70, 180);
        public Ball()
        {
            // object definition
            ObjectName = "obj_ball";
            Type = typeof(Ball);
            DefaultImageIndex = 0;
            DefaultImageSpeed = 0;
            DefaultSprite = null;
            DefaultOffset = new(0, 0);

            // physics
            DefaultPhysicsAttributes.BodyType = BodyType.Dynamic;
            DefaultPhysicsAttributes.PhysicsBodySize = BALL_SIZE;
            DefaultPhysicsAttributes.PhysicsBodyType = PhysicsBodyType.Box;
        }

        public override void OnCreate(Instance caller, SceneInstance scene)
        {
            RestartDemo(caller, scene);
            caller["isDemo"] = false;
            caller["bounce"] = 0;
            caller["vcheck"] = false;
            base.OnCreate(caller, scene);
        }

        public void RestartDemo(Instance caller, SceneInstance scene)
        {
            if (caller.InstanceVariables.ContainsKey("isDemo"))
                if (!caller.Get<bool>("isDemo"))
                    return;
            Random rng = new(DateTime.Now.Millisecond);
            caller.Depth = -1000;
            caller["angle"] = rng.NextDouble() * 360;
            caller["speed"] = rng.Next(DEMO_MIN_SPEED, DEMO_MAX_SPEED);
            caller.Position = new(scene.ViewSize.W / 2, scene.ViewSize.H / 2);
            AngleSanitize(caller);
        }

        public static void AngleSanitize(Instance caller)
        {
            double angle = caller.Get<double>("angle");
            angle = angle % 360;
            if (angle < 0)
                angle += 360;
            caller["angle"] = angle;
        }

        public static void VerticalFlip(Instance ball)
        {
            if (ball.BaseReference.ObjectName != "obj_ball")
                return;
            double angle = ball.Get<double>("angle");
            ball["angle"] = -angle;
            AngleSanitize(ball);
        }

        public static void HorizontalFlip(Instance ball, double modifier = 0)
        {
            if (ball.BaseReference.ObjectName != "obj_ball")
                return;
            double angle = ball.Get<double>("angle");
            ball["angle"] = -(angle-180) + modifier;
            AngleSanitize(ball);
        }

        public override void OnDestroy(Instance caller, SceneInstance scene)
        {
            base.OnDestroy(caller, scene);
        }

        public override void OnDraw(Instance caller, SceneInstance scene, GraphicsEngine graphics)
        {
            var color = graphics.GetRendererDrawColor();
            graphics.SetRenderDrawColor(BALL_COLOR);
            int x1 = caller.Position.X - BALL_SIZE.W / 2;
            int y1 = caller.Position.Y - BALL_SIZE.W / 2;
            int x2 = x1 + BALL_SIZE.W;
            int y2 = y1 + BALL_SIZE.H;
            graphics.DrawRectangleFilled(x1, y1, x2, y2);

            //graphics.DrawText(Resources.GetFontResource("font_mainMenu_options"), $"Angle: {Math.Round(caller.Get<double>("angle"),2)}, Speed: {caller.Get<int>("speed")}", 0, scene.ViewSize.H, null, TextHorizontalAlign.Left, TextVerticalAlign.Bottom);
            graphics.SetRenderDrawColor(color);

            base.OnDraw(caller, scene, graphics);
        }
        public override void Step(Instance caller, SceneInstance scene)
        {
            bool vcheck = caller.Get<bool>("vcheck");
            // process movement
            double angle = caller.Get<double>("angle");
            int speed = caller.Get<int>("speed");

            // get x and y components
            double cos = Math.Cos(ToRad(angle)) * speed; // x
            double sin = Math.Sin(ToRad(angle)) * speed; // y
            if (Math.Abs(sin) < 0.01)
                sin = 0.0;
            // determine sign
            int sC = cos > 0 ? 1 : cos < 0 ? -1 : 0;
            int sS = sin > 0 ? 1 : sin < 0 ? -1 : 0;

            int xVel = (int)Math.Max(1, Math.Abs(cos)) * sC;
            int yVel = (int)-Math.Max(1, Math.Abs(sin)) * sS;

            if (speed != 0)
            {
                caller.Position.X += xVel;
                caller.Position.Y += yVel;
            }

            int yPosition = caller.Position.Y;
            int xPosition = caller.Position.X;
            if (yPosition < 0 || yPosition > scene.ViewSize.H)
            {
                if (!vcheck)
                {
                    caller["vcheck"] = true;
                    VerticalFlip(caller);
                    PlayBounce(caller);
                }
            } else
            {
                caller["vcheck"] = false;
            }
            if (xPosition < 0 || xPosition > scene.ViewSize.W)
            {
                if (caller.Get<bool>("isDemo"))
                {
                    //HorizontalFlip(caller);
                    RestartDemo(caller, scene);
                    Instance demoController = scene.GetInstances("con_demo")[0];
                    double ballAngle = caller.Get<double>("angle");
                    caller["contact"] = DemoController.PredictBallContact(caller, ballAngle > 90 && ballAngle < 270 ? demoController.Get<Instance>("left") : demoController.Get<Instance>("right"), scene);
                    caller["check"] = false;
                    Audio.PlayEffect("snd_hurt", 0, 0);
                }
                else
                {
                    // serve
                    switch (scene.BaseReference.Name)
                    {
                        case "scn_1player":
                            var inst = scene.GetInstances("con_ai");
                            if (inst.Length > 0)
                            {
                                var controller = inst[0];
                                controller["serving"] = xPosition < scene.ViewSize.W / 2 ? 1 : 2;
                            }
                            break;
                        case "scn_2player":

                            break;
                        default:
                            Debug.Log("Unknown game type.");
                            break;
                    }
                    Audio.PlayEffect("snd_hurt", 0, 0);
                } 
            }
            base.Step(caller, scene);
        }

        public static void PlayBounce(Instance caller)
        {
            int bounce = caller.Get<int>("bounce");
            string snd = "";
            if (bounce == 0)
            {
                snd = "snd_bounce1";
                caller["bounce"] = 1;
            }
            else
            {
                snd = "snd_bounce2";
                caller["bounce"] = 0;
            }
            Audio.PlayEffect(snd, 0, 1);
        }
    }
}
