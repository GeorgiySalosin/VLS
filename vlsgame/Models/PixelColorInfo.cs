using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace VLSGame.Models
{

    public class PixelColorInfo : INotifyPropertyChanged
    {
        private Color _centerColor;
        private string _colorHex;
        private double _latitude;
        private double _longitude;

        public Color CenterColor
        {
            get => _centerColor;
            set
            {
                _centerColor = value;
                OnPropertyChanged();
                ColorHex = $"#{value.R:X2}{value.G:X2}{value.B:X2}";
            }
        }

        public string ColorHex
        {
            get => _colorHex;
            private set
            {
                _colorHex = value;
                OnPropertyChanged();
            }
        }

        public double Latitude
        {
            get => _latitude;
            set
            {
                _latitude = value;
                OnPropertyChanged();
            }
        }

        public double Longitude
        {
            get => _longitude;
            set
            {
                _longitude = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
