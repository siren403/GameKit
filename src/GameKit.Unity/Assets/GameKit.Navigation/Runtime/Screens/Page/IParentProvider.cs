using UnityEngine;

namespace GameKit.Navigation.Screens.Page
{
    public interface IParentProvider
    {
        Transform Parent { get; }
    }
}