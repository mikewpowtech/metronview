using System.Collections.Generic;

namespace Substituter
{
    public class ChunkList
    {
        public IList<Chunk> Chunks { get; }

        public ChunkList()
        {
            Chunks = new List<Chunk>();
        }
    }
}
