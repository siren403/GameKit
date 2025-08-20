using GameKit.Navigation.Screens.Page;
using UnityEngine;

namespace Samples.Navigation.PageQuickStart
{
    public class PageRoot : MonoBehaviour, IParentProvider
    {
        public Transform Parent => transform;
    }
}