using GEngine;
using GEngine.Engine;
using PingPong.Scenes;

namespace PingPong
{
    public class Program
    {
        private static GameEngine _engine;

        public static void Main(string[] args)
        {
            // create game engine
            _engine = new GameEngine(backend: VideoBackend.OpenGL_ES2);

            // engine properties
            _engine.Properties.Title = "Ping Pong";
            _engine.Properties.EnableFramelimiter = true;
            _engine.Properties.TargetFPS = 360;
            _engine.Properties.TargetTPS = 128;
            _engine.Properties.WindowResolution = new(1024, 768);
            _engine.Properties.InternalResolution = new(1024, 768);
            _engine.Properties.RenderScaleQuality = RenderScaleQuality.Linear;
            _engine.Properties.HideConsoleWindow = false;
            _engine.Properties.EnableDebug = true;

            // handle window close
            _engine.AllowClose = true;
            _engine.HandleClose = false;
            _engine.OnWindowClose += (eventArgs) =>
            {
                Environment.Exit(0);
            };

            // start engine
            _engine.Start();

            // set render color to black
            _engine.GraphicsEngine.SetRenderDrawColor(new(0, 0, 0));

            // load font resource
            Debug.Log("Loading fonts...");
            _engine.ResourceManager.LoadAsFont("OpenSans-Regular.ttf", "font_mainMenu", 48);
            _engine.ResourceManager.LoadAsFont("OpenSans-Regular.ttf", "font_mainMenu_options", 32);

            // load sounds
            Debug.Log("Loading sounds...");
            _engine.ResourceManager.LoadAsAudio("blip.wav", "snd_blip", AudioType.Effect);
            _engine.ResourceManager.LoadAsAudio("hurt.wav", "snd_hurt", AudioType.Effect);
            _engine.ResourceManager.LoadAsAudio("Bounce1.wav", "snd_bounce1", AudioType.Effect);
            _engine.ResourceManager.LoadAsAudio("Bounce2.wav", "snd_bounce2", AudioType.Effect);

            _engine.AudioEngine.SetEffectVolume("snd_bounce1", 0.1);
            _engine.AudioEngine.SetEffectVolume("snd_bounce2", 0.1);

            // load scenes
            Debug.Log("Loading scenes...");
            Loader.LoadAllScenes();

            _engine.SceneManager.SwitchToScene("scn_mainMenu");

            // allow run
            Debug.Log("Resources loaded!");
            _engine.ResourcesLoaded = true;

            // keep alive
            while (_engine.Running)
            {
                //Console.WriteLine("Frametime: {0}, Logictime: {1}, Total: {2}", _engine.CurrentFrametime, _engine.CurrentLogictime, _engine.CurrentFrametime + _engine.CurrentLogictime);
                Thread.Sleep(500);
            }
        }
    }
}