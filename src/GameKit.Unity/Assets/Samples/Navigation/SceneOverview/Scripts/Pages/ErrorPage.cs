using Cysharp.Threading.Tasks;
using GameKit.Navigation.Components.Pages;
using GameKit.Navigation.Screens.Page.Commands;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Samples.Navigation.SceneOverview.Pages
{
    public class ErrorPage : CanvasPage
    {
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Button backButton;

        public Observable<Unit> OnClickBack => backButton.OnClickAsObservable();

        public string Message
        {
            set => messageText.text = value;
        }
    }
}