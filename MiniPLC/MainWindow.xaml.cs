using System;
using System.Collections.Generic;
// using System.Linq;
// using System.Text;
using System.Windows;
using System.Windows.Controls;
// using System.Windows.Data; 
// using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
// using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Threading;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO.Ports;
using BlogsPrajeesh.BlogSpot.WPFControls;
using System.IO;

namespace SOC
{
    public partial class MainWindow : Window
    {
        #region Definicie a Inicializacie

        #region Konstanty
        const int PocetRiadkov = 3;
        const int PocetStlpcov = 4;
        const int PocetParStlpcov = PocetStlpcov + 1;
        const int PocetParRiadkov = PocetRiadkov - 1;
        const int PocetVstupov = 3;
        const int PocetVystupov = 13;
        #endregion Konstanty

        #region Bitmapy
        public readonly ImageBrush kontakt1 = new ImageBrush();
        public readonly ImageBrush kontakt2 = new ImageBrush();
        public readonly ImageBrush kontakt3 = new ImageBrush();
        public readonly ImageBrush kontakt4 = new ImageBrush();
        public readonly ImageBrush kontaktneg1 = new ImageBrush();
        public readonly ImageBrush kontaktneg2 = new ImageBrush();
        public readonly ImageBrush kontaktneg3 = new ImageBrush();
        public readonly ImageBrush kontaktneg4 = new ImageBrush();
        public readonly ImageBrush casovac1 = new ImageBrush();
        public readonly ImageBrush casovacNeg1 = new ImageBrush();
        public readonly ImageBrush casovac2 = new ImageBrush();
        public readonly ImageBrush casovacNeg2 = new ImageBrush();
        public readonly ImageBrush merker1 = new ImageBrush();
        public readonly ImageBrush merker2 = new ImageBrush();
        public readonly ImageBrush merker3 = new ImageBrush();
        public readonly ImageBrush merkerneg1 = new ImageBrush();
        public readonly ImageBrush merkerneg2 = new ImageBrush();
        public readonly ImageBrush merkerneg3 = new ImageBrush();
        public readonly ImageBrush citac1 = new ImageBrush();
        public readonly ImageBrush citacneg1 = new ImageBrush();
        public readonly ImageBrush paralelnySpoj = new ImageBrush();
        public readonly ImageBrush seriovySpoj = new ImageBrush();
        #endregion Bitmapy

        #region Stringy pre C - program
        private const string Hlavicky = "#include <avr/io.h>\n#define F_CPU 1000000UL\n#include <util/delay.h>\n#include <avr/interrupt.h>\n";
        private const string Prototypy = "void nastavenieSerialPortu(unsigned int baud);\nvoid serialPosli(unsigned char data);\nunsigned char serialPrijmi(void);\nvoid posliVstupy();\nvoid posliVystupy();\n";
        private const string NastavenieSerialPortu = "void nastavenieSerialPortu(unsigned int baud)\n{\nUBRRH = (unsigned char)(baud>>8);\nUBRRL = (unsigned char)baud;\nUCSRB = (1<<RXEN)|(1<<TXEN);\nUCSRC = (1<<USBS)|(3<<UCSZ0);\n}\n";
        private const string SerialPrijmi = "unsigned char serialPrijmi(void)\n{\nwhile(!(UCSRA && (1<<RXC)));\nreturn UDR;\n}\n";
        private const string SerialPosli = "void serialPosli(unsigned char data)\n{while(!(UCSRA && (1<<UDRE)));\nUDR = data;\n_delay_ms(10);\n}\n";
        private const string Main1 = "int main()\n{\n" + Init + "nastavenieSerialPortu(25);\nwhile(1)\n{\nposliVstupy();\nposliVystupy();\n";
        private const string Init = "DDRD |= (0 << PD3) | (1<<PD2);\nDDRB |= (0<<PB0) | (0<<PB1) | (0<<PB2) | (1<<PB3) | (1<<PB4);\nOCR1A = F_CPU / 64;\nTIMSK = (1 << OCIE1A);\nMCUCR |= (1<<ISC11) | (1<<ISC10);\nPCMSK |= (1<<PIND3);\nGIMSK |= (1<<INT1);\nsei();\n";
        private string Casovac1_1 = "int _OFF1 = 0;";
        private string Citac1_1 = "int _DIR1 = 0;";
        private const string Prerusenia = "int _M1 = 0;int _M2 = 0;int _M3 = 0;\nint _ZAP = 0;\nint _TIMER1 = 0;\nint _COUNTER1 = 0;\nint _COUNTER1INT = 0;\nvolatile unsigned int counter1 = 0;\nvolatile unsigned int timer1 = 0;\nISR(TIMER1_COMPA_vect)\n{\ntimer1++;\n}\n";
        private string Citac1Prerusenie1 = "ISR(INT1_vect)\n{\n_COUNTER1INT = 1;\n";        

        private const string ElseQ1 = "else{PORTB &= ~(1<<PB3);}\n";
        private const string ElseQ2 = "else{PORTB &= ~(1<<PB4);}\n";
        private const string ElseQ3 = "else{PORTD &= ~(1<<PD2);}\n";
        private const string ElseT1 = "else{_TIMER1 = 0;\nTCCR1B = 0;\ntimer1 = 0;if(_OFF1) _ZAP = 0;}\n";                        
        private const string IfCasovac1_1 = "if(timer1 == ";
        private const string IfCasovac2_1 = "){if(_OFF1){_ZAP = 1;_TIMER1 = 0;}else{_TIMER1 = 1;}\nTCCR1B = 0;}\n";

        private const string ElseC1 = "else{_COUNTER1 = 0;}\n";
        private const string IfCitac1_1 = "if(counter1 >= ";
        private const string IfCitac2_1 = "){_COUNTER1 = 1;}";

        private const string PosliVstupy = "void posliVstupy()\n{\nserialPosli('@');\nif(PINB & (1 << PB0)) serialPosli('A');\nelse serialPosli('a');\nif(PINB & (1 << PB1)) serialPosli('B');\nelse serialPosli('b');\nif(PINB & (1 << PB2)) serialPosli('C');\nelse serialPosli('c');\nif(_COUNTER1) serialPosli('K');\nelse serialPosli('k');\nif(_TIMER1) serialPosli('L');\nelse serialPosli('l');\n}\n";
        private const string PosliVystypy = "void posliVystupy()\n{\nif(PORTB & (1 << PB3)) serialPosli('D');\nelse serialPosli('d');\nif(PORTB & (1 << PB4)) serialPosli('E');\nelse serialPosli('e');\nif(PORTD & (1 << PD2)) serialPosli('F');\nelse serialPosli('f');\nif(_M1) serialPosli('G');\nelse serialPosli('g');\n\nif(_M2) serialPosli('H');\nelse serialPosli('h');\nif(_M3) serialPosli('I');\nelse serialPosli('i');\n\nif(TCCR1B != 0) serialPosli('J');\nelse serialPosli('j');\nif(PIND & (1 << PD3)) serialPosli('M');\nelse serialPosli('m');\nif(_DIR1) serialPosli('N');\nelse serialPosli('n');\nserialPosli((char)counter1);\nserialPosli((char)timer1);\n}\n";

        private const string Main2 = "}\nreturn 0;\n}\n";

        private string funkcia = null;
        private string program = null;
        #endregion Stringy pre C - program

        #region Seriovy Port
        private static SerialPort seriovyPort = new SerialPort();
        // private Thread prijmiThread;
        private string portNapalovanie;
        private string portKomunikacia;
        #endregion Seriovy Port

