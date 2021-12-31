using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// GEngine Stuff
using GEngine;
using GEngine.Engine;
using GEngine.Game;
using GEngine.Net;
using PingPong.GameObjects;
using static GEngine.GEngine;

namespace PingPong.Scenes
{
    public class MainMenu : Scene
    {
        public MainMenu(Size sceneSize, Size viewSize) : base(sceneSize, viewSize)
        {
            // initialize scene
            Name = "scn_mainMenu";
            ReferenceType = typeof(MainMenu);

            // set physics properties
            UsesPhysics = false;
            WorldGravity = new Coord(0, 0);

            // set scene properties
            Properties.AnimateSprites = false;

            // add game objects
            MainMenuController mainMenu = new();
            DemoController demo = new();
            GameObjects.Add(mainMenu, 0, 0);
            GameObjects.Add(demo, 0, 0);
        }

        public override void OnCreate(SceneInstance caller)
        {
            base.OnCreate(caller);
        }

        public override void OnDestroy(SceneInstance caller)
        {
            base.OnDestroy(caller);
        }

        public override void Step(SceneInstance caller)
        {
            base.Step(caller);
        }
    }
}
