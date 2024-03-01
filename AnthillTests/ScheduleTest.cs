namespace AnthillTests;

using Anthill.Hub;

[TestClass]
public class ScheduleTests 
{
    private readonly List<Exception> exceptions = [];
    private readonly List<string> workItems = [];

    [TestMethod]
    public void ExecutorShouldRunShedule()
    {
        var trail = new Trail()
            .AddQueue()
            .AddScheduler();
        trail.Subscribe<Exception>(ExceptionHandler, true);

        trail.Schedule("command", Run1, TimeSpan.FromMilliseconds(100));

        Thread.Sleep(1000);

        trail.Stop();

        Assert.IsTrue(workItems.Count() >= 9 );
        Assert.AreEqual(0, exceptions.Count());
    }

    [TestMethod]
    public void ExecutorShouldRunComplexSchedule()
    {
        var trail = new Trail()
            .AddQueue()
            .AddScheduler();

        trail.Subscribe<Exception>(ExceptionHandler, true);

        trail.Schedule("fast", Run1, TimeSpan.FromMilliseconds(100));
        trail.Schedule("slow", Run2, TimeSpan.FromMilliseconds(200));

        Thread.Sleep(1000);

        trail.Stop();

        Assert.IsTrue(workItems.Count() >= 15);
        Assert.AreEqual(0, exceptions.Count());
    }

    private void Run1()
    {
        DoWork("Run1");
    }

    private void Run2()
    {
        DoWork("Run1");
    }

    private void DoWork(string value)
    {
        lock (this)
        {
            this.workItems.Add(value);
        }
    }

    private void ExceptionHandler(Exception exception) 
    {
        lock (this)
        {
            this.exceptions.Add(exception);
        }
    }
}
