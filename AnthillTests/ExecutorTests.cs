namespace AnthillTests;

using Anthill.Hub;
using System.Collections.Concurrent;

[TestClass]
public class ExecutorTests
{
    private ConcurrentBag<Exception> exceptions = new();
    private ConcurrentBag<string> workItems = new();

    public TestContext TestContext { get; set; } = default!;

    [TestMethod]
    public void ExecutorShouldRunSimpleList()
    {
        var trail = new Trail()
            .AddQueue(2);
        trail.Subscribe<Exception>(ExceptionHandler, true);
        trail.Queue(Run1);
        trail.Queue(Run2);
        trail.Queue(RunException);

        trail.Stop();

        Assert.AreEqual(1, exceptions.Count);
        Assert.AreEqual(2, workItems.Count);

    }

    [TestMethod]
    public void ExecutorShouldRunComplexList()
    {
        var trail = new Trail()
            .AddQueue(2);
        trail.Subscribe<Exception>(ExceptionHandler, true);

        for (var i=0; i < 5; i++)
        {
            trail.Queue(Run1);
            trail.Queue(Run2);
        }

        trail.Stop();

        Assert.AreEqual(0, exceptions.Count);
        Assert.AreEqual(10, workItems.Count);

    }

    [TestMethod]
    public void ExecutorShouldRunCascadeList()
    {
        var trail = new Trail()
            .AddQueue(2);
        trail.Subscribe<Exception>(ExceptionHandler, true);

        for (var i = 0; i < 5; i++)
        {
            trail.Queue(Run1);
            trail.Queue(RunCascade);
        }

        trail.Stop();

        Assert.AreEqual(0, exceptions.Count);
        Assert.AreEqual(15, workItems.Count, $"{DateTime.Now.TimeOfDay} - {Thread.CurrentThread.ManagedThreadId} - {workItems.Count} out of 15");

    }

    [TestMethod]
    public void ExecutorShouldHandleExceptions()
    {
        var trail = new Trail()
            .AddQueue(2);
        trail.Subscribe<Exception>(ExceptionHandler, true);

        for (var i = 0; i < 5; i++)
        {
            trail.Queue(Run1);
            trail.Queue(RunException);
        }

        trail.Stop();

        Assert.AreEqual(5, exceptions.Count);
        Assert.AreEqual(5, workItems.Count);

    }

    private void Run1()
    {
        DoWork(nameof(Run1));
    }

    private void Run2()
    {
        DoWork(nameof(Run2));
    }

    private void RunException()
    {
        throw new Exception("All fine");
    }

    private void RunCascade(Trail trail)
    {
        DoWork(nameof(RunCascade));
        trail.Queue(Run1);
    }

    private void DoWork(string value)
    {
        var message = $"{DateTime.Now.TimeOfDay} - {Thread.CurrentThread.ManagedThreadId} - {value}";
        TestContext.WriteLine(message);
        this.workItems.Add(message);
    }

    private void ExceptionHandler(Exception exception) 
    {
        this.exceptions.Add(exception);
    }
}
