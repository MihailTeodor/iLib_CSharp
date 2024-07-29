using Xunit;
using FluentAssertions;
using iLib.ModelTest;

public class BaseEntityTest
{
    private readonly FakeBaseEntity _entity1;
    private readonly FakeBaseEntity _entity2;

    public BaseEntityTest()
    {
        string uuid1 = Guid.NewGuid().ToString();
        string uuid2 = Guid.NewGuid().ToString();
        _entity1 = new FakeBaseEntity(uuid1);
        _entity2 = new FakeBaseEntity(uuid2);
    }

    [Fact]
    public void TestNullUUID()
    {
        Action act = () => new FakeBaseEntity(null!);
        act.Should().Throw<ArgumentException>().WithMessage("uuid cannot be null!");
    }

    [Fact]
    public void TestEquals()
    {
        _entity1.Should().BeEquivalentTo(_entity1);
        _entity1.Should().NotBeEquivalentTo(_entity2);
    }
}
