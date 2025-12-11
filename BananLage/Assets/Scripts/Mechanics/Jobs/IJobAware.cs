namespace Mechanics.Jobs
{
    public interface IJobAware: IJobContainer
    {
        public TaskType JobType { get; }
    }

    public interface IJobContainer
    {
        public JobContext JobContext { get; }
    }
}