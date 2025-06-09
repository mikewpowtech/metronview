using Metron2Client;
using System;

namespace TelemetryLoadTestClient
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            DoTest();
        }

        private static void DoTest()
        {
             Metron2 rtu = new("MWTEST10000000000000", "localhost", 50200) { Secret = "secret", Pin = "1234", AutoConfig = true };
            //Metron2 rtu = new("RJCFAKEFAKEFAKEFAKE1", "localhost", 50200) { Secret = "secret", Pin = "1234", AutoConfig = true };
            //Metron2 rtu = new("AP1000000M2110097925", "localhost", 50200) { Secret = "secret", Pin = "1234", AutoConfig = true };
            rtu.AddSensor(new RandomAnalogueSensor(1, "T1", new Random()));
            //for (int i = 0; i < 100; i++)
            //{
                rtu.OneIteration();
            //}
        }
    }
}
