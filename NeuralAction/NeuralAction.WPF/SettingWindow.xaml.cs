﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NeuralAction.WPF
{
    /// <summary>
    /// SettingWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SettingWindow : Window
    {
        int scale;
        public SettingWindow()
        {
            InitializeComponent();
            DataContext = Settings.Current;
            Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Top = SystemParameters.WorkArea.Height - ActualHeight + 1;
            Left = SystemParameters.WorkArea.Width - ActualWidth;
        }

        private void Bt_Apply_Click(object sender, RoutedEventArgs e)
        {
            int ind = int.MinValue;
            try
            {
                ind = Convert.ToInt32(Tb_Camera.Text);
            }
            catch
            {

            }
            if (ind != int.MinValue)
                Settings.Current.CameraIndex = ind;

            Settings.Save();
        }

        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == System.Windows.Input.Key.Escape)
            {
                Close();
            }
        }
    }
}
