using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace GameKit.Navigation.Tests.Screens.Sessions
{
    public static class TaskExtensions
    {
        public static void IsCompleted<T>(this UniTask<T> task)
        {
            Assert.True(task.Status == UniTaskStatus.Succeeded);
        }

        public static void IsCompleted<T>(this Task<T> task)
        {
            Assert.True(task.IsCompletedSuccessfully);
        }
    }
}