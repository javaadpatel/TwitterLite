namespace TwitterLite.Contracts.Models
{
    public class UserCheckpointMetadata
    {
        public string LastReadHash { get; set; } = null;
        public double LastReadLine { get; set; } = 0;
    }
}
