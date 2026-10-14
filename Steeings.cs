
using ModSettings;
using UnityEngine;

namespace JustZoom
{
    internal class Settings : JsonModSettings
    {
        internal static Settings Instance { get; } = new Settings();

        [Name("Zoom key")]
        public KeyCode zoomKey = KeyCode.Z;

        [Name("Zoom wheel step")]
        [Slider(5, 50)]
        public int zoomWheelStep = 15;

        [Name("Zoom speed")]
        [Description("How fast the FOV transitions")]
        [Slider(1, 30)]
        public int zoomSmoothing = 12;

        [Name("Smoot camera")]
        [Slider(1, 40)]
        public int smoothLag = 25;
    }
}