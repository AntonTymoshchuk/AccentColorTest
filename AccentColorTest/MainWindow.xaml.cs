using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AccentColorTest
{
    public partial class MainWindow : Window
    {
        [DllImport("uxtheme.dll", EntryPoint = "#95")]
        public static extern uint GetImmersiveColorFromColorSetEx(uint dwImmersiveColorSet, uint dwImmersiveColorType, bool bIgnoreHighContrast, uint dwHighContrastCacheMode);
        [DllImport("uxtheme.dll", EntryPoint = "#96")]
        public static extern uint GetImmersiveColorTypeFromName(IntPtr pName);
        [DllImport("uxtheme.dll", EntryPoint = "#98")]
        public static extern int GetImmersiveUserColorSetPreference(bool bForceCheckRegistry, bool bSkipCheckOnFail);

        private Color GetAccentColor()
        {
            uint colorSetEx = GetImmersiveColorFromColorSetEx(
                (uint)GetImmersiveUserColorSetPreference(false, false),
                GetImmersiveColorTypeFromName(Marshal.StringToHGlobalUni("ImmersiveStartSelectionBackground")),
                false, 0);

            Color color = Color.FromArgb((byte)((0xFF000000 & colorSetEx) >> 24), (byte)(0x000000FF & colorSetEx),
                (byte)((0x0000FF00 & colorSetEx) >> 8), (byte)((0x00FF0000 & colorSetEx) >> 16));

            return color;
        }

        private Color GetUsedAccentColor(Color accentColor)
        {
            int[] rgbArray = new int[3];

            rgbArray[0] = Convert.ToInt32(accentColor.R);
            rgbArray[1] = Convert.ToInt32(accentColor.G);
            rgbArray[2] = Convert.ToInt32(accentColor.B);

            int maxValue = rgbArray.Max();
            int maxValueIndex = 0;
            for (int i = 0; i < 3; i++)
            {
                if (rgbArray[maxValueIndex] == maxValue)
                    break;
                maxValueIndex++;
            }

            int minValue = rgbArray.Min();
            int minValueIndex = 0;
            for (int i = 0; i < 3; i++)
            {
                if (rgbArray[minValueIndex] == minValue)
                    break;
                minValueIndex++;
            }

            int dependentValueIndex = 0;
            for (int i = 0; i < 3; i++)
            {
                if (rgbArray[dependentValueIndex] != rgbArray[maxValueIndex] && rgbArray[dependentValueIndex] != rgbArray[minValueIndex])
                    break;
                dependentValueIndex++;
            }

            bool usedAccentColorIsBrighter = false;
            if (rgbArray[maxValueIndex] > 204)
                usedAccentColorIsBrighter = false;
            else if (rgbArray[maxValueIndex] < 204)
                usedAccentColorIsBrighter = true;

            if (usedAccentColorIsBrighter == false)
            {
                int maxValuesDifference = rgbArray[maxValueIndex] - 204;
                int minValuesDifference = Math.Abs(rgbArray[minValueIndex] - 43);

                rgbArray[dependentValueIndex] -= (maxValuesDifference + minValuesDifference) / 2;

                rgbArray[maxValueIndex] = 204;
                rgbArray[minValueIndex] = 43;
            }
            else if (usedAccentColorIsBrighter == true)
            {
                int maxValuesDifference = 204 - rgbArray[maxValueIndex];
                int minValuesDifference = Math.Abs(43 - rgbArray[minValueIndex]);

                if (rgbArray[dependentValueIndex] < 150)
                    rgbArray[dependentValueIndex] -= (maxValuesDifference + minValuesDifference) / 2;
                else if (rgbArray[dependentValueIndex] > 150 && rgbArray[dependentValueIndex] < 166)
                    rgbArray[dependentValueIndex] += (maxValuesDifference + minValuesDifference) / 4;
                else
                    rgbArray[dependentValueIndex] += (maxValuesDifference + minValuesDifference) / 2;

                rgbArray[maxValueIndex] = 204;
                rgbArray[minValueIndex] = 43;
            }

            int red, green, blue;

            red = rgbArray[0];
            green = rgbArray[1];
            blue = rgbArray[2];

            return Color.FromArgb(Convert.ToByte(255), Convert.ToByte(red), Convert.ToByte(green), Convert.ToByte(blue));
        }

        public MainWindow()
        {
            InitializeComponent();
            Color usedAccentColor = GetUsedAccentColor(GetAccentColor());
            Color transparentAccentColor = Color.FromArgb(77, usedAccentColor.R, usedAccentColor.G, usedAccentColor.B);
            transparentAccentColorTextBlock.Background = new SolidColorBrush(transparentAccentColor);
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            //this.Close();
        }
    }
}
