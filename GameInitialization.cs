using System;

namespace LegendaryTools.Systems.Maestro
{
    public class GameInitialization : SingletonBehaviour<GameInitialization>
    {
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        public bool IsInitialized { private set; get; }
        public InitStepListingConfig InitStepListing;
        public event Action OnBeforeInitialize;
        public event Action OnInitialize;
        
        protected override async void Start()
        {
            base.Start();
            BeforeInitialize();
            OnBeforeInitialize?.Invoke();
            
            Maestro maestro = new Maestro();
            foreach (InitStepConfig initStepConfig in InitStepListing.Configs)
            {
                maestro.AddWithDependency(initStepConfig);
            }
            
            await maestro.Start();
            IsInitialized = true;
            
            AfterInitialize();
            OnInitialize?.Invoke();
        }

        protected virtual void BeforeInitialize()
        {
            
        }
        
        protected virtual void AfterInitialize()
        {
            
        }
    }
}