﻿using System.ComponentModel;

namespace Fusee.Examples.Integrations.Wpf.Model
{
    public class PositionModel : INotifyPropertyChanged
    {
        private float _x, _y, _z;

        public float X
        {
            get
            {
                return _x;
            }
            set
            {
                _x = value;
                OnPropertyChanged("X");
            }
        }
        public float Y
        {
            get
            {
                return _y;
            }
            set
            {
                _y = value;
                OnPropertyChanged("Y");
            }
        }
        public float Z
        {
            get
            {
                return _z;
            }
            set
            {
                _z = value;
                OnPropertyChanged("Z");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}