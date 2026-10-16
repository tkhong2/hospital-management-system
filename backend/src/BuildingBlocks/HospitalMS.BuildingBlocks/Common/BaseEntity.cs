namespace HospitalMS.BuildingBlocks.Common;

public abstract class BaseEntity
{
    public Guid Id { get; set; }
}

public abstract class BaseEntity<TKey>
{
    public TKey Id { get; set; } = default!;
}
