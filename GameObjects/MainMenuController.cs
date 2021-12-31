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
    public class MainMenuController : GameObject
    {
        public const SDL_Keycode SELECT = SDL_Keycode.SDLK_SPACE;
        public const SDL_Keycode UP = SDL_Keycode.SDLK_UP;
        public const SDL_Keycode DOWN = SDL_Keycode.SDLK_DOWN;

        public static readonly ColorRGBA TITLE_COLOR = ColorRGBA.WHITE;
        public static readonly ColorRGBA MENU_INACTIVE = new(85, 81, 90);
        public static readonly ColorRGBA MENU_ACTIVE = new(220, 240, 255);

        public const int MENU_SPACING = 48;
        public MainMenuController()
        {
            // object definition
            ObjectName = "con_mainMenu";
            Type = typeof(MainMenuController);
            DefaultImageIndex = 0;
            DefaultImageSpeed = 0;
            DefaultSprite = null;
            DefaultOffset = new(0, 0);

            // physics
            DefaultPhysicsAttributes.PhysicsBodySize = new(1, 1);
            DefaultPhysicsAttributes.PhysicsBodyType = PhysicsBodyType.Box;
        }

        public override void OnCreate(Instance caller, SceneInstance scene)
        {
            caller["currentOption"] = 0;
            caller["optionCount"] = 3;
            caller["options"] = new string[]
            {
                "1-Player",
                "2-Player",
                "Exit"
            };
            base.OnCreate(caller, scene);
        }

        public override void OnDestroy(Instance caller, SceneInstance scene)
        {
            base.OnDestroy(caller, scene);
        }

        public override void OnDraw(Instance caller, SceneInstance scene, GraphicsEngine graphics)
        {
            ColorRGBA color = graphics.GetRendererDrawColor();

            FontResource font = Resources.GetFontResource("font_mainMenu");
            FontResource fontOptions = Resources.GetFontResource("font_mainMenu_options");

            graphics.SetRenderDrawColor(TITLE_COLOR);
            graphics.DrawText(font, "Ping Pong", scene.ViewSize.W/2, 60, null, TextHorizontalAlign.Center);
            int currentOption = caller.Get<int>("currentOption");
            int optionCount = caller.Get<int>("optionCount");

            // top preview
            if (currentOption > 0)
            {
                graphics.SetRenderDrawColor(MENU_INACTIVE);
                string topText = caller.Get<string[]>("options")[currentOption - 1];
                graphics.DrawText(fontOptions, topText, scene.ViewSize.W / 2, (scene.ViewSize.H / 2) + 20 - MENU_SPACING, null, TextHorizontalAlign.Center, TextVerticalAlign.Middle);
            }
            // main selected
            graphics.SetRenderDrawColor(MENU_INACTIVE);
            string optionText = caller.Get<string[]>("options")[currentOption];
            int textWidth = graphics.MeasureText(fontOptions, optionText).W + 20;
            int x1 = (scene.ViewSize.W / 2) - (textWidth / 2);
            int x2 = x1 + textWidth;
            int y = (scene.ViewSize.H / 2) + 20;
            //graphics.DrawLine(x1, y, x2, y);
            graphics.SetRenderDrawColor(MENU_ACTIVE);
            graphics.DrawText(fontOptions, optionText, scene.ViewSize.W / 2, (scene.ViewSize.H / 2) + 20, null, TextHorizontalAlign.Center, TextVerticalAlign.Middle);

            // bottom preview
            if (currentOption <= optionCount - 2)
            {
                graphics.SetRenderDrawColor(MENU_INACTIVE);
                string bottomText = caller.Get<string[]>("options")[currentOption + 1];
                graphics.DrawText(fontOptions, bottomText, scene.ViewSize.W / 2, (scene.ViewSize.H / 2) + 20 + MENU_SPACING, null, TextHorizontalAlign.Center, TextVerticalAlign.Middle);
            }

            graphics.SetRenderDrawColor(color);
            base.OnDraw(caller, scene, graphics);
        }

        public override void Step(Instance caller, SceneInstance scene)
        {
            base.Step(caller, scene);
            int currentOption = caller.Get<int>("currentOption");
            int optionCount = caller.Get<int>("optionCount");

            if (Input.IsPressed(UP))
            {
                if (currentOption > 0)
                    caller["currentOption"] = currentOption - 1;
            }
            if (Input.IsPressed(DOWN))
            {
                if (currentOption < optionCount - 1)
                    caller["currentOption"] = currentOption + 1;
            }
            if (Input.IsReleased(SELECT))
            {
                switch (caller.Get<int>("currentOption"))
                {
                    case 0: // 1-Player
                        Game.SceneManager.SwitchToScene("scn_1player");
                        break;
                    case 1: // 2-Player
                        Game.SceneManager.SwitchToScene("scn_2player");
                        break;
                    case 2: // Exit
                        Game.ForceStop();
                        break;
                }
            }
        }
    }
}
