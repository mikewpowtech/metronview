using System;
using System.Collections.Generic;

namespace Powelectrics.Telemetry.FileProcessor
{
    public class TubeCubeHeader
    {
        public int Attempt { get; }
        public string Site { get; }
        public string SerialNo { get; }
        public DateTime HeaderDateTime { get; }
        public List<HeaderSensor> HeaderSensors { get; set; }

        public TubeCubeHeader(int attempt, string site, string serialNo, DateTime headerDateTime)
        {
            Attempt = attempt;
            Site = site;
            SerialNo = serialNo;
            HeaderDateTime = headerDateTime;
        }
    }
}
