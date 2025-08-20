namespace GameKit.Navigation.Scenes
{
    public readonly struct LoadingStatus
    {
        public readonly int TotalCount;
        public readonly int LoadedCount;
        public readonly float CurrentProgress;

        public LoadingStatus(int totalCount)
        {
            TotalCount = totalCount;
            LoadedCount = 0;
            CurrentProgress = 0f;
        }

        public LoadingStatus(int totalCount, int loadedCount, float currentProgress)
        {
            TotalCount = totalCount;
            LoadedCount = loadedCount;
            CurrentProgress = currentProgress;
        }

        public LoadingStatus WithLoadedCount(int loadedCount)
        {
            return new LoadingStatus(
                TotalCount,
                loadedCount,
                0
            );
        }

        public LoadingStatus WithCurrentProgress(float currentProgress)
        {
            return new LoadingStatus(
                TotalCount,
                LoadedCount,
                currentProgress
            );
        }

        public override string ToString()
        {
            return
                $"LoadingStatus: TotalCount={TotalCount}, LoadedCount={LoadedCount}, CurrentProgress={CurrentProgress}";
        }
    }
}