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
    [Loader(LoaderType.Automatic)]
    public class AIGame : Scene
    {
        public AIGame(Size sceneSize, Size viewSize) : base(sceneSize, viewSize)
        {
            // initialize scene
            Name = "scn_1player";
            ReferenceType = typeof(AIGame);

            // set physics properties
            UsesPhysics = false;
            WorldGravity = new Coord(0, 0);

            // set scene properties
            Properties.AnimateSprites = false;

        }

        public override void OnCreate(SceneInstance caller)
        {
            base.OnCreate(caller);
            AIController ai = new();
            var instance = ai.CreateInstance(out _);
            caller.AddInstance(instance);
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
