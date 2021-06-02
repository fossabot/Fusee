using Fusee.Math.Core;
using System;
using System.ComponentModel;

namespace Fusee.Examples.Simple.Wpf.Model
{
    public class ColorModel : INotifyPropertyChanged
    {
        public float4 Color
        {
            get => _color;
            set
            {
                _color = value;
                OnPropertyChanged(nameof(Color));
            }
        }
        private float4 _color;

        private readonly Random _rnd = new Random();

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void ChangeColor()
        {
            Color = new float4(_rnd.Next(256) / 255f, _rnd.Next(256) / 255f, _rnd.Next(256) / 255f, 1.0f);
        }
    }
}