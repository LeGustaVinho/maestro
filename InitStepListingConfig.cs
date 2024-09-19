﻿using System;
using UnityEngine;

namespace LegendaryTools.Systems.Maestro
{
    [CreateAssetMenu(menuName = "Tools/Maestro/InitStepListingConfig")]
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