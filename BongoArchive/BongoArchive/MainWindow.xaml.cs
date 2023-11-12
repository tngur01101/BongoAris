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
using Winforms = System.Windows.Forms;

namespace BongoArchive
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

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr LParam);

        const int WH_KEYBOARD_LL = 13;
        const int WM_KEYDOWN = 0x100;

        private IntPtr hook = IntPtr.Zero;
        private static LowLevelKeyboardProc _hook;

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

            //Imageinit
            SetCharactor(0);

            //TimeSetting
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += TimeFunction;
            timer.Start();
            
            //TrayIcon
            Winforms.NotifyIcon notifyIcon = new Winforms.NotifyIcon();
            notifyIcon.Icon = Properties.Resources.aris;
            notifyIcon.Visible = true;
            notifyIcon.Text = "빛이여";

            Winforms.ContextMenuStrip menuStrip = new Winforms.ContextMenuStrip();
            Winforms.ToolStripMenuItem item0=new Winforms.ToolStripMenuItem();
            item0.Text = "종료";
            item0.Click += delegate (object? click, EventArgs e) { this.Close(); };
            menuStrip.Items.Add(item0);
            Winforms.ToolStripMenuItem item1 = new Winforms.ToolStripMenuItem();
            item1.Text = "BASIC";
            item1.Click += delegate (object? click, EventArgs e) { SetCharactor(0); };
            menuStrip.Items.Add(item1);
            Winforms.ToolStripMenuItem item2 = new Winforms.ToolStripMenuItem();
            item2.Text = "ARIS";
            item2.Click += delegate (object? click, EventArgs e) { SetCharactor(1); };
            menuStrip.Items.Add(item2);
            Winforms.ToolStripMenuItem item3 = new Winforms.ToolStripMenuItem();
            item3.Text = "KAYOKO";
            item3.Click += delegate (object? click, EventArgs e) { SetCharactor(2); };
            menuStrip.Items.Add(item3);



            notifyIcon.ContextMenuStrip = menuStrip;

            this.ShowInTaskbar = false;

        }

        private void SetCharactor(int num)
        {
            //0 = basic
            //1 = aris
            //2 = kayoko
            if (num == 0)
            {
                catBitmapImage[0] = new BitmapImage(new Uri("uu.png", UriKind.RelativeOrAbsolute));
                catBitmapImage[1] = new BitmapImage(new Uri("ud.png", UriKind.RelativeOrAbsolute));
                catBitmapImage[2] = new BitmapImage(new Uri("du.png", UriKind.RelativeOrAbsolute));
                catBitmapImage[3] = new BitmapImage(new Uri("dd.png", UriKind.RelativeOrAbsolute));
            }
            else if (num == 1)
            {
                catBitmapImage[0] = new BitmapImage(new Uri("auu.png", UriKind.RelativeOrAbsolute));
                catBitmapImage[1] = new BitmapImage(new Uri("aud.png", UriKind.RelativeOrAbsolute));
                catBitmapImage[2] = new BitmapImage(new Uri("adu.png", UriKind.RelativeOrAbsolute));
                catBitmapImage[3] = new BitmapImage(new Uri("add.png", UriKind.RelativeOrAbsolute));
            }
            else if (num == 2)
            {
                catBitmapImage[0] = new BitmapImage(new Uri("kayokouu.png", UriKind.RelativeOrAbsolute));
                catBitmapImage[1] = new BitmapImage(new Uri("kayokoud.png", UriKind.RelativeOrAbsolute));
                catBitmapImage[2] = new BitmapImage(new Uri("kayokodu.png", UriKind.RelativeOrAbsolute));
                catBitmapImage[3] = new BitmapImage(new Uri("kayokodd.png", UriKind.RelativeOrAbsolute));
            }

            ChangeImageSel(0);
        }


        private IntPtr HookProc(int code, IntPtr wParam, IntPtr lParam)//키입력 들어오면 작동하는 친구
        {
            if (code >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                byte vkCode = (byte)Marshal.ReadInt32(lParam);
                
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
