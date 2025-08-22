using GameKit.Navigation.Components.Pages;
using R3;
using UnityEngine;
using Button = UnityEngine.UI.Button;

namespace Samples.Navigation.SceneOverview.Pages
{
    public class InitPage : CanvasPage
    {
        [SerializeField] private Button initButton;
        
        public Observable<Unit> OnClickInit => initButton.OnClickAsObservable();
    }
}