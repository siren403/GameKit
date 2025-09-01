using GameKit.Navigation.Screens.Core;
using GameKit.Navigation.Screens.Core.Commands;
using GameKit.Navigation.Screens.Dialog;
using GameKit.Navigation.Screens.Dialog.Commands;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VitalRouter;

namespace Samples.Navigation.SceneOverview.Dialogs
{
    [Routes]
    public partial class ConfirmDialog : MonoBehaviour, IDialog, IScreenProps<string>
    {
        public bool IsVisible
        {
            get => GetComponent<Canvas>().enabled;
            set
            {
                GetComponent<Canvas>().enabled = value;
                GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }
        }

        [SerializeField] private Button scrimButton = null!;
        [SerializeField] private Button confirmButton = null!;
        [SerializeField] private Button cancelButton = null!;

        [SerializeField] private TextMeshProUGUI messageText = null!;

        public string Message
        {
            set => messageText.text = value;
        }

        string IScreenProps<string>.Props
        {
            set => Message = value;
        }


        public Observable<Unit> OnClickScrim => scrimButton.OnClickAsObservable();
        public Observable<Unit> OnClickConfirm => confirmButton.OnClickAsObservable();
        public Observable<Unit> OnClickCancel => cancelButton.OnClickAsObservable();

        [Route]
        private void On(ShowCommand command)
        {
            IsVisible = true;
        }

        [Route]
        private void On(HideCommand command)
        {
            IsVisible = false;
        }

        [Route]
        private void On(DialogErrorCommand command)
        {
            IsVisible = false;
            Debug.LogError($"Error on dialog {name}: {command.ErrorCode} - {command.Message}");
        }
    }
}