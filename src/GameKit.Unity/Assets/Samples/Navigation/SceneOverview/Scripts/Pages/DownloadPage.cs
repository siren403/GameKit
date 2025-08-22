using GameKit.Navigation.Components.Pages;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Samples.Navigation.SceneOverview.Pages
{
    public class DownloadPage : CanvasPage
    {
        [SerializeField] private Button downloadButton;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Image progressImage;
        
        public Observable<Unit> OnClickDownload => downloadButton.OnClickAsObservable();
        public string Message
        {
            set => messageText.text = value;
        }
        
        public float Progress
        {
            set => progressImage.fillAmount = value;
        }
    }
}