        #region Polia, slovniky - mapovanie mriezky
        public bool[, ,] par = new bool[PocetVystupov, PocetParRiadkov, PocetParStlpcov];
        public Dictionary<string, Rectangle>[,] parRects = new Dictionary<string, Rectangle>[PocetVystupov, PocetParRiadkov]
        {
            { new Dictionary<string, Rectangle>(), new Dictionary<string, Rectangle>() },
            { new Dictionary<string, Rectangle>(), new Dictionary<string, Rectangle>() },
            { new Dictionary<string, Rectangle>(), new Dictionary<string, Rectangle>() },
            { new Dictionary<string, Rectangle>(), new Dictionary<string, Rectangle>() },
            { new Dictionary<string, Rectangle>(), new Dictionary<string, Rectangle>() },
            { new Dictionary<string, Rectangle>(), new Dictionary<string, Rectangle>() },
            { new Dictionary<string, Rectangle>(), new Dictionary<string, Rectangle>() },
            { new Dictionary<string, Rectangle>(), new Dictionary<string, Rectangle>() },
            { new Dictionary<string, Rectangle>(), new Dictionary<string, Rectangle>() },
            { new Dictionary<string, Rectangle>(), new Dictionary<string, Rectangle>() },
            { new Dictionary<string, Rectangle>(), new Dictionary<string, Rectangle>() },
            { new Dictionary<string, Rectangle>(), new Dictionary<string, Rectangle>() },
            { new Dictionary<string, Rectangle>(), new Dictionary<string, Rectangle>() }
        };
        public Dictionary<string, Rectangle>[,] rects = new Dictionary<string, Rectangle>[PocetVystupov, PocetRiadkov]
        {
            { new Dictionary<string, Rectangle>(), new Dictionary<string, Rectangle>(), new Dictionary<string, Rectangle>() },
            { new Dictionary<string, Rectangle>(), new Dictionary<string, Rectangle>(), new Dictionary<string, Rectangle>() },
            { new Dictionary<string, Rectangle>(), new Dictionary<string, Rectangle>(), new Dictionary<string, Rectangle>() },
            { new Dictionary<string, Rectangle>(), new Dictionary<string, Rectangle>(), new Dictionary<string, Rectangle>() },
            { new Dictionary<string, Rectangle>(), new Dictionary<string, Rectangle>(), new Dictionary<string, Rectangle>() },
            { new Dictionary<string, Rectangle>(), new Dictionary<string, Rectangle>(), new Dictionary<string, Rectangle>() },
            { new Dictionary<string, Rectangle>(), new Dictionary<string, Rectangle>(), new Dictionary<string, Rectangle>() },
            { new Dictionary<string, Rectangle>(), new Dictionary<string, Rectangle>(), new Dictionary<string, Rectangle>() },
            { new Dictionary<string, Rectangle>(), new Dictionary<string, Rectangle>(), new Dictionary<string, Rectangle>() },
            { new Dictionary<string, Rectangle>(), new Dictionary<string, Rectangle>(), new Dictionary<string, Rectangle>() },
            { new Dictionary<string, Rectangle>(), new Dictionary<string, Rectangle>(), new Dictionary<string, Rectangle>() },
            { new Dictionary<string, Rectangle>(), new Dictionary<string, Rectangle>(), new Dictionary<string, Rectangle>() },
            { new Dictionary<string, Rectangle>(), new Dictionary<string, Rectangle>(), new Dictionary<string, Rectangle>() }
        };
        public Dictionary<int, string>[,] matOrigin = new Dictionary<int, string>[PocetVystupov, PocetRiadkov]
        {
            { new Dictionary<int, string>(), new Dictionary<int, string>(), new Dictionary<int, string>() },
            { new Dictionary<int, string>(), new Dictionary<int, string>(), new Dictionary<int, string>() },
            { new Dictionary<int, string>(), new Dictionary<int, string>(), new Dictionary<int, string>() },
            { new Dictionary<int, string>(), new Dictionary<int, string>(), new Dictionary<int, string>() },           
            { new Dictionary<int, string>(), new Dictionary<int, string>(), new Dictionary<int, string>() },
            { new Dictionary<int, string>(), new Dictionary<int, string>(), new Dictionary<int, string>() },
            { new Dictionary<int, string>(), new Dictionary<int, string>(), new Dictionary<int, string>() },
            { new Dictionary<int, string>(), new Dictionary<int, string>(), new Dictionary<int, string>() },           
            { new Dictionary<int, string>(), new Dictionary<int, string>(), new Dictionary<int, string>() },
            { new Dictionary<int, string>(), new Dictionary<int, string>(), new Dictionary<int, string>() },
            { new Dictionary<int, string>(), new Dictionary<int, string>(), new Dictionary<int, string>() },           
            { new Dictionary<int, string>(), new Dictionary<int, string>(), new Dictionary<int, string>() },
            { new Dictionary<int, string>(), new Dictionary<int, string>(), new Dictionary<int, string>() }
        };
        public Dictionary<int, string>[,] mat = new Dictionary<int, string>[PocetVystupov, PocetRiadkov]
        {
            { new Dictionary<int, string>(), new Dictionary<int, string>(), new Dictionary<int, string>() },
            { new Dictionary<int, string>(), new Dictionary<int, string>(), new Dictionary<int, string>() },
            { new Dictionary<int, string>(), new Dictionary<int, string>(), new Dictionary<int, string>() },
            { new Dictionary<int, string>(), new Dictionary<int, string>(), new Dictionary<int, string>() },           
            { new Dictionary<int, string>(), new Dictionary<int, string>(), new Dictionary<int, string>() },
            { new Dictionary<int, string>(), new Dictionary<int, string>(), new Dictionary<int, string>() },
            { new Dictionary<int, string>(), new Dictionary<int, string>(), new Dictionary<int, string>() },
            { new Dictionary<int, string>(), new Dictionary<int, string>(), new Dictionary<int, string>() },
            { new Dictionary<int, string>(), new Dictionary<int, string>(), new Dictionary<int, string>() },           
            { new Dictionary<int, string>(), new Dictionary<int, string>(), new Dictionary<int, string>() },
            { new Dictionary<int, string>(), new Dictionary<int, string>(), new Dictionary<int, string>() },           
            { new Dictionary<int, string>(), new Dictionary<int, string>(), new Dictionary<int, string>() },
            { new Dictionary<int, string>(), new Dictionary<int, string>(), new Dictionary<int, string>() }
        };
        #endregion Polia, slovniky - mapovanie mriezky

        #region Procesy
        private readonly Process kompilovanie = new Process();
        private readonly Process napalovanie = new Process();
        #endregion Procesy

        #region Premenne
        public string menoSuboru = "";
        public int cas = 0;
        public int pocet = 0;
        public bool isVyberOkno = false;
        public bool isSaved = false;
        public bool[] vstupy = new bool[PocetVstupov];
        public bool[] vystupy = new bool[PocetVystupov];
        #endregion Premenne

        #endregion definicie a inicializacie

