namespace GameKit.Navigation.Tests.Screens.Sessions
{
    public record SampleReviewForm
    {
        public string Name { get; init; } = string.Empty;
        public float Progress { get; init; }
        public int Count { get; init; }
    }
}