using System.IO;
using System.Text;

namespace Utils
{
    public class Comms
    {
        #region Properties and Fields

        private MemoryStream ReadStream { get; set; }
        private MemoryStream WriteStream { get; set; }
        private BinaryReader Reader { get; set; }
        private BinaryWriter Writer { get; set; }

        #endregion

        public Comms()
        {
            //Create the readers and writers.
            ReadStream = new MemoryStream();
            WriteStream = new MemoryStream();
            Reader = new BinaryReader(ReadStream);
            Writer = new BinaryWriter(WriteStream);
        }

        #region Data Sending Functions

        public void SendData(string str)
        {
            byte[] results = Encoding.UTF8.GetBytes(str);
            Writer.Write(results);

            int bytesWritten = (int)WriteStream.Position;
            byte[] result = new byte[bytesWritten];

            WriteStream.Position = 0;
            WriteStream.Read(result, 0, bytesWritten);
            WriteStream.Position = 0;

            Client.GetStream().BeginWrite(result, 0, result.Length, null, null);
            Writer.Flush();
        }

        #endregion
    }
}
