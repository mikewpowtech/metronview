using System;

namespace Powelectrics.Telemetry.FileProcessor
{
    class FileInUseException : Exception
    {
        public FileInUseException(Exception ex)
            : base("File is in use", ex)
        {
        }
    }
}
