using System;
using UnityEngine;

namespace LegendaryTools.Systems.MaestroV2
{
    [CreateAssetMenu(menuName = "Tools/Maestro/InitStepListingConfigV2")]
    public class InitStepListingConfig : ConfigListing<InitStepConfig>, IDisposable
    {
        public void Dispose()
        {
            foreach (InitStepConfig initStepConfig in Configs)
            {
                initStepConfig.Dispose();
            }
        }
    }
}