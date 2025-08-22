using GameKit.Navigation.Scenes.Extensions;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using VitalRouter;

namespace Samples.Navigation.SceneOverview.Scenes
{
    public class TitleScene : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<TitleEntryPoint>();
        }
    }

    public class TitleEntryPoint : ITickable
    {
        private readonly Router _router;

        public TitleEntryPoint(Router router)
        {
            _router = router;
        }

        public void Tick()
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                _router.ToScene("/intro");
            }
        }
    }
}