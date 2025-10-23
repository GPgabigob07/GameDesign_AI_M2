namespace Mechanics.Jobs
{
    public interface IJobAware
    {
        public JobContext JobContext { get; }
        public TaskType JobType { get; }
    }
}