        // Konstruktor hlavneho okna
        public MainWindow()
        {
            // Inicializovanie komponentov
            InitializeComponent();
            // Nacitanie bitmap
            kontakt1.ImageSource = new BitmapImage(new Uri(@"C:\Users\Ivo\Desktop\Projekt\MiniPLC\Bitmaps/kontakt1.png", UriKind.Relative));
            kontakt2.ImageSource = new BitmapImage(new Uri(@"C:\Users\Ivo\Desktop\Projekt\MiniPLC\Bitmaps/kontakt2.png", UriKind.Relative));
            kontakt3.ImageSource = new BitmapImage(new Uri(@"C:\Users\Ivo\Desktop\Projekt\MiniPLC\Bitmaps/kontakt3.png", UriKind.Relative));
            kontakt4.ImageSource = new BitmapImage(new Uri(@"C:\Users\Ivo\Desktop\Projekt\MiniPLC\Bitmaps/kontakt4.png", UriKind.Relative));
            kontaktneg1.ImageSource = new BitmapImage(new Uri(@"C:\Users\Ivo\Desktop\Projekt\MiniPLC\Bitmaps/kontaktneg1.png", UriKind.Relative));
            kontaktneg2.ImageSource = new BitmapImage(new Uri(@"C:\Users\Ivo\Desktop\Projekt\MiniPLC\Bitmaps/kontaktneg2.png", UriKind.Relative));
            kontaktneg3.ImageSource = new BitmapImage(new Uri(@"C:\Users\Ivo\Desktop\Projekt\MiniPLC\Bitmaps/kontaktneg3.png", UriKind.Relative));
            kontaktneg4.ImageSource = new BitmapImage(new Uri(@"C:\Users\Ivo\Desktop\Projekt\MiniPLC\Bitmaps/kontaktneg4.png", UriKind.Relative));            
            merker1.ImageSource = new BitmapImage(new Uri(@"C:\Users\Ivo\Desktop\Projekt\MiniPLC\Bitmaps/merker1.png", UriKind.Relative));
            merker2.ImageSource = new BitmapImage(new Uri(@"C:\Users\Ivo\Desktop\Projekt\MiniPLC\Bitmaps/merker2.png", UriKind.Relative));
            merker3.ImageSource = new BitmapImage(new Uri(@"C:\Users\Ivo\Desktop\Projekt\MiniPLC\Bitmaps/merker3.png", UriKind.Relative));
            merkerneg1.ImageSource = new BitmapImage(new Uri(@"C:\Users\Ivo\Desktop\Projekt\MiniPLC\Bitmaps/merkerNeg1.png", UriKind.Relative));
            merkerneg2.ImageSource = new BitmapImage(new Uri(@"C:\Users\Ivo\Desktop\Projekt\MiniPLC\Bitmaps/merkerNeg2.png", UriKind.Relative));
            merkerneg3.ImageSource = new BitmapImage(new Uri(@"C:\Users\Ivo\Desktop\Projekt\Mi niPLC\Bitmaps/merkerNeg3.png", UriKind.Relative));
            casovac1.ImageSource = new BitmapImage(new Uri(@"C:\Users\Ivo\Desktop\Projekt\MiniPLC\Bitmaps/casovac1.png", UriKind.Relative));
            casovacNeg1.ImageSource = new BitmapImage(new Uri(@"C:\Users\Ivo\Desktop\Projekt\MiniPLC\Bitmaps/casovacNeg1.png", UriKind.Relative));
            casovac2.ImageSource = new BitmapImage(new Uri(@"C:\Users\Ivo\Desktop\Projekt\MiniPLC\Bitmaps/casovac2.png", UriKind.Relative));
            casovacNeg2.ImageSource = new BitmapImage(new Uri(@"C:\Users\Ivo\Desktop\Projekt\MiniPLC\Bitmaps/casovacNeg2.png", UriKind.Relative));
            citac1.ImageSource = new BitmapImage(new Uri(@"C:\Users\Ivo\Desktop\Projekt\MiniPLC\Bitmaps/citac1.png", UriKind.Relative));
            citacneg1.ImageSource = new BitmapImage(new Uri(@"C:\Users\Ivo\Desktop\Projekt\MiniPLC\Bitmaps/citacneg1.png", UriKind.Relative));            
            paralelnySpoj.ImageSource = new BitmapImage(new Uri(@"C:\Users\Ivo\Desktop\Projekt\MiniPLC\Bitmaps/parallel.png", UriKind.Relative));
            seriovySpoj.ImageSource = new BitmapImage(new Uri(@"C:\Users\Ivo\Desktop\Projekt\MiniPLC\Bitmaps/seriovy_kontakt.png", UriKind.Relative));

            // Inicializacia procesov
            kompilovanie.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            kompilovanie.StartInfo.FileName = @"C:\Users\Ivo\Desktop\Projekt\MiniPLC\bin\Debug\preklad.bat";
            napalovanie.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            napalovanie.StartInfo.FileName = @"avrdude.exe";

            // Inicializacia serioveho portu                       
            // seriovyPort.PortName = "c";            
            seriovyPort.BaudRate = 2400;
            seriovyPort.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(Prijem);

            // Inicializacia slovniku matOrigin
            for (int k = 0; k < PocetVystupov; ++k)
                for (int i = 0; i < PocetRiadkov; ++i)
                    for (int j = 0; j < PocetStlpcov; ++j)
                    {
                        mat[k, i].Add(j, "0");
                        matOrigin[k, i].Add(j, "0");
                    }
            #region Inicializacia rects
            rects[0, 0]["matrix0_0_0"] = matrix0_0_0;
            rects[0, 0]["matrix0_1_0"] = matrix0_1_0;
            rects[0, 0]["matrix0_2_0"] = matrix0_2_0;
            rects[0, 0]["matrix0_3_0"] = matrix0_3_0;

            rects[0, 1]["matrix0_0_1"] = matrix0_0_1;
            rects[0, 1]["matrix0_1_1"] = matrix0_1_1;
            rects[0, 1]["matrix0_2_1"] = matrix0_2_1;
            rects[0, 1]["matrix0_3_1"] = matrix0_3_1;

            rects[0, 2]["matrix0_0_2"] = matrix0_0_2;
            rects[0, 2]["matrix0_1_2"] = matrix0_1_2;
            rects[0, 2]["matrix0_2_2"] = matrix0_2_2;
            rects[0, 2]["matrix0_3_2"] = matrix0_3_2;

            rects[1, 0]["matrix1_0_0"] = matrix1_0_0;
            rects[1, 0]["matrix1_1_0"] = matrix1_1_0;
            rects[1, 0]["matrix1_2_0"] = matrix1_2_0;
            rects[1, 0]["matrix1_3_0"] = matrix1_3_0;

            rects[1, 1]["matrix1_0_1"] = matrix1_0_1;
            rects[1, 1]["matrix1_1_1"] = matrix1_1_1;
            rects[1, 1]["matrix1_2_1"] = matrix1_2_1;
            rects[1, 1]["matrix1_3_1"] = matrix1_3_1;

            rects[1, 2]["matrix1_0_2"] = matrix1_0_2;
            rects[1, 2]["matrix1_1_2"] = matrix1_1_2;
            rects[1, 2]["matrix1_2_2"] = matrix1_2_2;
            rects[1, 2]["matrix1_3_2"] = matrix1_3_2;

            rects[2, 0]["matrix2_0_0"] = matrix2_0_0;
            rects[2, 0]["matrix2_1_0"] = matrix2_1_0;
            rects[2, 0]["matrix2_2_0"] = matrix2_2_0;
            rects[2, 0]["matrix2_3_0"] = matrix2_3_0;

            rects[2, 1]["matrix2_0_1"] = matrix2_0_1;
            rects[2, 1]["matrix2_1_1"] = matrix2_1_1;
            rects[2, 1]["matrix2_2_1"] = matrix2_2_1;
            rects[2, 1]["matrix2_3_1"] = matrix2_3_1;

            rects[2, 2]["matrix2_0_2"] = matrix2_0_2;
            rects[2, 2]["matrix2_1_2"] = matrix2_1_2;
            rects[2, 2]["matrix2_2_2"] = matrix2_2_2;
            rects[2, 2]["matrix2_3_2"] = matrix2_3_2;

            rects[3, 0]["matrix3_0_0"] = matrix3_0_0;
            rects[3, 0]["matrix3_1_0"] = matrix3_1_0;
            rects[3, 0]["matrix3_2_0"] = matrix3_2_0;
            rects[3, 0]["matrix3_3_0"] = matrix3_3_0;

            rects[3, 1]["matrix3_0_1"] = matrix3_0_1;
            rects[3, 1]["matrix3_1_1"] = matrix3_1_1;
            rects[3, 1]["matrix3_2_1"] = matrix3_2_1;
            rects[3, 1]["matrix3_3_1"] = matrix3_3_1;

            rects[3, 2]["matrix3_0_2"] = matrix3_0_2;
            rects[3, 2]["matrix3_1_2"] = matrix3_1_2;
            rects[3, 2]["matrix3_2_2"] = matrix3_2_2;
            rects[3, 2]["matrix3_3_2"] = matrix3_3_2;

            rects[4, 0]["matrix4_0_0"] = matrix4_0_0;
            rects[4, 0]["matrix4_1_0"] = matrix4_1_0;
            rects[4, 0]["matrix4_2_0"] = matrix4_2_0;
            rects[4, 0]["matrix4_3_0"] = matrix4_3_0;

            rects[4, 1]["matrix4_0_1"] = matrix4_0_1;
            rects[4, 1]["matrix4_1_1"] = matrix4_1_1;
            rects[4, 1]["matrix4_2_1"] = matrix4_2_1;
            rects[4, 1]["matrix4_3_1"] = matrix4_3_1;

            rects[4, 2]["matrix4_0_2"] = matrix4_0_2;
            rects[4, 2]["matrix4_1_2"] = matrix4_1_2;
            rects[4, 2]["matrix4_2_2"] = matrix4_2_2;
            rects[4, 2]["matrix4_3_2"] = matrix4_3_2;

            rects[5, 0]["matrix5_0_0"] = matrix5_0_0;
            rects[5, 0]["matrix5_1_0"] = matrix5_1_0;
            rects[5, 0]["matrix5_2_0"] = matrix5_2_0;
            rects[5, 0]["matrix5_3_0"] = matrix5_3_0;

            rects[5, 1]["matrix5_0_1"] = matrix5_0_1;
            rects[5, 1]["matrix5_1_1"] = matrix5_1_1;
            rects[5, 1]["matrix5_2_1"] = matrix5_2_1;
            rects[5, 1]["matrix5_3_1"] = matrix5_3_1;

            rects[5, 2]["matrix5_0_2"] = matrix5_0_2;
            rects[5, 2]["matrix5_1_2"] = matrix5_1_2;
            rects[5, 2]["matrix5_2_2"] = matrix5_2_2;
            rects[5, 2]["matrix5_3_2"] = matrix5_3_2;

            rects[6, 0]["matrix6_0_0"] = matrix6_0_0;
            rects[6, 0]["matrix6_1_0"] = matrix6_1_0;
            rects[6, 0]["matrix6_2_0"] = matrix6_2_0;
            rects[6, 0]["matrix6_3_0"] = matrix6_3_0;

            rects[6, 1]["matrix6_0_1"] = matrix6_0_1;
            rects[6, 1]["matrix6_1_1"] = matrix6_1_1;
            rects[6, 1]["matrix6_2_1"] = matrix6_2_1;
            rects[6, 1]["matrix6_3_1"] = matrix6_3_1;

            rects[6, 2]["matrix6_0_2"] = matrix6_0_2;
            rects[6, 2]["matrix6_1_2"] = matrix6_1_2;
            rects[6, 2]["matrix6_2_2"] = matrix6_2_2;
            rects[6, 2]["matrix6_3_2"] = matrix6_3_2;

            rects[7, 0]["matrix7_0_0"] = matrix7_0_0;
            rects[7, 0]["matrix7_1_0"] = matrix7_1_0;
            rects[7, 0]["matrix7_2_0"] = matrix7_2_0;
            rects[7, 0]["matrix7_3_0"] = matrix7_3_0;

            rects[7, 1]["matrix7_0_1"] = matrix7_0_1;
            rects[7, 1]["matrix7_1_1"] = matrix7_1_1;
            rects[7, 1]["matrix7_2_1"] = matrix7_2_1;
            rects[7, 1]["matrix7_3_1"] = matrix7_3_1;

            rects[7, 2]["matrix7_0_2"] = matrix7_0_2;
            rects[7, 2]["matrix7_1_2"] = matrix7_1_2;
            rects[7, 2]["matrix7_2_2"] = matrix7_2_2;
            rects[7, 2]["matrix7_3_2"] = matrix7_3_2;

            rects[8, 0]["matrix8_0_0"] = matrix8_0_0;
            rects[8, 0]["matrix8_1_0"] = matrix8_1_0;
            rects[8, 0]["matrix8_2_0"] = matrix8_2_0;
            rects[8, 0]["matrix8_3_0"] = matrix8_3_0;

            rects[8, 1]["matrix8_0_1"] = matrix8_0_1;
            rects[8, 1]["matrix8_1_1"] = matrix8_1_1;
            rects[8, 1]["matrix8_2_1"] = matrix8_2_1;
            rects[8, 1]["matrix8_3_1"] = matrix8_3_1;

            rects[8, 2]["matrix8_0_2"] = matrix8_0_2;
            rects[8, 2]["matrix8_1_2"] = matrix8_1_2;
            rects[8, 2]["matrix8_2_2"] = matrix8_2_2;
            rects[8, 2]["matrix8_3_2"] = matrix8_3_2;

            rects[9, 0]["matrix9_0_0"] = matrix9_0_0;
            rects[9, 0]["matrix9_1_0"] = matrix9_1_0;
            rects[9, 0]["matrix9_2_0"] = matrix9_2_0;
            rects[9, 0]["matrix9_3_0"] = matrix9_3_0;

            rects[9, 1]["matrix9_0_1"] = matrix9_0_1;
            rects[9, 1]["matrix9_1_1"] = matrix9_1_1;
            rects[9, 1]["matrix9_2_1"] = matrix9_2_1;
            rects[9, 1]["matrix9_3_1"] = matrix9_3_1;

            rects[9, 2]["matrix9_0_2"] = matrix9_0_2;
            rects[9, 2]["matrix9_1_2"] = matrix9_1_2;
            rects[9, 2]["matrix9_2_2"] = matrix9_2_2;
            rects[9, 2]["matrix9_3_2"] = matrix9_3_2;

            rects[9, 0]["matrix9_0_0"] = matrix9_0_0;
            rects[9, 0]["matrix9_1_0"] = matrix9_1_0;
            rects[9, 0]["matrix9_2_0"] = matrix9_2_0;
            rects[9, 0]["matrix9_3_0"] = matrix9_3_0;

            rects[9, 1]["matrix9_0_1"] = matrix9_0_1;
            rects[9, 1]["matrix9_1_1"] = matrix9_1_1;
            rects[9, 1]["matrix9_2_1"] = matrix9_2_1;
            rects[9, 1]["matrix9_3_1"] = matrix9_3_1;

            rects[9, 2]["matrix9_0_2"] = matrix9_0_2;
            rects[9, 2]["matrix9_1_2"] = matrix9_1_2;
            rects[9, 2]["matrix9_2_2"] = matrix9_2_2;
            rects[9, 2]["matrix9_3_2"] = matrix9_3_2;

            rects[9, 0]["matrix9_0_0"] = matrix9_0_0;
            rects[9, 0]["matrix9_1_0"] = matrix9_1_0;
            rects[9, 0]["matrix9_2_0"] = matrix9_2_0;
            rects[9, 0]["matrix9_3_0"] = matrix9_3_0;

            rects[9, 1]["matrix9_0_1"] = matrix9_0_1;
            rects[9, 1]["matrix9_1_1"] = matrix9_1_1;
            rects[9, 1]["matrix9_2_1"] = matrix9_2_1;
            rects[9, 1]["matrix9_3_1"] = matrix9_3_1;

            rects[9, 2]["matrix9_0_2"] = matrix9_0_2;
            rects[9, 2]["matrix9_1_2"] = matrix9_1_2;
            rects[9, 2]["matrix9_2_2"] = matrix9_2_2;
            rects[9, 2]["matrix9_3_2"] = matrix9_3_2;


            rects[10, 0]["matrix10_0_0"] = matrix10_0_0;
            rects[10, 0]["matrix10_1_0"] = matrix10_1_0;
            rects[10, 0]["matrix10_2_0"] = matrix10_2_0;
            rects[10, 0]["matrix10_3_0"] = matrix10_3_0;

            rects[10, 1]["matrix10_0_1"] = matrix10_0_1;
            rects[10, 1]["matrix10_1_1"] = matrix10_1_1;
            rects[10, 1]["matrix10_2_1"] = matrix10_2_1;
            rects[10, 1]["matrix10_3_1"] = matrix10_3_1;

            rects[10, 2]["matrix10_0_2"] = matrix10_0_2;
            rects[10, 2]["matrix10_1_2"] = matrix10_1_2;
            rects[10, 2]["matrix10_2_2"] = matrix10_2_2;
            rects[10, 2]["matrix10_3_2"] = matrix10_3_2;

            rects[11, 0]["matrix11_0_0"] = matrix11_0_0;
            rects[11, 0]["matrix11_1_0"] = matrix11_1_0;
            rects[11, 0]["matrix11_2_0"] = matrix11_2_0;
            rects[11, 0]["matrix11_3_0"] = matrix11_3_0;

            rects[11, 1]["matrix11_0_1"] = matrix11_0_1;
            rects[11, 1]["matrix11_1_1"] = matrix11_1_1;
            rects[11, 1]["matrix11_2_1"] = matrix11_2_1;
            rects[11, 1]["matrix11_3_1"] = matrix11_3_1;

            rects[11, 2]["matrix11_0_2"] = matrix11_0_2;
            rects[11, 2]["matrix11_1_2"] = matrix11_1_2;
            rects[11, 2]["matrix11_2_2"] = matrix11_2_2;
            rects[11, 2]["matrix11_3_2"] = matrix11_3_2;

            rects[12, 0]["matrix12_0_0"] = matrix12_0_0;
            rects[12, 0]["matrix12_1_0"] = matrix12_1_0;
            rects[12, 0]["matrix12_2_0"] = matrix12_2_0;
            rects[12, 0]["matrix12_3_0"] = matrix12_3_0;

            rects[12, 1]["matrix12_0_1"] = matrix12_0_1;
            rects[12, 1]["matrix12_1_1"] = matrix12_1_1;
            rects[12, 1]["matrix12_2_1"] = matrix12_2_1;
            rects[12, 1]["matrix12_3_1"] = matrix12_3_1;

            rects[12, 2]["matrix12_0_2"] = matrix12_0_2;
            rects[12, 2]["matrix12_1_2"] = matrix12_1_2;
            rects[12, 2]["matrix12_2_2"] = matrix12_2_2;
            rects[12, 2]["matrix12_3_2"] = matrix12_3_2;
            #endregion rects
            #region Inicializacia parRects
            parRects[0, 0]["parallel0_0_0"] = parallel0_0_0;
            parRects[0, 0]["parallel0_1_0"] = parallel0_1_0;
            parRects[0, 0]["parallel0_2_0"] = parallel0_2_0;
            parRects[0, 0]["parallel0_3_0"] = parallel0_3_0;
            parRects[0, 0]["parallel0_4_0"] = parallel0_4_0;

            parRects[0, 1]["parallel0_0_1"] = parallel0_0_1;
            parRects[0, 1]["parallel0_1_1"] = parallel0_1_1;
            parRects[0, 1]["parallel0_2_1"] = parallel0_2_1;
            parRects[0, 1]["parallel0_3_1"] = parallel0_3_1;
            parRects[0, 1]["parallel0_4_1"] = parallel0_4_1;

            parRects[1, 0]["parallel1_0_0"] = parallel1_0_0;
            parRects[1, 0]["parallel1_1_0"] = parallel1_1_0;
            parRects[1, 0]["parallel1_2_0"] = parallel1_2_0;
            parRects[1, 0]["parallel1_3_0"] = parallel1_3_0;
            parRects[1, 0]["parallel1_4_0"] = parallel1_4_0;

            parRects[1, 1]["parallel1_0_1"] = parallel1_0_1;
            parRects[1, 1]["parallel1_1_1"] = parallel1_1_1;
            parRects[1, 1]["parallel1_2_1"] = parallel1_2_1;
            parRects[1, 1]["parallel1_3_1"] = parallel1_3_1;
            parRects[1, 1]["parallel1_4_1"] = parallel1_4_1;

            parRects[2, 0]["parallel2_0_0"] = parallel2_0_0;
            parRects[2, 0]["parallel2_1_0"] = parallel2_1_0;
            parRects[2, 0]["parallel2_2_0"] = parallel2_2_0;
            parRects[2, 0]["parallel2_3_0"] = parallel2_3_0;
            parRects[2, 0]["parallel2_4_0"] = parallel2_4_0;

            parRects[2, 1]["parallel2_0_1"] = parallel2_0_1;
            parRects[2, 1]["parallel2_1_1"] = parallel2_1_1;
            parRects[2, 1]["parallel2_2_1"] = parallel2_2_1;
            parRects[2, 1]["parallel2_3_1"] = parallel2_3_1;
            parRects[2, 1]["parallel2_4_1"] = parallel2_4_1;

            parRects[3, 0]["parallel3_0_0"] = parallel3_0_0;
            parRects[3, 0]["parallel3_1_0"] = parallel3_1_0;
            parRects[3, 0]["parallel3_2_0"] = parallel3_2_0;
            parRects[3, 0]["parallel3_3_0"] = parallel3_3_0;
            parRects[3, 0]["parallel3_4_0"] = parallel3_4_0;

            parRects[3, 1]["parallel3_0_1"] = parallel3_0_1;
            parRects[3, 1]["parallel3_1_1"] = parallel3_1_1;
            parRects[3, 1]["parallel3_2_1"] = parallel3_2_1;
            parRects[3, 1]["parallel3_3_1"] = parallel3_3_1;
            parRects[3, 1]["parallel3_4_1"] = parallel3_4_1;

            parRects[4, 0]["parallel4_0_0"] = parallel4_0_0;
            parRects[4, 0]["parallel4_1_0"] = parallel4_1_0;
            parRects[4, 0]["parallel4_2_0"] = parallel4_2_0;
            parRects[4, 0]["parallel4_3_0"] = parallel4_3_0;
            parRects[4, 0]["parallel4_4_0"] = parallel4_4_0;

            parRects[4, 1]["parallel4_0_1"] = parallel4_0_1;
            parRects[4, 1]["parallel4_1_1"] = parallel4_1_1;
            parRects[4, 1]["parallel4_2_1"] = parallel4_2_1;
            parRects[4, 1]["parallel4_3_1"] = parallel4_3_1;
            parRects[4, 1]["parallel4_4_1"] = parallel4_4_1;

            parRects[5, 0]["parallel5_0_0"] = parallel5_0_0;
            parRects[5, 0]["parallel5_1_0"] = parallel5_1_0;
            parRects[5, 0]["parallel5_2_0"] = parallel5_2_0;
            parRects[5, 0]["parallel5_3_0"] = parallel5_3_0;
            parRects[5, 0]["parallel5_4_0"] = parallel5_4_0;

            parRects[5, 1]["parallel5_0_1"] = parallel5_0_1;
            parRects[5, 1]["parallel5_1_1"] = parallel5_1_1;
            parRects[5, 1]["parallel5_2_1"] = parallel5_2_1;
            parRects[5, 1]["parallel5_3_1"] = parallel5_3_1;
            parRects[5, 1]["parallel5_4_1"] = parallel5_4_1;

            parRects[6, 0]["parallel6_0_0"] = parallel6_0_0;
            parRects[6, 0]["parallel6_1_0"] = parallel6_1_0;
            parRects[6, 0]["parallel6_2_0"] = parallel6_2_0;
            parRects[6, 0]["parallel6_3_0"] = parallel6_3_0;
            parRects[6, 0]["parallel6_4_0"] = parallel6_4_0;

            parRects[6, 1]["parallel6_0_1"] = parallel6_0_1;
            parRects[6, 1]["parallel6_1_1"] = parallel6_1_1;
            parRects[6, 1]["parallel6_2_1"] = parallel6_2_1;
            parRects[6, 1]["parallel6_3_1"] = parallel6_3_1;
            parRects[6, 1]["parallel6_4_1"] = parallel6_4_1;

            parRects[7, 0]["parallel7_0_0"] = parallel7_0_0;
            parRects[7, 0]["parallel7_1_0"] = parallel7_1_0;
            parRects[7, 0]["parallel7_2_0"] = parallel7_2_0;
            parRects[7, 0]["parallel7_3_0"] = parallel7_3_0;
            parRects[7, 0]["parallel7_4_0"] = parallel7_4_0;

            parRects[7, 1]["parallel7_0_1"] = parallel7_0_1;
            parRects[7, 1]["parallel7_1_1"] = parallel7_1_1;
            parRects[7, 1]["parallel7_2_1"] = parallel7_2_1;
            parRects[7, 1]["parallel7_3_1"] = parallel7_3_1;
            parRects[7, 1]["parallel7_4_1"] = parallel7_4_1;

            parRects[8, 0]["parallel8_0_0"] = parallel8_0_0;
            parRects[8, 0]["parallel8_1_0"] = parallel8_1_0;
            parRects[8, 0]["parallel8_2_0"] = parallel8_2_0;
            parRects[8, 0]["parallel8_3_0"] = parallel8_3_0;
            parRects[8, 0]["parallel8_4_0"] = parallel8_4_0;

            parRects[8, 1]["parallel8_0_1"] = parallel8_0_1;
            parRects[8, 1]["parallel8_1_1"] = parallel8_1_1;
            parRects[8, 1]["parallel8_2_1"] = parallel8_2_1;
            parRects[8, 1]["parallel8_3_1"] = parallel8_3_1;
            parRects[8, 1]["parallel8_4_1"] = parallel8_4_1;

            parRects[9, 0]["parallel9_0_0"] = parallel9_0_0;
            parRects[9, 0]["parallel9_1_0"] = parallel9_1_0;
            parRects[9, 0]["parallel9_2_0"] = parallel9_2_0;
            parRects[9, 0]["parallel9_3_0"] = parallel9_3_0;
            parRects[9, 0]["parallel9_4_0"] = parallel9_4_0;

            parRects[9, 1]["parallel9_0_1"] = parallel9_0_1;
            parRects[9, 1]["parallel9_1_1"] = parallel9_1_1;
            parRects[9, 1]["parallel9_2_1"] = parallel9_2_1;
            parRects[9, 1]["parallel9_3_1"] = parallel9_3_1;
            parRects[9, 1]["parallel9_4_1"] = parallel9_4_1;

            parRects[10, 0]["parallel10_0_0"] = parallel10_0_0;
            parRects[10, 0]["parallel10_1_0"] = parallel10_1_0;
            parRects[10, 0]["parallel10_2_0"] = parallel10_2_0;
            parRects[10, 0]["parallel10_3_0"] = parallel10_3_0;
            parRects[10, 0]["parallel10_4_0"] = parallel10_4_0;

            parRects[10, 1]["parallel10_0_1"] = parallel10_0_1;
            parRects[10, 1]["parallel10_1_1"] = parallel10_1_1;
            parRects[10, 1]["parallel10_2_1"] = parallel10_2_1;
            parRects[10, 1]["parallel10_3_1"] = parallel10_3_1;
            parRects[10, 1]["parallel10_4_1"] = parallel10_4_1;

            parRects[11, 0]["parallel11_0_0"] = parallel11_0_0;
            parRects[11, 0]["parallel11_1_0"] = parallel11_1_0;
            parRects[11, 0]["parallel11_2_0"] = parallel11_2_0;
            parRects[11, 0]["parallel11_3_0"] = parallel11_3_0;
            parRects[11, 0]["parallel11_4_0"] = parallel11_4_0;

            parRects[11, 1]["parallel11_0_1"] = parallel11_0_1;
            parRects[11, 1]["parallel11_1_1"] = parallel11_1_1;
            parRects[11, 1]["parallel11_2_1"] = parallel11_2_1;
            parRects[11, 1]["parallel11_3_1"] = parallel11_3_1;
            parRects[11, 1]["parallel11_4_1"] = parallel11_4_1;

            parRects[12, 0]["parallel12_0_0"] = parallel12_0_0;
            parRects[12, 0]["parallel12_1_0"] = parallel12_1_0;
            parRects[12, 0]["parallel12_2_0"] = parallel12_2_0;
            parRects[12, 0]["parallel12_3_0"] = parallel12_3_0;
            parRects[12, 0]["parallel12_4_0"] = parallel12_4_0;

            parRects[12, 1]["parallel12_0_1"] = parallel12_0_1;
            parRects[12, 1]["parallel12_1_1"] = parallel12_1_1;
            parRects[12, 1]["parallel12_2_1"] = parallel12_2_1;
            parRects[12, 1]["parallel12_3_1"] = parallel12_3_1;
            parRects[12, 1]["parallel12_4_1"] = parallel12_4_1;
            #endregion Inicializacia parRects
        }

