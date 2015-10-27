using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
using System.Windows;
// using System.Windows.Controls;
// using System.Windows.Data;
// using System.Windows.Documents;
using System.Windows.Input;
// using System.Windows.Media;
// using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SOC
{
    public partial class VyberWindow : Window
    {
        public MainWindow mw = Application.Current.MainWindow as MainWindow;
        public Rectangle temp = new Rectangle();

        public int r = 0;
        public int s = 0;
        public int v = 0;
        public string str = null;
        public string[] a;

        // Hlavny konstruktor
        public VyberWindow(Rectangle rect, double x, double y)
        {
            InitializeComponent();
            this.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
            this.Left = x - 20;
            this.Top = y + 50;      

            temp = rect;
            str = temp.Name.ToString().Remove(0, 6);
            a = str.Split('_');
            v = Int32.Parse(a[0].ToString());
            s = Int32.Parse(a[1].ToString());
            r = Int32.Parse(a[2].ToString());

            mw.matOrigin[v, r][s] = "0";
            temp.Fill = System.Windows.Media.Brushes.Transparent;                                          
        }
        
        // Vyber kontaktu
        private void radioKontakt_Checked(object sender, RoutedEventArgs e)
        {            
            radioMerker.IsChecked = false;
            radioMerkerNeg.IsChecked = false;
            radioCitac.IsChecked = false;
            radioCitacNeg.IsChecked = false;
            radioCasovac.IsChecked = false;
            radioCasovacNeg.IsChecked = false;
            radioSeriovySpoj.IsChecked = false;

            radioCislo1.IsEnabled = true;
            radioCislo2.IsEnabled = true;
            radioCislo3.IsEnabled = true;
            radioCislo4.IsEnabled = true;

            if (radioKontakt.IsChecked == true)
            {                                
                if (radioCislo1.IsChecked == true)                
                {
                    temp.Fill = mw.kontakt1;
                    mw.matOrigin[v, r][s] = "PINB & (1 << PB0)";
                }
                else if (radioCislo2.IsChecked == true)
                {
                    temp.Fill = mw.kontakt2;
                    mw.matOrigin[v, r][s] = "PINB & (1 << PB1)";
                }
                else if (radioCislo3.IsChecked == true)
                {
                    temp.Fill = mw.kontakt3;
                    mw.matOrigin[v, r][s] = "PINB & (1 << PB2)";
                }
                else if (radioCislo4.IsChecked == true)
                {
                    temp.Fill = mw.kontakt4;
                    mw.matOrigin[v, r][s] = "_COUNTER1INT";
                }
            }
            else if (radioKontaktNeg.IsChecked == true)
            {
                if (radioCislo1.IsChecked == true)
                {
                    temp.Fill = mw.kontaktneg1;
                    mw.matOrigin[v, r][s] = "!PINB & (1 << PB0)";
                }
                else if (radioCislo2.IsChecked == true)
                {
                    temp.Fill = mw.kontaktneg2;
                    mw.matOrigin[v, r][s] = "!PINB & (1 << PB1)";
                }
                else if (radioCislo3.IsChecked == true)
                {
                    temp.Fill = mw.kontaktneg3;
                    mw.matOrigin[v, r][s] = "!PINB & (1 << PB2)";
                }
                else if (radioCislo4.IsChecked == true)
                {
                    temp.Fill = mw.kontaktneg4;
                    mw.matOrigin[v, r][s] = "!_COUNTER1INT";
                }
            }         
        }                

        private void radioMarker_Checked(object sender, RoutedEventArgs e)
        {
            radioKontakt.IsChecked = false;
            radioKontaktNeg.IsChecked = false;            
            radioCitac.IsChecked = false;
            radioCitacNeg.IsChecked = false;
            radioCasovac.IsChecked = false;
            radioCasovacNeg.IsChecked = false;
            radioSeriovySpoj.IsChecked = false;

            radioCislo1.IsEnabled = true;
            radioCislo2.IsEnabled = true;
            radioCislo3.IsEnabled = true;
            radioCislo4.IsEnabled = false;
            if (radioMerker.IsChecked == true)
            {
                if (radioCislo1.IsChecked == true)
                {
                    temp.Fill = mw.merker1;
                    mw.matOrigin[v, r][s] = "_M1";
                }
                else if (radioCislo2.IsChecked == true)
                {
                    temp.Fill = mw.merker2;
                    mw.matOrigin[v, r][s] = "_M2";
                }
                else if (radioCislo3.IsChecked == true)
                {
                    temp.Fill = mw.merker3;
                    mw.matOrigin[v, r][s] = "_M3";
                }
            }
            else if (radioMerkerNeg.IsChecked == true)
            {
                radioCislo4.IsEnabled = false;
                if (radioCislo1.IsChecked == true)
                {
                    temp.Fill = mw.merkerneg1;
                    mw.matOrigin[v, r][s] = "!_M1";
                }
                else if (radioCislo2.IsChecked == true)
                {
                    temp.Fill = mw.merkerneg2;
                    mw.matOrigin[v, r][s] = "!_M2";
                }
                else if (radioCislo3.IsChecked == true)
                {
                    temp.Fill = mw.merkerneg3;
                    mw.matOrigin[v, r][s] = "!_M3";
                }
            }
        }

        private void radioSeriovy_Checked(object sender, RoutedEventArgs e)
        {
            radioKontakt.IsChecked = false;
            radioKontaktNeg.IsChecked = false;
            radioMerker.IsChecked = false;
            radioMerkerNeg.IsChecked = false;
            radioCitac.IsChecked = false;
            radioCitacNeg.IsChecked = false;
            radioCasovac.IsChecked = false;
            radioCasovacNeg.IsChecked = false;
            
            if (radioSeriovySpoj.IsChecked == true)
            {
                radioCislo1.IsEnabled = false;
                radioCislo2.IsEnabled = false;
                radioCislo3.IsEnabled = false;
                radioCislo4.IsEnabled = false;
                temp.Fill = mw.seriovySpoj;
                mw.matOrigin[v, r][s] = "1";
            }
        }

        private void radioCitac_Checked(object sender, RoutedEventArgs e)
        {
            radioKontakt.IsChecked = false;
            radioKontaktNeg.IsChecked = false;
            radioMerker.IsChecked = false;
            radioMerkerNeg.IsChecked = false;            
            radioCasovac.IsChecked = false;
            radioCasovacNeg.IsChecked = false;
            radioSeriovySpoj.IsChecked = false;

            radioCislo1.IsEnabled = true;
            radioCislo2.IsEnabled = false;
            radioCislo3.IsEnabled = false;
            radioCislo4.IsEnabled = false;
            if (radioCitac.IsChecked == true)
            {                
                if (radioCislo1.IsChecked == true)
                {
                    temp.Fill = mw.citac1;
                    mw.matOrigin[v, r][s] = "_COUNTER1";
                }
            }
           else if (radioCitacNeg.IsChecked == true)
            {
                if (radioCislo1.IsChecked == true)
                {
                    temp.Fill = mw.citacneg1;
                    mw.matOrigin[v, r][s] = "!_COUNTER1";
                }
            }
        }

        private void radioCasovac_Checked(object sender, RoutedEventArgs e)
        {
            radioKontakt.IsChecked = false;
            radioKontaktNeg.IsChecked = false;
            radioMerker.IsChecked = false;
            radioMerkerNeg.IsChecked = false;
            radioCitac.IsChecked = false;
            radioCitacNeg.IsChecked = false;           
            radioSeriovySpoj.IsChecked = false;

            radioCislo1.IsEnabled = true;
            radioCislo2.IsEnabled = false;
            radioCislo3.IsEnabled = false;
            radioCislo4.IsEnabled = false;
            if (radioCasovac.IsChecked == true)
            {
                if (radioCislo1.IsChecked == true)
                {
                    temp.Fill = mw.casovac1;
                    mw.matOrigin[v, r][s] = "_TIMER1";
                }
            }
            else if (radioCasovacNeg.IsChecked == true)
            {
                if (radioCislo1.IsChecked == true)
                {
                    temp.Fill = mw.casovacNeg1;
                    mw.matOrigin[v, r][s] = "!_TIMER1";
                }
            }   
        }

        // Zatvorenie okna
        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            mw.isVyberOkno = false;
            this.Close();
        }
    }
}
