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
 *	Object: AIController
 */

namespace PingPong.GameObjects
{
    internal class AIController : GameObject
    {
        private static readonly int STARTING_X = 42;
        private static readonly int BALL_SERVE_DIST = 14;
        private static readonly int PADDLE_PLAYER_SPEED = 6;
        private static readonly int PADDLE_PLAYER_SPRINT = 16;
        private static readonly int PADDLE_AI_MOVECONST = 40;
        private static readonly double PADDLE_AI_MOVEBASE = 2.0;
        private static Instance GetPlayer(Instance caller) => caller.Get<Instance>("player");
        private static Instance GetAI(Instance caller) => caller.Get<Instance>("ai");
        private static Instance GetBall(Instance caller) => caller.Get<Instance>("ball");

        public AIController()
        {
            // object definition
            ObjectName = "con_ai";
            Type = typeof(AIController);
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
            base.OnCreate(caller, scene);
            Paddle paddleRef = new Paddle();
            Ball ballRef = new Ball();
            Instance player = paddleRef.CreateInstance(out _);
            Instance ai = paddleRef.CreateInstance(out _);
            Instance ball = ballRef.CreateInstance(out _);

            player.Position.X = STARTING_X;
            player.Position.Y = scene.ViewSize.H / 2;
            ai.Position.X = scene.ViewSize.W - STARTING_X;
            ai.Position.Y = scene.ViewSize.H / 2;

            scene.AddInstance(player);
            scene.AddInstance(ai);
            scene.AddInstance(ball);
            ball["isDemo"] = false;

            caller["serving"] = 1; // 0 = no serve, 1 = player, 2 = AI
            caller["player"] = player;
            caller["ai"] = ai;
            caller["ball"] = ball;
        }

        public override void OnDestroy(Instance caller, SceneInstance scene)
        {
            base.OnDestroy(caller, scene);
        }

        public override void OnDraw(Instance caller, SceneInstance scene, GraphicsEngine graphics)
        {
            //Instance ball = caller.Get<Instance>("ball");
            //var temp = graphics.GetRendererDrawColor();
            //graphics.SetRenderDrawColor(new(255, 30, 30, 200));
            //graphics.DrawLine(0, ball.Get<int>("contact"), scene.ViewSize.W, ball.Get<int>("contact"));
            //graphics.SetRenderDrawColor(temp);
            base.OnDraw(caller, scene, graphics);
        }

        public override void Step(Instance caller, SceneInstance scene)
        {
            if (Input.IsPressed(SDL_Keycode.SDLK_ESCAPE))
            {
                GEngine.GEngine.Scenes.SwitchToScene("scn_mainMenu");
            }
            // get instance var
            int serving = caller.Get<int>("serving");
            Instance player = GetPlayer(caller);
            Instance ai = GetAI(caller);
            Instance ball = GetBall(caller);

            int playerX = player.Position.X;
            int playerY = player.Position.Y;

            int aiX = ai.Position.X;
            int aiY = ai.Position.Y;

            int ballX = ball.Position.X;
            int ballY = ball.Position.Y;

            // get player input
            bool playerUp = Input.IsDown(SDL_Keycode.SDLK_w) || Input.IsDown(SDL_Keycode.SDLK_a);
            bool playerDown = Input.IsDown(SDL_Keycode.SDLK_s) || Input.IsDown(SDL_Keycode.SDLK_d);
            bool playerServe = Input.IsDown(SDL_Keycode.SDLK_SPACE);
            bool playerSprint = Input.IsDown(SDL_Keycode.SDLK_LSHIFT);

            // player paddle movement
            if (playerUp && !playerDown)
            {
                playerY -= playerSprint ? PADDLE_PLAYER_SPRINT : PADDLE_PLAYER_SPEED;
            }
            if (playerDown && !playerUp)
            {
                playerY += playerSprint ? PADDLE_PLAYER_SPRINT : PADDLE_PLAYER_SPEED;
            }

            // clamp movement to visible area
            if (playerY - Paddle.PADDLE_SIZE.H / 2 < 0)
                playerY = Paddle.PADDLE_SIZE.H / 2;
            if (playerY + Paddle.PADDLE_SIZE.H / 2 > scene.ViewSize.H)
                playerY = scene.ViewSize.H - Paddle.PADDLE_SIZE.H/2;

            // set default ai target (idle)
            int aiTarget = scene.ViewSize.H / 2; // not implemented idle behavior

            // "check once" status
            bool check = ball.Get<bool>("check");

            // paddle collision
            if (Collision.CheckCollision(scene, ball, "obj_paddle") != 0)
            {
                Instance paddle = ballX > scene.ViewSize.W / 2 ? ai : player;
                if (!check)
                {
                    int simX = paddle == player ? paddle.Position.X - 30 : paddle.Position.X + 30;
                    //angle = ball.Get<double>("angle");
                    double angle = Math.Atan2(paddle.Position.Y - ball.Position.Y, ball.Position.X - simX) * 180.0 / Math.PI;
                    Ball.AngleSanitize(ball);
                    ball["angle"] = angle;
                    //Ball.HorizontalFlip(ball);
                    ball["contact"] = DemoController.PredictBallContact(ball, paddle == ai ? player : ai, scene);
                    ball["check"] = true;
                    Ball.PlayBounce(ball);
                }
            }
            else
            {
                ball["check"] = false;
            } 

            // get inputs
            switch (serving)
            {
                case 1: // player serve
                    ballX = playerX + BALL_SERVE_DIST;
                    ballY = playerY;
                    if (playerServe)
                    {
                        caller["serving"] = 0; // no serve
                        // set ball vel
                        ball["speed"] = 12;
                        ball["angle"] = 0.0;
                        ball["contact"] = DemoController.PredictBallContact(ball, ai, scene);
                    }
                    break;
                case 2: // ai serve
                    ballX = aiX - BALL_SERVE_DIST - 1;
                    ballY = aiY;
                    break;
            }

            /* AI BEHAVIOR */

            // ai move toward target if angle is toward ai paddle
            double ballAngle = ball.Get<double>("angle");
            if (serving == 0 &&
                !(ballAngle > 90 && ballAngle < 270)) // angle not towards left side
            {
                // get target position from last computed contact point
                aiTarget = ball.Get<int>("contact");

                // determine distance to target posistion
                int distance = aiTarget - aiY;

                // calculate movement value based on how far the distance is
                double moveMod = Math.Max(1, Math.Abs(distance) / PADDLE_AI_MOVECONST);
                int moveValue = (int)(PADDLE_AI_MOVEBASE * moveMod);

                // apply movement
                if (distance > 0)
                {
                    if (aiY + Paddle.PADDLE_SIZE.H / 2 < scene.ViewSize.H)
                        aiY += moveValue;
                } else if (distance < 0)
                {
                    if (aiY - Paddle.PADDLE_SIZE.H / 2 > 0)
                        aiY -= moveValue;
                }
            }
            // if ai is serving
            else if (serving == 2)
            {

            }

            // update instances
            player.Position.X = playerX;
            player.Position.Y = playerY;
            ai.Position.X = aiX;
            ai.Position.Y = aiY;
            ball.Position.X = ballX;
            ball.Position.Y = ballY;

            base.Step(caller, scene);
        }
    }
}
