using System;

namespace ADHDTraining.Core.BciTransport
{
    [Serializable]
    public class BciTransportConfig
    {
        public string transportType = "null";
        public string host = "127.0.0.1";
        public int port = 9876;
        public string serialPort = "COM3";
        public int baudRate = 115200;
        public string replayFile = "";
        public string focusField = "focus";
        public string blinkField = "blink";
        public string headField = "head";
    }
}
