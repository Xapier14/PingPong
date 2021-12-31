using GEngine;
using GEngine.Engine;
using GEngine.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GEngine.GEngine;
using static SDL2.SDL;

/*	GEngine-R
 *	GameObject Template
 *	
 *	Object: DemoController
 */

namespace PingPong.GameObjects
{
    internal class DemoController : GameObject
    {
        public static readonly Coord STARTING_POSITION = new(42, 60);
        public static readonly double DEMO_PADDLE_BASESPEED = 2.0;
        public DemoController()
        {
            // object definition
            ObjectName = "con_demo"; // CHANGE ME
            Type = typeof(DemoController);
            DefaultImageIndex = 0;
            DefaultImageSpeed = 0;
            DefaultSprite = null;
            DefaultOffset = new(0, 0); // CHANGE ME

            // physics
            DefaultPhysicsAttributes.BodyType = BodyType.Dynamic; // CHANGE ME
            DefaultPhysicsAttributes.PhysicsBodySize = new(32, 32); // CHANGE ME
            DefaultPhysicsAttributes.PhysicsBodyType = PhysicsBodyType.Box; // CHANGE ME
        }

        public override void OnCreate(Instance caller, SceneInstance scene)
        {
            // create references
            Paddle paddleReference = new();
            Ball ballReference = new();

            // create instances
            Instance leftPaddle = paddleReference.CreateInstance(out _);
            Instance rightPaddle = paddleReference.CreateInstance(out _);
            Instance ball = ballReference.CreateInstance(out _);
            scene.AddInstance(leftPaddle);
            scene.AddInstance(rightPaddle);
            scene.AddInstance(ball);

            // store instances
            caller["left"] = leftPaddle;
            caller["right"] = rightPaddle;
            caller["ball"] = ball;

            // set defaults
            leftPaddle.Position = new(STARTING_POSITION);
            rightPaddle.Position = new(scene.ViewSize.W-STARTING_POSITION.X,
                                      scene.ViewSize.H - STARTING_POSITION.Y);
            ball.Position = new(scene.ViewSize.W/2, scene.ViewSize.H/2);
            ball["isDemo"] = true;

            double ballAngle = ball.Get<double>("angle");

            ball["contact"] = PredictBallContact(ball, ballAngle > 90 && ballAngle < 270 ? leftPaddle : rightPaddle, scene);
            ball["check"] = false;

            base.OnCreate(caller, scene);
        }

        public override void OnDestroy(Instance caller, SceneInstance scene)
        {
            base.OnDestroy(caller, scene);
        }

        public override void OnDraw(Instance caller, SceneInstance scene, GraphicsEngine graphics)
        {
            base.OnDraw(caller, scene, graphics);
        }

        public override void Step(Instance caller, SceneInstance scene)
        {
            Instance ball = caller.Get<Instance>("ball");
            Instance leftPaddle = caller.Get<Instance>("left");
            Instance rightPaddle = caller.Get<Instance>("right");
            int contact = ball.Get<int>("contact");
            double ballAngle = ball.Get<double>("angle");
            if (ballAngle < 0)
                ballAngle += 360;
            bool approachingLeft = ballAngle > 90 && ballAngle < 270;
            bool approachingRight = !approachingLeft;
            bool check = ball.Get<bool>("check");

            Instance paddle = approachingLeft ? leftPaddle : rightPaddle;
            Instance otherPaddle = paddle == leftPaddle ? rightPaddle : leftPaddle;

            // check paddle collision
            if (Collision.CheckCollision(scene, ball, "obj_paddle") != 0)
            {
                if (!check)
                {
                    int simX = paddle == leftPaddle ? paddle.Position.X - 30 : paddle.Position.X + 30;
                    //angle = ball.Get<double>("angle");
                    double angle = Math.Atan2(paddle.Position.Y - ball.Position.Y, ball.Position.X - simX) * 180.0 / Math.PI;
                    Ball.AngleSanitize(ball);
                    ball["angle"] = angle;
                    //Ball.HorizontalFlip(ball);
                    ball["contact"] = PredictBallContact(ball, paddle == leftPaddle ? rightPaddle : leftPaddle, scene);
                    ball["check"] = true;
                    Ball.PlayBounce(ball);
                }
            } else
            {
                ball["check"] = false;
                // move toward ball
                contact = ball.Get<int>("contact");
                int displacement = contact - paddle.Position.Y;
                double moveMod = Math.Max(1, Math.Abs(displacement) /30);
                int moveVal = (int)(DEMO_PADDLE_BASESPEED * moveMod);
                if (displacement > 0)
                {
                    if (paddle.Position.Y < (scene.ViewSize.H-Paddle.PADDLE_SIZE.H/2)-8)
                        paddle.Position.Y += moveVal;
                } else if (displacement < 0)
                {
                    if (paddle.Position.Y > (Paddle.PADDLE_SIZE.H/2))
                        paddle.Position.Y -= moveVal;
                }
            }

            if (otherPaddle.Position.Y < scene.ViewSize.H/2 &&
                otherPaddle.Position.Y < (scene.ViewSize.H - Paddle.PADDLE_SIZE.H / 2)-8)
                    otherPaddle.Position.Y += 2;
            if (otherPaddle.Position.Y > scene.ViewSize.H / 2 &&
                otherPaddle.Position.Y > (Paddle.PADDLE_SIZE.H / 2))
                    otherPaddle.Position.Y -= 2;

            base.Step(caller, scene);
        }

        public static int PredictBallContact(Instance ball, Instance paddle, SceneInstance scene)
        {
            // PredictBallContact()
            // should return the y-coordinate of where the ball should make contact

            double ballAngle = ball.Get<double>("angle");
            int ballSpeed = ball.Get<int>("speed");

            int xPaddle = paddle.Position.X;

            int pX = ball.Position.X;
            int pY = ball.Position.Y;

            bool checkType = xPaddle < pX; // true = <, false = >

            // we need to try and simulate ball movement
            while (true)
            {
                // perform movement

                double cos = Math.Cos(Tools.ToRad(ballAngle)) * ballSpeed;
                double sin = Math.Sin(Tools.ToRad(ballAngle)) * ballSpeed;
                int sC = cos > 0 ? 1 : cos < 0 ? -1 : 0;
                int sS = sin > 0 ? 1 : sin < 0 ? -1 : 0;

                int xVel = (int)Math.Max(1, Math.Abs(cos)) * sC;
                int yVel = (int)-Math.Max(1, Math.Abs(sin)) * sS;

                // apply velocity
                pX += xVel;
                pY += yVel;

                // check for flips
                if (pY < 0 || pY > scene.ViewSize.H)
                {
                    // do flip
                    ballAngle = -ballAngle;
                }

                // check for pX
                if (checkType ? pX <= xPaddle : pX >= xPaddle)
                    break;
            }
            Random rand = new Random(DateTime.Now.Millisecond);
            return pY + rand.Next(-30, 30);
        }

        private static void MoveInstanceClamped(Instance inst, int x, int y, SceneInstance scene)
        {

        }
    }
}
