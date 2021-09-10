/**
 * Copyright (c) 2019-2021 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

using UnityEngine;

namespace Simulator.Sensors
{
    using System.Linq;
    using Simulator.Bridge.Data;
    using Simulator.Sensors.Postprocessing;
    using Simulator.Utilities;
    using UnityEngine.Rendering.HighDefinition;
    using UnityEngine.Serialization;

    [SensorType("Color Camera", new[] { typeof(ImageData) })]
    [DefaultPostprocessing(typeof(Rain), typeof(SunFlare))]
    public class ColorCameraSensor : CameraSensorBase
    {
        // fix for 1000+ ms lag spike that appears approx. 1 s after starting
        // simulation from quickscript will now happen after sensor
        // initialization instead of mid-simulation
        // NOTE this fix is dependent on HDRP version

        private int renderedFrames;
        private int requiredFrames;

        [FormerlySerializedAs("SensorDistributionType")]
        [SensorParameter]
        public SensorDistributionType sensorDistributionType = SensorDistributionType.ClientOnly;
        [FormerlySerializedAs("DisplayIndex")]
        [SensorParameter]
        public int DisplayIndex = -1;
        public override float PerformanceLoad { get; } = 1.0f;
        public override SensorDistributionType DistributionType => sensorDistributionType;

        protected override void Initialize()
        {
            if (DisplayIndex == -1)
            {
                base.Initialize();
                SetupSkyWarmup();
            }
            else
            {
                ActivateOtherDisplay();
            }
        }

        protected override void Update()
        {
            if (DisplayIndex == -1)
            {
                base.Update();
                CheckSkyWarmup();
            }
        }

        private void SetupSkyWarmup()
        {
            renderedFrames = 0;
            requiredFrames = 0;

            var activeProfile = SimulatorManager.Instance.EnvironmentEffectsManager.ActiveProfile;
            var pbrSky = activeProfile.components.FirstOrDefault(x => x is PhysicallyBasedSky) as PhysicallyBasedSky;
            if (pbrSky == null)
                return;

            requiredFrames = pbrSky.numberOfBounces.value;
        }

        private void CheckSkyWarmup()
        {
            if (renderedFrames > requiredFrames)
                return;

            renderedFrames++;
            RenderCamera();
        }

        private void ActivateOtherDisplay()
        {
            if (Display.displays.Length > DisplayIndex)
            {
                SensorCamera.targetDisplay = DisplayIndex;
                if (renderTarget != null)
                {
                    renderTarget.Release();
                    renderTarget = null;
                }
                SensorCamera.targetTexture = null;
                SensorCamera.enabled = true;

                var display = Display.displays[DisplayIndex];
                display.Activate(display.systemWidth, display.systemHeight, 60);

                OnVisualizeToggle(true);
            }
        }
    }
}
