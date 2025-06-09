namespace Metron2Client;

public interface ISensor
{
    int Channel { get; }
    bool IsDigital { get; }
    string Name { get; }
    double Value { get; }
}