using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
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
using System.Windows.Threading;

namespace BongoCat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc calback, IntPtr hInstance, uint threadid);

        [DllImport("user32.dll")]
        static extern bool UnhookWindowsHookEx(IntPtr hInstance);

        [DllImport("user32.dll")]
        static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, int wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("user32.dll")]
        static extern void keybd_event(uint vk, uint scan, uint falgs, uint extraInfo);//( 가상 키, 스캔 코드, DOWN(0) or UP(2), 키보드타입); 일반키보드는 타입을 0으로

        [DllImport("user32.dll")]
        static extern uint MapVirtualKey(int wCode, int wMapType);//( 변환 할 키, 변환 동작 설정);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr LParam);

        const int WH_KEYBOARD_LL = 13;
        const int WM_KEYDOWN = 0x100;

        private IntPtr hook = IntPtr.Zero;
        private static LowLevelKeyboardProc _hook;

        const int LShift = 160;
        const int LCtrl = 162;
        const int TAB = 9;
        const int CapsLock = 20;
        const int Enter = 13;
        const int Space = 32;
        const int ChangeLanguage = 21;

        bool inputingFlag = false;


        int currentImageNum = 0;
        BitmapImage[] catBitmapImage = new BitmapImage[4];

        //timer
        int resetTimer = 0;
        int feverTimer = 0;
        public MainWindow()
        {
            InitializeComponent();

            //start Hooking your keyboardevent
            IntPtr hInstance = LoadLibrary("user32");
            _hook = HookProc;
            hook = SetWindowsHookEx(WH_KEYBOARD_LL, _hook, hInstance, 0);

            //unhooking method append closing event
            this.Closing += delegate (object? sender, System.ComponentModel.CancelEventArgs e)
            {
                //e.Cancel = true;종료 막는 동작
                UnhookWindowsHookEx(hook);
            };

            //Imagebookmark
            catBitmapImage[0] = new BitmapImage(new Uri("auu.png", UriKind.RelativeOrAbsolute));
            catBitmapImage[1] = new BitmapImage(new Uri("aud.png", UriKind.RelativeOrAbsolute));
            catBitmapImage[2] = new BitmapImage(new Uri("adu.png", UriKind.RelativeOrAbsolute));
            catBitmapImage[3] = new BitmapImage(new Uri("add.png", UriKind.RelativeOrAbsolute));

            //TimeSetting
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += TimeFunction;
            timer.Start();
        }

        private void KeyInput(char c)
        {
            byte result = 0;
            bool lShiftUse = false;
            if (c == '~') { result = 192; lShiftUse = true; }
            else if (c == '!') { result = 49; lShiftUse = true; }
            else if (c == '@') { result = 50; lShiftUse = true; }
            else if (c == '#') { result = 51; lShiftUse = true; }
            else if (c == '$') { result = 52; lShiftUse = true; }
            else if (c == '%') { result = 53; lShiftUse = true; }
            else if (c == '^') { result = 54; lShiftUse = true; }
            else if (c == '&') { result = 55; lShiftUse = true; }
            else if (c == '*') { result = 56; lShiftUse = true; }
            else if (c == '(') { result = 57; lShiftUse = true; }
            else if (c == ')') { result = 58; lShiftUse = true; }
            else if (c == '_') { result = 189; lShiftUse = true; }
            else if (c == '+') { result = 187; lShiftUse = true; }
            else if (c == '{') { result = 219; lShiftUse = true; }
            else if (c == '}') { result = 221; lShiftUse = true; }
            else if (c == ':') { result = 186; lShiftUse = true; }
            else if (c == '\"') { result = 222; lShiftUse = true; }
            else if (c == '<') { result = 188; lShiftUse = true; }
            else if (c == '>') { result = 190; lShiftUse = true; }
            else if (c == '?') { result = 191; lShiftUse = true; }
            else result = (byte)GetVkCode(c);

            if (lShiftUse) keybd_event(LShift, 0, 0, 0);
            keybd_event(result, 0, 0, 0);
            keybd_event(result, 0, 2, 0);
            if (lShiftUse) keybd_event(LShift, 0, 2, 0);
        }

        private int GetVkCode(char a)
        {
            Debug.WriteLine((int)a);
            if ((int)a >= 48 && (int)a <= 57) return (int)a;
            else if ((int)a >= 65 && (int)a <= 90) return (int)a;
            else if ((int)a >= 97 && (int)a <= 122) return (int)a - 32;
            else if (a.Equals('`')) return 192;
            else if (a.Equals('-')) return 189;
            else if (a.Equals('=')) return 187;
            else if (a.Equals('[')) return 219;
            else if (a.Equals(']')) return 221;
            else if (a.Equals(';')) return 186;
            else if (a.Equals('\'')) return 222;
            else if (a.Equals(',')) return 188;
            else if (a.Equals('.')) return 190;
            else if (a.Equals('/')) return 191;
            return 0;
        }

        private IntPtr HookProc(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                inputingFlag = true;
                byte vkCode = (byte)Marshal.ReadInt32(lParam);
                
                inputingFlag = false;
                ChangeImage();

                //return (IntPtr)1; 키 씹기.
            }

            return CallNextHookEx(hook, code, (int)wParam, lParam);
        }

        private void ChangeImage()
        {
            if (feverTimer < 30)
            {
                if (currentImageNum == 2) currentImageNum = 1;
                else currentImageNum = 2;
            }
            else
            {
                if (currentImageNum == 0) currentImageNum = 3;
                else currentImageNum = 0;
            }

            catImage.Source = catBitmapImage[currentImageNum];


            if (feverTimer < 50) feverTimer++;
            resetTimer = 10;
        }
        private void ChangeImageSel(int num)
        {
            if (num >= 0 && num < 4)
            {
                currentImageNum = num;
                catImage.Source = catBitmapImage[num];
            }

        }

        private void TimeFunction(object? sender, EventArgs e)
        {
            if (resetTimer > 0)
            {
                resetTimer--;
                if(resetTimer == 0)
                {
                    ChangeImageSel(0);
                }
            }
            if(feverTimer>0)
            {
                feverTimer--;
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