        #region Ovladanie
        // Zobrazenie vyberu
        private void vyber_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Rectangle temp = (Rectangle)sender;            
            VyberWindow vyber = new VyberWindow(temp, e.GetPosition(this).X, e.GetPosition(this).Y);
            if (e.ClickCount == 2)
            {
                int r = 0;
                int s = 0;
                int v = 0;
                string str = temp.Name.ToString().Remove(0, 6);                
                string[] a = str.Split('_');
                v = Int32.Parse(a[0].ToString());
                s = Int32.Parse(a[1].ToString());
                r = Int32.Parse(a[2].ToString());
                
                matOrigin[v, r][s] = "1";
                temp.Fill = seriovySpoj;                  
            }
            else if (e.ClickCount == 1)
            {                                
                if (isVyberOkno != true)
                {                    
                    isVyberOkno = true;
                    vyber.Show();
                }
                isSaved = false;                
            }            
        }

        // Mazanie
        private void vyber_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Rectangle temp = (Rectangle)sender;
            int r = 0;
            int s = 0;
            int v = 0;
            string str = temp.Name.ToString().Remove(0, 6);                        
            string[] a = str.Split('_');
            v = Int32.Parse(a[0].ToString());
            s = Int32.Parse(a[1].ToString());
            r = Int32.Parse(a[2].ToString());
            
