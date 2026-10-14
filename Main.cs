
using MelonLoader;

namespace JustZoom
{
    public class Main : MelonMod
    {
        public static Main Instance { get; private set; }

        public JustZoom Zoom { get; private set; }

        public override void OnInitializeMelon()
        {
            Instance = this;
            Zoom = new JustZoom();
            Settings.Instance.AddToModSettings("Just Zoom");
        }

        public override void OnLateUpdate()
        {
            Zoom.Update();
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            Zoom.Reset();
        }
    }
}