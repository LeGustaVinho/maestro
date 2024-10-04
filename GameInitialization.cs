using System;

namespace LegendaryTools.Maestro
{
    public class GameInitialization : SingletonBehaviour<GameInitialization>
    {
        public bool Verbose;
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
        [Sirenix.OdinInspector.ReadOnly]
#endif
        public bool IsInitialized { private set; get; }
        public bool AutoDisposeOnDestroy;
        public InitStepListingConfig InitStepListing;
        public event Action OnBeforeInitialize;
        public event Action OnInitialize;

        public Maestro Maestro { get; private set; }
        
        protected override async void Start()
        {
            base.Start();
            Maestro = new Maestro(Verbose);
            
            BeforeInitialize();
            OnBeforeInitialize?.Invoke();
            
            foreach (InitStepConfig initStepConfig in InitStepListing.Configs)
            {
                Maestro.Add(initStepConfig);
            }
            
            await Maestro.Start();
            IsInitialized = true;
            
            AfterInitialize();
            OnInitialize?.Invoke();
        }

        protected virtual void OnDestroy()
        {
            if (AutoDisposeOnDestroy)
            {
                InitStepListing.Dispose();
            }
        }

        protected virtual void BeforeInitialize()
        {
            
        }
        
        protected virtual void AfterInitialize()
        {
            
        }
    }
}