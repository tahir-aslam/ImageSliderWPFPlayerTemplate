using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ImageSliderWPF
{
    public class WeatherData
    {
        private string _Temperature;
        public string Temperature
        {
            get { return this._Temperature; }
            set { this._Temperature = value; }
        }

        private DrawingImage _IconData;
        public DrawingImage IconData
        {
            get { return this._IconData; }
            set { this._IconData = value; }
        }

        private Style _TemperatureStyle;
        public Style TemperatureStyle
        {
            get { return this._TemperatureStyle; }
            set { this._TemperatureStyle = value; }
        }

        private double _Width;
        public double Width
        {
            get { return this._Width; }
            set { this._Width = value; }
        }

        private double _Height;
        public double Height
        {
            get { return this._Height; }
            set { this._Height = value; }
        }

        private string _Day;
        public string Day
        {
            get { return this._Day; }
            set { this._Day = value; }
        }

        private double _ItemWidth;
        public double ItemWidth
        {
            get { return this._ItemWidth; }
            set { this._ItemWidth = value; }
        }
    }
}
