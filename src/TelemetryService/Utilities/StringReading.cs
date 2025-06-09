namespace Utilities;

public partial record StringReading : Reading
{
    public string Value { init; get; }
    public override void Save(IReadingsDb db) => db.AddReading(this);
}