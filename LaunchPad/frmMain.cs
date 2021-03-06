using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace LaunchPad
{
  public partial class frmMain : Form
  {
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
    [DllImport("kernel32", SetLastError = true)]
    public static extern short GlobalAddAtom(string lpString);

    private struct TileApp
    {
      public string Title;
      public Image  Icon;
      public string Executable;
      public string Arguments;
      public string WorkingDir;
    }

    private int HotkeyID;
    private List<TileApp> tileApps = new List<TileApp>();
    private int tileSize    = 96; // <-- Application Icon Size
    private int tileSpacing = 96; // <-- Icon Spacing
    private int tilePadSize, nTilesX, nTilesY, offsetX, offsetY;

    public frmMain()
    {
      InitializeComponent();
    }

    private void frmMain_Load(object sender, EventArgs e)
    {
      string atomName = Thread.CurrentThread.ManagedThreadId.ToString("X8") + this.GetType().FullName;
      HotkeyID = GlobalAddAtom(atomName);
      RegisterHotKey(this.Handle, HotkeyID, 0, (int)Keys.F4);

      Width  = Screen.PrimaryScreen.Bounds.Width;
      Height = Screen.PrimaryScreen.Bounds.Height;
      tilePadSize = tileSize + tileSpacing;
      nTilesX = Width / tilePadSize;
      nTilesY = Height / tilePadSize;
      offsetX = (Width + tileSpacing - (nTilesX * tilePadSize)) >> 1;
      offsetY = (Height + tileSpacing - (nTilesY * tilePadSize)) >> 1;
      LoadTiles();
    }

    private void frmMain_Deactivate(object sender, EventArgs e)
    {
      Hide();
    }

    private void frmMain_KeyDown(object sender, KeyEventArgs e)
    {
      //Escape -> hide LaunchPad
      //R      -> reload configuration
      switch (e.KeyCode)
      {
        case Keys.Escape: frmMain_Deactivate(sender, e); break;
        case Keys.R:
          tileApps.Clear();
          LoadTiles();
          Hide();
          SetBackground();
          Show();
          break;
      }
    }

    private void frmMain_MouseClick(object sender, MouseEventArgs e)
    {
      Hide();

      //blank areas...
      int tileXOff = (e.X - offsetX) % tilePadSize; if(tileXOff > tileSize     ) return;
      int tileYOff = (e.Y - offsetY) % tilePadSize; if(tileYOff > (tileSize+16)) return;
      if((e.X < offsetX) || (e.X > (Width  - offsetX))) return;
      if((e.Y < offsetY) || (e.Y > (Height - offsetY))) return;
      
      int tileX = ((e.X - offsetX) - tileXOff) / tilePadSize;
      int tileY = ((e.Y - offsetY) - tileYOff) / tilePadSize;
      int index = (tileY * nTilesX) + tileX;
      if((index < 0) || (index >= tileApps.Count)) return; //who knows how this would happen...

      //launch selected tile
      TileApp ta = tileApps[index];
      System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
      psi.FileName         = ta.Executable;
      psi.Arguments        = ta.Arguments;
      psi.WorkingDirectory = ta.WorkingDir;
      System.Diagnostics.Process.Start(psi);
    }

    private void SetBackground()
    {
      int scrnW = Screen.PrimaryScreen.Bounds.Width, scrnH = Screen.PrimaryScreen.Bounds.Height;
      Bitmap scrnB = new Bitmap(scrnW, scrnH);
      Graphics gfx = Graphics.FromImage(scrnB);
      gfx.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, 0, 0, scrnB.Size, CopyPixelOperation.SourceCopy);
      Brush scrnF = new SolidBrush(Color.FromArgb(0xBB, 0x10, 0x10, 0x10)); // <-- Background Darkness
      gfx.FillRectangle(scrnF, 0, 0, scrnW, scrnH);

      Top = Left = 0;
      Width = scrnW;
      Height = scrnH;
      tilePadSize = tileSize + tileSpacing;
      nTilesX = Width / tilePadSize;
      nTilesY = Height / tilePadSize;
      offsetX = (Width + tileSpacing - (nTilesX * tilePadSize)) >> 1;
      offsetY = (Height + tileSpacing - (nTilesY * tilePadSize)) >> 1;

      StringFormat tileFmt  = new StringFormat();
      tileFmt.LineAlignment = StringAlignment.Center;
      tileFmt.Alignment     = StringAlignment.Center;
      int tileCount = tileApps.Count;
      Font tileFont = new Font("Helvetica", 9.0f, FontStyle.Bold);
      Brush tileBrush = new SolidBrush(Color.White);
      for(int index=0; index<tileCount; index++)
      {
        int x = index % nTilesX;
        int y = (index - x) / nTilesX;
        int drawLeft = offsetX + (x * tilePadSize);
        int drawTop  = offsetY + (y * tilePadSize);
        gfx.DrawImage(tileApps[index].Icon, drawLeft,drawTop, tileSize,tileSize);
        Rectangle rc = new Rectangle(drawLeft, drawTop + tileSize + 4, tileSize, tileFont.Height);
        gfx.DrawString(tileApps[index].Title, tileFont, tileBrush, rc, tileFmt);
      }

      BackgroundImage = scrnB;

      gfx.Dispose();
      scrnF.Dispose();
      tileFont.Dispose();
      tileBrush.Dispose();
      GC.Collect(); // <-- prevent insane memory growth...
    }

    private void LoadTiles()
    {
      //load text file from "\Users\{username}\LaunchPad\Apps.text"
      //i also place LaunchPad.exe, and its icons, in \Users\{username}\LaunchPad (not required though)
      //Apps.text is a single-text-line-per-entry, pipe-separated file, of the format:
      //  Title|IconPath|ExecutablePath|ExecutableArguments|WorkingDirectory
      //Arguments and WorkingDir are optional, other values are required.
      string launchPadDir = Environment.GetEnvironmentVariable("USERPROFILE") + "\\LaunchPad\\";
      string[] tileData = File.ReadAllLines(launchPadDir + "Apps.text");
      for(var i = 0; i < tileData.Length; i++)
      {
        string curr = tileData[i].Trim();
        if(curr.Length > 0)
        {
          string[] components = curr.Split(new char[]{'|'});
          if(components.Length > 2)
          {
            TileApp newApp = new TileApp();
            newApp.Title      = components[0];
            newApp.Icon       = Image.FromFile(launchPadDir + components[1]);
            newApp.Executable = components[2];
            if(components.Length > 3) newApp.Arguments  = components[3];
            if(components.Length > 4) newApp.WorkingDir = components[4];
            tileApps.Add(newApp);
          }
        }
      }
    }

    private void Display()
    {
      if(Visible) { Hide(); return; }
      SetBackground();
      Show();
    }

    protected override void WndProc(ref Message m)
    {
      switch(m.Msg)
      {
        case 0x0312:
          if((short)m.WParam == HotkeyID) Display();
          break;
        default:
          base.WndProc(ref m);
          break;
      }
    }

  }
}
