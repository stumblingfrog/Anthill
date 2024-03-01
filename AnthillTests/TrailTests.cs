using Anthill.Hub;

namespace AnthillTests;

[TestClass]
public class TrailTests
{
    private int intCollector = 0;
    private int baseCollector = 0;
    private int derivedCollector = 0;

    [TestMethod]
    public void TrailShouldRegisterSimpleHandlers()
    {
        var trail = new Trail();
        trail.Subscribe<int>(HandleInt);

        trail.Publish(12);
        trail.Publish(18);

        Assert.AreEqual(30, this.intCollector);
    }

    [TestMethod]
    public void TrailShouldRegisterBaseHandlers()
    {
        var trail = new Trail();
        trail.Subscribe<Base>(HandleBase, true);

        trail.Publish(new Base());
        trail.Publish(new Derived());

        Assert.AreEqual(2, this.baseCollector);
    }

    [TestMethod]
    public void TrailShouldRegisterDerivedHandlers()
    {
        var trail = new Trail();
        trail.Subscribe<Base>(HandleBase, true);
        trail.Subscribe<Derived>(HandleDerived);

        trail.Publish(new Base());
        trail.Publish(new Derived());

        Assert.AreEqual(1, this.baseCollector);
        Assert.AreEqual(1, this.derivedCollector);
    }

    private void HandleInt(int value)
    {
        this.intCollector += value;
    }

    private void HandleBase(Base value)
    {
        this.baseCollector++;
    }

    private void HandleDerived(Derived value)
    {
        this.derivedCollector++;
    }

    private class Base
    {
    }

    private class Derived: Base
    {
    }
}
