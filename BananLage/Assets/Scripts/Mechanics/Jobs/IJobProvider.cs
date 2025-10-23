namespace Mechanics.Jobs
{
    public interface IJobProvider<out T> where T : JobContext
    {
        public TaskType JobType { get; }
        public T CurrentJob { get; }
    }
}