using System;

namespace kadmium_dmx_core.Color
{
    /// <summary>
    /// Structure to define RGB.
    /// </summary>
    public class RGB
    {
        #region Fields
        private int red;
        private int green;
        private int blue;

        #endregion

        #region Operators
        public static bool operator ==(RGB item1, RGB item2)
        {
            return (
                item1.Red == item2.Red
                && item1.Green == item2.Green
                && item1.Blue == item2.Blue
                );
        }

        public static bool operator !=(RGB item1, RGB item2)
        {
            return (
                item1.Red != item2.Red
                || item1.Green != item2.Green
                || item1.Blue != item2.Blue
                );
        }

        #endregion

        #region Accessors
        public int Red
        {
            get
            {
                return red;
            }
            set
            {
                red = (value > 255) ? 255 : ((value < 0) ? 0 : value);
            }
        }

        public int Green
        {
            get
            {
                return green;
            }
            set
            {
                green = (value > 255) ? 255 : ((value < 0) ? 0 : value);
            }
        }

        public int Blue
        {
            get
            {
                return blue;
            }
            set
            {
                blue = (value > 255) ? 255 : ((value < 0) ? 0 : value);
            }
        }

        public RGB Mix(RGB other, float otherAmount)
        {
            float thisAmount = 1 - otherAmount;
            int red = (int)(otherAmount * other.Red + thisAmount * this.Red);
            int green = (int)(otherAmount * other.Green + thisAmount * this.Green);
            int blue = (int)(otherAmount * other.Blue + thisAmount * this.Blue);
            return new Color.RGB(red, green, blue);
        }
        #endregion

        public RGB(int R, int G, int B)
        {
            red = (R > 255) ? 255 : ((R < 0) ? 0 : R);
            green = (G > 255) ? 255 : ((G < 0) ? 0 : G);
            blue = (B > 255) ? 255 : ((B < 0) ? 0 : B);
        }

        public RGB() : this(0, 0, 0) { }

        public override string ToString()
        {
            return string.Format("#{0:X2}{1:X2}{2:X2}", red, green, blue);
        }

        public static RGB Parse(string rgbString)
        {
            string strippedString = rgbString.Replace("#", "");
            int red = int.Parse(strippedString.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            int green = int.Parse(strippedString.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            int blue = int.Parse(strippedString.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            return new RGB(red, green, blue);
        }

        #region Methods
        public override bool Equals(Object obj)
        {
            if (obj == null || GetType() != obj.GetType()) return false;

            return (this == (RGB)obj);
        }

        public override int GetHashCode()
        {
            return Red.GetHashCode() ^ Green.GetHashCode() ^ Blue.GetHashCode();
        }

        #endregion
    }
}
