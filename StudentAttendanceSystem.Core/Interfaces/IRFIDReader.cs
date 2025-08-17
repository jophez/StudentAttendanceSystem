namespace StudentAttendanceSystem.Core.Interfaces
{
    public interface IRFIDReader
    {
        event EventHandler<RFIDReadEventArgs> CardRead;
        event EventHandler<RFIDErrorEventArgs> ReadError;
        event EventHandler<RFIDStatusEventArgs> StatusChanged;
        
        bool IsConnected { get; }
        string ReaderName { get; }
        
        Task<bool> InitializeAsync();
        Task<bool> StartReadingAsync();
        Task<bool> StopReadingAsync();
        void Dispose();
    }

    public class RFIDReadEventArgs : EventArgs
    {
        public string CardId { get; set; } = string.Empty;
        public DateTime ReadTime { get; set; }
        public byte[] RawData { get; set; } = Array.Empty<byte>();
    }

    public class RFIDErrorEventArgs : EventArgs
    {
        public string ErrorMessage { get; set; } = string.Empty;
        public Exception? Exception { get; set; }
    }

    public class RFIDStatusEventArgs : EventArgs
    {
        public RFIDReaderStatus Status { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public enum RFIDReaderStatus
    {
        Disconnected,
        Connecting,
        Connected,
        Reading,
        Error
    }
}