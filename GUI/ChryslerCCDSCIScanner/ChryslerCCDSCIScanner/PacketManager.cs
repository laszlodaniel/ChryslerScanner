using System.ComponentModel;

namespace ChryslerCCDSCIScanner
{
    public class PacketManager : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public PacketManager()
        {
            
        }

        public bool DataReceived
        {
            set { SendPropertyChanged("DataReceived"); }
        }

        private void SendPropertyChanged(string property)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }
}