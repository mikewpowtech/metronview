using System;

namespace Powelectrics.Telemetry.FileProcessor
{
    public class TubeCubeDump
    {
        public string IdFromFilename { get; set; }
        public DateTime DateFromFilename { get; set; }
        public DateTime FileCreationDateUtc { get; set; }
        public TubeCubeHeader Header { get; set; }
        public TubeCubeReadings Readings { get; set; }
        public TubeCubeStatus Status { get; set; }
        public string ParseErrors { get; set; }
    }
}
