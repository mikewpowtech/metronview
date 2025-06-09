namespace Utilities;

public record NumericReading : Reading
{
    public double Value { get; init; }
    public override void Save(IReadingsDb db) => db.AddReading(this);
}