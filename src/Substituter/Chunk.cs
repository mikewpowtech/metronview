namespace Substituter
{
    public class Chunk
    {
        public bool IsSubstitution { get; }
        public string Text { get; }

        public Chunk(bool isSubstitution, string text)
        {
            IsSubstitution = isSubstitution;
            Text = text;
        }
    }
}
