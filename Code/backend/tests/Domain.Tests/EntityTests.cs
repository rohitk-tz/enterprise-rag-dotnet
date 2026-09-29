using SharedKernel;
using FluentAssertions;
using Xunit;

namespace Domain.Tests;

public class EntityTests
{
    private sealed class TestEntity : Entity<Guid>
    {
        public TestEntity(Guid id) : base(id) { }
    }

    [Fact]
    public void Entities_with_the_same_id_are_equal()
    {
        var id = Guid.NewGuid();
        var a = new TestEntity(id);
        var b = new TestEntity(id);

        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void Entities_with_different_ids_are_not_equal()
    {
        var a = new TestEntity(Guid.NewGuid());
        var b = new TestEntity(Guid.NewGuid());

        a.Equals(b).Should().BeFalse();
    }
}
