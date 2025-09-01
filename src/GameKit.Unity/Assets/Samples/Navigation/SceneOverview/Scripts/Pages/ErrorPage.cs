using GameKit.Navigation.Components.Pages;
using GameKit.Navigation.Screens.Core;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Samples.Navigation.SceneOverview.Pages
{
    public class ErrorPage : CanvasPage, IScreenProps<string>
    {
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Button backButton;
        [SerializeField] private Button copyButton;

        public Observable<Unit> OnClickBack => backButton.OnClickAsObservable();

        public Observable<string> OnClickCopy => copyButton.OnClickAsObservable()
            .Select(this, (_, page) => page.messageText.text);

        public string Message
        {
            set => messageText.text = value;
        }

        string IScreenProps<string>.Props
        {
            set => Message = value;
        }
    }
}