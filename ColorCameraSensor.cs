/**
 * Copyright (c) 2019-2021 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

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
        public override float PerformanceLoad { get; } = 1.0f;
        public override SensorDistributionType DistributionType => sensorDistributionType;

        protected override void Initialize()
        {
            base.Initialize();
            SetupSkyWarmup();
        }

        protected override void Update()
        {
            base.Update();
            CheckSkyWarmup();
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
    }
}