            matOrigin[v, r][s] = "0";
            temp.Fill = Brushes.Transparent;
            isSaved = false;
        }

        // Paralelne spojenie - Pridanie
        private void parallel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            int s = 0;
            int r = 0;
            int v = 0;
            Rectangle x = (Rectangle)sender;
            string str = x.Name.ToString().Remove(0, 8);
            x.Fill = paralelnySpoj;            
                string[] a = str.Split('_');
                v = Int32.Parse(a[0].ToString());
                s = Int32.Parse(a[1].ToString());
                r = Int32.Parse(a[2].ToString());            

            par[v, r, s] = true;
            isSaved = false;
        }

        // Paralelne spojenie - Mazanie
        private void parallel_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            int s = 0;
            int r = 0;
            int v = 0;
            Rectangle x = (Rectangle)sender;
            string str = x.Name.ToString().Remove(0, 8);
            x.Fill = Brushes.Transparent;

                string[] a = str.Split('_');
                v = Int32.Parse(a[0].ToString());
                s = Int32.Parse(a[1].ToString());
                r = Int32.Parse(a[2].ToString());

            par[v, r, s] = false;
            isSaved = false;
        }

        // Kompilovanie *.C->*.hex
        private void buttonKompiluj_Click(object sender, RoutedEventArgs e)
        {
            if (isSaved != true)
            {
                WPFMessageBox.Show("Neuložené...", "Neboli uložené zmeny!", WPFMessageBoxButtons.OK, WPFMessageBoxImage.Alert);
                 Microsoft.Win32.SaveFileDialog saveDlg = new Microsoft.Win32.SaveFileDialog();
                 saveDlg.DefaultExt = ".c";
                 saveDlg.Filter = "Dokument C (.c)|*.c";
                 if (saveDlg.ShowDialog() == true)
                 {
                     menoSuboru = saveDlg.FileName.Remove(saveDlg.FileName.Length - 2, 2);
                     File.WriteAllText(menoSuboru + ".c", program);
                     SaveData(par, menoSuboru + "Arr" + ".dat");
                     SaveData(matOrigin, menoSuboru + "Dic" + ".dat");
                     this.Title = menoSuboru;
                     isSaved = true;
                 }
                
            }
            else
            {
                for (int k = 0; k < PocetVystupov; ++k)
                    for (int i = 0; i < PocetRiadkov; ++i)
                        mat[k, i] = new Dictionary<int, string>(matOrigin[k, i]);

                program = "";
                funkcia = "";
                if (radioOff.IsChecked == true)
                {
                    Casovac1_1 = Casovac1_1.Replace("0", "1");                                        
                }                
                program += Hlavicky + Prototypy + Casovac1_1 + Citac1_1 + Prerusenia + Main1;
                for (int i = 0; i < PocetVystupov; ++i)
                {
                    funkcia = generujFunkciu(i);
                    if (funkcia == "0 && 0 && 0 && 0" && i != 10)
                        continue;
                    funkcia = funkcia.Insert(0, "if(");
                    switch (i)
                    {
                        // Vystupy
                        case 0:
                            funkcia += "){PORTB |= (1<<PB3);}\n" + ElseQ1;
                            break;
                        case 1:
                            funkcia += "){PORTB |= (1<<PB4);}\n" + ElseQ2;
                            break;
                        case 2:
                            funkcia += "){PORTD |= (1<<PD2);}\n" + ElseQ3;
                            break;

                        // Casovac 1
                        case 3:
                            funkcia += "){TCCR1B = (1 << WGM12) | (3 << CS10);\nif(!_ZAP && _OFF1) _TIMER1 = 1;}\n" + ElseT1;
                            try
                            {
                                cas = Convert.ToInt32(editCasovacCas1.Text);
                            }
                            catch (FormatException)
                            {
                                WPFMessageBox.Show("Chyba! - Časovač 1", "Chybne zadaný čas", WPFMessageBoxButtons.OK, WPFMessageBoxImage.Error);
                                return;
                            }
                            program += IfCasovac1_1 + cas + IfCasovac2_1;
                            break;

                        case 4:
                            funkcia += "){_M1 = 1;}";
                            break;                        
                        case 5:
                            funkcia += "){_M1 = 0;}";
                            break;
                        case 6:
                            funkcia += "){_M2 = 1;}";
                            break;
                        case 7:
                            funkcia += "){_M2 = 0;}";
                            break;
                        case 8:
                            funkcia += "){_M3 = 1;}";
                            break;
                        case 9:
                            funkcia += "){_M3 = 0;}";
                            break;

                        // Citac 1
                        case 10:
                            Citac1Prerusenie1 = "ISR(INT1_vect)\n{\n_COUNTER1INT = 1;\n";
                            funkcia += "){\nif(_DIR1 == 0){counter1++;}\nelse if(_DIR1 == 1 && counter1 > 0){counter1--;}\n_COUNTER1INT = 0;}\n}\n";                              
                            try
                            {                                                                
                                pocet = Convert.ToInt32(editCitacPocet1.Text);                                                                
                            }
                            catch (FormatException)
                            {
                                WPFMessageBox.Show("Chyba! - Čítač 1", "Chybne zadaný počet", WPFMessageBoxButtons.OK, WPFMessageBoxImage.Error);
                                return;
                            }
                            program += IfCitac1_1 + pocet + IfCitac2_1 + ElseC1;
                            break;                                                    
                        case 11:
                            funkcia += "){counter1 = 0;serialPosli('O');}\nelse serialPosli('o');";                            
                            break;
                        case 12:
                            funkcia += "){_DIR1 = 1;}\nelse{ _DIR1 = 0;}\n";
                            break;                               
                    }
                    if (i == 10)
                        Citac1Prerusenie1 += funkcia;
                    else
                        program += funkcia;
                }

                program += Main2 + Citac1Prerusenie1 + NastavenieSerialPortu + SerialPosli + SerialPrijmi + PosliVstupy + PosliVystypy;
                File.WriteAllText(menoSuboru + ".c", program);                

                kompilovanie.StartInfo.Arguments = menoSuboru;
                kompilovanie.Start();
                kompilovanie.WaitForExit();
                if (kompilovanie.ExitCode == 0)
                {
                    WPFMessageBox.Show("Preklad...", "Program bol úspešne preložený!", WPFMessageBoxButtons.OK, WPFMessageBoxImage.Information);
                }
                else
                {
                    WPFMessageBox.Show("Preklad...", "Program nebol preložený!", WPFMessageBoxButtons.OK, WPFMessageBoxImage.Error);
                }
            }
        }

        // Napalovanie do uProcesoru
        private void buttonNahraj_Click(object sender, RoutedEventArgs e)
        {
            // string ms = menoSuboru.Remove(0, 2).Replace("\\", "/");            
            napalovanie.StartInfo.Arguments = "-p attiny2313 -c siprog -V -P " + portNapalovanie + " -U flash:w:" + menoSuboru + ".hex";
            napalovanie.Start();
            napalovanie.WaitForExit();
            if (napalovanie.ExitCode == 0)
            {
                WPFMessageBox.Show("Napalovanie...", "Program bol uspesne napaleny!", WPFMessageBoxButtons.OK, WPFMessageBoxImage.Information);
            }
            else
            {
                WPFMessageBox.Show("Chyba pri napalovani...", "Program nebol napaleny!", WPFMessageBoxButtons.OK, WPFMessageBoxImage.Error);
            }
        }
        #endregion Ovladanie

        #region Algoritmy
        // Vytvorenie logickej funkcie - hladanie paralelnych spojov      
        public string generujFunkciu(int v)
        {
            // Prehladavanie riadkov - od posledneho riadku k prvemu
            for (int i = PocetParRiadkov - 1; i >= 0; --i)
            {
                // Prehladavanie stlpcov v i-tom riadku 
                for (int j = 0; j < PocetParStlpcov; ++j)
                {
                    // Najdeny prvy paralelny spoj
                    if (par[v, i, j] == true)
                    {
                        // Hladanie druheho paralelneho spoju - od miesta prveho najdeneho
                        for (int k = j + 1; k < PocetParStlpcov; ++k)
                        {
                            // Najdeny druhy paralelny spoj
                            if (par[v, i, k] == true)
                            {
                                // Ak su hned vedla seba
                                if ((k - j) == 1)
                                {
                                    mat[v, i][k - 1] += " || " + SpojRiadok(v, i + 1, j, k) + ")";
                                    mat[v, i][k - 1] = mat[v, i][k - 1].Insert(0, "(");
                                    break;
                                }
                                // Ak je medzi nimi seriovy spoj
                                else
                                {
                                    mat[v, i][k - 1] += " || " + SpojRiadok(v, i + 1, j, k) + ")";
                                    mat[v, i][j] = mat[v, i][j].Insert(0, "(");
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            return SpojRiadok(v, 0, 0, PocetStlpcov);
        }

        // Funkcia spaja riadky - seriove spoje
        public string SpojRiadok(int v, int riadok, int prvy, int druhy)
        {
            string str = "";
            for (int i = prvy; i < druhy; ++i)
            {
                str += mat[v, riadok][i] + " && ";
            }

            return str.Remove(str.Length - 4, 4);
        }
        #endregion Algoritmy

        #region Komunikacia
        // Zmena tabs
        private void tabControl1_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (tabControl1.SelectedItem == tabItem_Schemes)
            {
                if (seriovyPort.IsOpen)
                {
                    // Dispatcher.InvokeShutdown();
                    seriovyPort.DataReceived -= new System.IO.Ports.SerialDataReceivedEventHandler(Prijem);
                    seriovyPort.Close();
                }
            }
            else if (tabControl1.SelectedItem == tabItem_Check)
            {

            }
        }        

        // Port pre napalovanie
        private void comboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem cbi = (ComboBoxItem)comboBoxNapalovanie.SelectedItem;
            portNapalovanie = cbi.Content.ToString();
        }

        // Port pre komunikaciu
        private void comboBoxKomunikacia_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (seriovyPort.IsOpen)
            {
                seriovyPort.DataReceived -= new System.IO.Ports.SerialDataReceivedEventHandler(Prijem);
                seriovyPort.Close();
            }
            try
            {
                ComboBoxItem cbi = (ComboBoxItem)comboBoxKomunikacia.SelectedItem;
                portKomunikacia = cbi.Content.ToString();
                seriovyPort.PortName = portKomunikacia;
                seriovyPort.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(Prijem);
                seriovyPort.BaudRate = 2400;
                seriovyPort.Open();
                seriovyPort.DiscardInBuffer();
            }
            catch (IOException ex)
            {
                WPFMessageBox.Show("Chyba!", "Chyba serioveho portu", ex.ToString(), WPFMessageBoxButtons.OK, WPFMessageBoxImage.Error);
            }
        }

        // Citanie dat
        //private delegate void UpdateUiTextDelegate(char text);
        private delegate void UpdateUiTextDelegate(string text);
        private void Prijem(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            if (seriovyPort.IsOpen)
            {
                try
                {
                    string str = "";
                    //char c = (char)seriovyPort.ReadChar();
                     while (seriovyPort.ReadChar() != '@')
                        continue;
                    for (int i = 0; i < 17; ++i)
                    {
                        str += (char)seriovyPort.ReadChar();
                    } 
                
                    //Dispatcher.BeginInvoke(DispatcherPriority.Send,
                       //new UpdateUiTextDelegate(WriteData), c);
                    Dispatcher.BeginInvoke(DispatcherPriority.Send,
                       new UpdateUiTextDelegate(WriteData), str);
                }
                catch (IOException ex)
                {
                    WPFMessageBox.Show("Chyba!", "Chyba serioveho portu", ex.ToString(), WPFMessageBoxButtons.OK, WPFMessageBoxImage.Error);
                }
            }
        }

        // Update checkboxov
        //private void WriteData(char c)
        private void WriteData(string s)
        {
            if (seriovyPort.IsOpen)
            {
                #region schovaj sa
                /*
                if (c == 'A')
                {
                    checkBoxI1.IsChecked = true;
                }
                else if (c == 'a')
                {
                    checkBoxI1.IsChecked = false;
                }
                if (c == 'B')
                {
                    checkBoxI2.IsChecked = true;
                }
                else if (c == 'b')
                {
                    checkBoxI2.IsChecked = false;
                }
                if (c == 'C')
                {
                    checkBoxI3.IsChecked = true;
                }
                else if (c == 'c')
                {
                    checkBoxI3.IsChecked = false;
                }
                if (c == 'D')
                {
                    checkBoxQ1.IsChecked = true;
                }
                else if (c == 'd')
                {
                    checkBoxQ1.IsChecked = false;
                }
                if (c == 'E')
                {
                    checkBoxQ2.IsChecked = true;
                }
                else if (c == 'e')
                {
                    checkBoxQ2.IsChecked = false;
                }
                if (c == 'F')
                {
                    checkBoxQ3.IsChecked = true;
                }
                else if (c == 'f')
                {
                    checkBoxQ3.IsChecked = false;
                }
                if (c == 'G')
                {
                    checkBoxM1.IsChecked = true;
                }
                else if (c == 'g')
                {
                    checkBoxM1.IsChecked = false;
                }
                if (c == 'H')
                {
                    checkBoxM2.IsChecked = true;
                }
                else if (c == 'h')
                {
                    checkBoxM2.IsChecked = false;
                }
                if (c == 'I')
                {
                    checkBoxM3.IsChecked = true;
                }
                else if (c == 'i')
                {
                    checkBoxM3.IsChecked = false;
                }
                if (c == 'J')
                {
                    checkBoxTMR1.IsChecked = true;
                }
                else if (c == 'j')
                {
                    checkBoxTMR1.IsChecked = false;
                }
                if (c == 'K')
                {                
                    checkBoxC1INT.IsChecked = true;
                }                
                else if(c == 'k')
                {
                    checkBoxC1INT.IsChecked = false;
                }
                if (c == 'L')
                {
                    checkBoxT1.IsChecked = true;
                }
                else if (c == 'l')
                {
                    checkBoxT1.IsChecked = false;
                }
                if (c == 'M')
                {
                    checkBoxI4.IsChecked = true;
                    checkBoxC1.IsChecked = true; // ajajajajaj
                }
                else if (c == 'm')
                {
                    checkBoxI4.IsChecked = false;
                    checkBoxC1.IsChecked = false; // ajajajajaj
                }
                if (c == 'N')
                { 
                    checkBoxC1DIR.IsChecked = true;
                }
                else if (c == 'n')
                {
                    checkBoxC1DIR.IsChecked = false;
                }
                if (c == 'O')
                {
                    checkBoxC1RESET.IsChecked = true;
                }
                else if (c == 'o')
                {
                    checkBoxC1RESET.IsChecked = false;
                } */
                #endregion
                checkBoxI1.IsChecked = s[0] == 'A';
                checkBoxI2.IsChecked = s[1] == 'B';
                checkBoxI3.IsChecked = s[2] == 'C';
                checkBoxC1INT.IsChecked = s[3] == 'K';
                checkBoxT1.IsChecked = s[4] == 'L';
                checkBoxQ1.IsChecked = s[5] == 'D';
                checkBoxQ2.IsChecked = s[6] == 'E';
                checkBoxQ3.IsChecked = s[7] == 'F';
                checkBoxM1.IsChecked = s[8] == 'G';
                checkBoxM2.IsChecked = s[9] == 'H';
                checkBoxM3.IsChecked = s[10] == 'I';
                checkBoxTMR1.IsChecked = s[11] == 'J';
                checkBoxI4.IsChecked = s[12] == 'M';
                checkBoxC1.IsChecked = s[12] == 'M';
                checkBoxC1DIR.IsChecked = s[13] == 'N';
                labelPocet1.Content = (int)s[14];
                labelCas1.Content = (int)s[15];
                checkBoxC1RESET.IsChecked = s[16] == 'O';                
            }
        }
        #endregion Komunikacia

        #region Save + Open + Menu
        // Ulozenie slovniku
        public void SaveData(Dictionary<int, string>[,] obj, string fileName)
        {
            using (var stream = File.OpenWrite(fileName))
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, obj);
            }
        }

        // Ulozenie 3D pola
        public void SaveData(bool[, ,] obj, string fileName)
        {
            using (var stream = File.OpenWrite(fileName))
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, obj);
            }
        }

        // Nacitanie slovniku
        public void LoadDataDict(string fileName)
        {
            using (var stream = File.OpenRead(fileName))
            {
                var formatter = new BinaryFormatter();
                matOrigin = (Dictionary<int, string>[,])formatter.Deserialize(stream);
            }
        }

        // Nacitanie 3D pola
        public void LoadData2DArray(string fileName)
        {
            using (var stream = File.OpenRead(fileName))
            {
                var formatter = new BinaryFormatter();
                par = (bool[, ,])formatter.Deserialize(stream);

            }
        }

        // Refresh nacitanej schemy
        public void LoadBitmaps()
        {
            string str = null;          
            #region seriove
            for (int k = 0; k < PocetVystupov; ++k)
                for (int i = 0; i < PocetRiadkov; ++i)
                    for (int j = 0; j < PocetStlpcov; ++j)
                    {
                        str = "matrix";
                        if (matOrigin[k, i][j] == "PINB & (1 << PB0)")
                        {
                            str += k.ToString() + "_" + j.ToString() + "_" + i.ToString();
                            rects[k, i][str].Fill = kontakt1;
                        }
                        else if (matOrigin[k, i][j] == "PINB & (1 << PB1)")
                        {
                            str += k.ToString() + "_" + j.ToString() + "_" + i.ToString();
                            rects[k, i][str].Fill = kontakt2;
                        }
                        else if (matOrigin[k, i][j] == "PINB & (1 << PB2)")
                        {
                            str += k.ToString() + "_" + j.ToString() + "_" + i.ToString();
                            rects[k, i][str].Fill = kontakt3;
                        }
                        else if (matOrigin[k, i][j] == "!PINB & (1 << PB0)")
                        {
                            str += k.ToString() + "_" + j.ToString() + "_" + i.ToString();
                            rects[k, i][str].Fill = kontaktneg1;
                        }
                        else if (matOrigin[k, i][j] == "!PINB & (1 << PB1)")
                        {
                            str += k.ToString() + "_" + j.ToString() + "_" + i.ToString();
                            rects[k, i][str].Fill = kontaktneg2;
                        }
                        else if (matOrigin[k, i][j] == "!PINB & (1 << PB2)")
                        {
                            str += k.ToString() + "_" + j.ToString() + "_" + i.ToString();
                            rects[k, i][str].Fill = kontaktneg3;
                        }
                        else if (matOrigin[k, i][j] == "_M1")
                        {
                            str += k.ToString() + "_" + j.ToString() + "_" + i.ToString();
                            rects[k, i][str].Fill = merker1;
                        }
                        else if (matOrigin[k, i][j] == "_M2")
                        {
                            str += k.ToString() + "_" + j.ToString() + "_" + i.ToString();
                            rects[k, i][str].Fill = merker2;
                        }
                        else if (matOrigin[k, i][j] == "_M3")
                        {
                            str += k.ToString() + "_" + j.ToString() + "_" + i.ToString();
                            rects[k, i][str].Fill = merker3;
                        }
                        else if (matOrigin[k, i][j] == "!_M1")
                        {
                            str += k.ToString() + "_" + j.ToString() + "_" + i.ToString();
                            rects[k, i][str].Fill = merkerneg1;
                        }
                        else if (matOrigin[k, i][j] == "!_M2")
                        {
                            str += k.ToString() + "_" + j.ToString() + "_" + i.ToString();
                            rects[k, i][str].Fill = merkerneg2;
                        }
                        else if (matOrigin[k, i][j] == "!_M3")
                        {
                            str += k.ToString() + "_" + j.ToString() + "_" + i.ToString();
                            rects[k, i][str].Fill = merkerneg3;
                        }
                        else if (matOrigin[k, i][j] == "_TIMER1")
                        {
                            str += k.ToString() + "_" + j.ToString() + "_" + i.ToString();
                            rects[k, i][str].Fill = casovac1;
                        }
                        else if (matOrigin[k, i][j] == "!_TIMER1")
                        {
                            str += k.ToString() + "_" + j.ToString() + "_" + i.ToString();
                            rects[k, i][str].Fill = casovacNeg1;
                        }
                        else if (matOrigin[k, i][j] == "_COUNTER1")
                        {
                            str += k.ToString() + "_" + j.ToString() + "_" + i.ToString();
                            rects[k, i][str].Fill = citac1;
                        }
                        else if (matOrigin[k, i][j] == "!_COUNTER1")
                        {
                            str += k.ToString() + "_" + j.ToString() + "_" + i.ToString();
                            rects[k, i][str].Fill = citacneg1;
                        }
                        else if (matOrigin[k, i][j] == "_COUNTER1INT")
                        {
                            str += k.ToString() + "_" + j.ToString() + "_" + i.ToString();
                            rects[k, i][str].Fill = kontakt4;
                        }
                        else if (matOrigin[k, i][j] == "!_COUNTER1INT")
                        {
                            str += k.ToString() + "_" + j.ToString() + "_" + i.ToString();
                            rects[k, i][str].Fill = kontaktneg4;
                        }
                        else if (matOrigin[k, i][j] == "1")
                        {
                            str += k.ToString() + "_" + j.ToString() + "_" + i.ToString();
                            rects[k, i][str].Fill = seriovySpoj;
                        }
                        else if (matOrigin[k, i][j] == "0")
                        {
                            str += k.ToString() + "_" + j.ToString() + "_" + i.ToString();
                            rects[k, i][str].Fill = Brushes.Transparent;
                        }
                    }
            #endregion seriove
            #region paralelne            
            for (int k = 0; k < PocetVystupov; ++k)
                for (int i = 0; i < PocetParRiadkov; ++i)
                    for (int j = 0; j < PocetParStlpcov; ++j)
                    {
                        str = "parallel";
                        str += k.ToString() + "_" + j.ToString() + "_" + i.ToString();
                        if (par[k, i, j] == true)
                        {                            
                            parRects[k, i][str].Fill = paralelnySpoj;
                        }
                        else
                        {                            
                            parRects[k, i][str].Fill = Brushes.Transparent;
                        }
                    }
            #endregion paralelne
        }

        // Reset matOrigin | par 
        private void resetDicts()
        {
            for (int k = 0; k < PocetVystupov; ++k)
                for (int i = 0; i < PocetRiadkov; ++i)
                    for (int j = 0; j < PocetStlpcov; ++j)
                        matOrigin[k, i][j] = "0";
            for (int k = 0; k < PocetVystupov; ++k)
                for (int i = 0; i < PocetParRiadkov; ++i)
                    for (int j = 0; j < PocetParStlpcov; ++j)
                        par[k, i, j] = false;       
        }

        // Novy projekt
        private void menuNew_Click(object sender, RoutedEventArgs e)
        {
            if (isSaved == true)
            {
                resetDicts();
                LoadBitmaps();
                menoSuboru = "";                
                this.Title = "Bez názvu";
                isSaved = false;
            }
            else
            {
                WPFMessageBoxResult result = WPFMessageBox.Show("Neuložené...", "Neboli uložené zmeny!\nPokračovať ?", WPFMessageBoxButtons.YesNo, WPFMessageBoxImage.Alert);
                if (result == WPFMessageBoxResult.Yes)
                {
                    resetDicts();
                    LoadBitmaps();
                    menoSuboru = "";
                    isSaved = true;                    
                    this.Title = "Bez názvu";
                }                
            }
        }

        // Ulozit projekt
        private void menuSave_Click(object sender, RoutedEventArgs e)
        {
            if (menoSuboru != "")
            {
                File.WriteAllText(menoSuboru + ".c", program);
                SaveData(par, menoSuboru + "Arr" + ".dat");
                SaveData(matOrigin, menoSuboru + "Dic" + ".dat");
                isSaved = true;
            }
            else
            {
                Microsoft.Win32.SaveFileDialog saveDlg = new Microsoft.Win32.SaveFileDialog();
                saveDlg.DefaultExt = ".c";
                saveDlg.Filter = "Dokument C (.c)|*.c";
                if (saveDlg.ShowDialog() == true)
                {
                    menoSuboru = saveDlg.FileName.Remove(saveDlg.FileName.Length - 2, 2);
                    File.WriteAllText(menoSuboru + ".c", program);
                    SaveData(par, menoSuboru + "Arr" + ".dat");
                    SaveData(matOrigin, menoSuboru + "Dic" + ".dat");
                    this.Title = menoSuboru;
                    isSaved = true;
                }
            }
        }

        // Ulozit projekt ako...
        private void menuSaveAs_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveDlg = new Microsoft.Win32.SaveFileDialog();
            saveDlg.DefaultExt = ".c";
            saveDlg.Filter = "Dokument C (.c)|*.c";
            if (saveDlg.ShowDialog() == true)
            {
                menoSuboru = saveDlg.FileName.Remove(saveDlg.FileName.Length - 2, 2);
                File.WriteAllText(menoSuboru + ".c", program);
                SaveData(par, menoSuboru + "Arr" + ".dat");
                SaveData(matOrigin, menoSuboru + "Dic" + ".dat");
                isSaved = true;
                this.Title = menoSuboru;
            }
        }

        // Otvorit projekt
        private void menuLoad_Click(object sender, RoutedEventArgs e)
        {
            if (isSaved == true)
            {
                Microsoft.Win32.OpenFileDialog openDlg = new Microsoft.Win32.OpenFileDialog();
                openDlg.DefaultExt = ".c";
                openDlg.Filter = "Dokument C (.c)|*.c";
                if (openDlg.ShowDialog() == true)
                {
                    menoSuboru = openDlg.FileName.Remove(openDlg.FileName.Length - 2, 2);
                    LoadData2DArray(menoSuboru + "Arr" + ".dat");
                    LoadDataDict(menoSuboru + "Dic" + ".dat");
                    LoadBitmaps();
                    this.Title = menoSuboru;                    
                }
            }
            else
            {
                WPFMessageBoxResult result = WPFMessageBox.Show("Neuložené...", "Neboli uložené zmeny!\nPokračovať ?", WPFMessageBoxButtons.YesNo, WPFMessageBoxImage.Alert);
                if (result == WPFMessageBoxResult.Yes)
                {
                    Microsoft.Win32.OpenFileDialog openDlg = new Microsoft.Win32.OpenFileDialog();
                    openDlg.DefaultExt = ".c";
                    openDlg.Filter = "Dokument C (.c)|*.c";
                    if (openDlg.ShowDialog() == true)
                    {
                        menoSuboru = openDlg.FileName.Remove(openDlg.FileName.Length - 2, 2);
                        LoadData2DArray(menoSuboru + "Arr" + ".dat");
                        LoadDataDict(menoSuboru + "Dic" + ".dat");
                        LoadBitmaps();
                        isSaved = true;
                        this.Title = menoSuboru;
                    }
                }
            }
        }

        // About...
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            WPFMessageBox wpf = new WPFMessageBox();                        
            WPFMessageBox.Show("O programe...", "Autor: Ivo Hrádek\nEmail: ivohradek@gmail.com\n\nVerzia: 1.0.0", WPFMessageBoxButtons.OK, WPFMessageBoxImage.Information);                        
        }

        // Zatvorenie okna        
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (isSaved != true)
            {
                WPFMessageBoxResult result = WPFMessageBox.Show("Neuložené...", "Neboli uložené zmeny!\nChcete naozaj ukončiť ?", WPFMessageBoxButtons.YesNo, WPFMessageBoxImage.Alert);
                if (result == WPFMessageBoxResult.No)
                {
                    e.Cancel = true;
                }
                else
                {
                    e.Cancel = false;
                    if (seriovyPort.IsOpen)
                    {
                        seriovyPort.DataReceived -= new System.IO.Ports.SerialDataReceivedEventHandler(Prijem);
                        seriovyPort.Close();
                    }
                }
            }            
        }
        private void Window_Closing(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion
    }
}
