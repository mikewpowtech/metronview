namespace Metron2Configuration
{
    /// <summary>
    /// Visitor pattern implementation for Metron2Configuration
    /// </summary>
    public interface IVisitableConfiguration
    {
        void Accept(IConfigurationVisitor visitor);
    }
}
