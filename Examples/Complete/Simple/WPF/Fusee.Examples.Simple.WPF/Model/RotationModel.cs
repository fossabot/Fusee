using System.ComponentModel;

namespace Fusee.Examples.Simple.Wpf.Model
{
    public class RotationModel : INotifyPropertyChanged
    {
        private float _x, _y, _z;

        public float RotX
        {
            get => _x;
            set
            {
                _x = value;
                OnPropertyChanged(nameof(RotX));
            }
        }
        public float RotY
        {
            get => _y;
            set
            {
                _y = value;
                OnPropertyChanged(nameof(RotY));
            }
        }
        public float RotZ
        {
            get => _z;
            set
            {
                _z = value;
                OnPropertyChanged(nameof(RotZ));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
