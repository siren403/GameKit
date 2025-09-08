using System;
using UnityEngine;

namespace Samples.Navigation.PageQuickStart.Pages
{
    [ExecuteAlways]
    public class CanvasPageDebugger : MonoBehaviour
    {
        [SerializeField] private RectTransform? rectTransform;
        private readonly Vector3[] _corners = new Vector3[4];

        private void Update()
        {
            if (!ReferenceEquals(rectTransform, null))
            {
                rectTransform.GetWorldCorners(_corners);
                for (var i = 0; i < 4; i++)
                {
                    Debug.DrawLine(_corners[i], _corners[(i + 1) % 4], Color.green);
                }
            }
        }

        private void Reset()
        {
            rectTransform = GetComponent<RectTransform>();
        }
    }
}