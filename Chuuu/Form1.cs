using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace Chuuu {

    public partial class Form1 :Form {

        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private enum KeyModifier {
            None = 0x0000,
            Alt = 0x0001,
            Control = 0x0002,
            Shift = 0x0004,
            WinKey = 0x0008
        }

        public const int WM_HOTKEY = 0x0312;
        public const int MOD_NOREPEAT = 0x4000;

        private List<int> idNumber = new List<int>();
        private int HighestNumber;

        private List<string> AudioList = new List<string>();

        private String audioFile;

        private AudioFileReader reader;

        private bool CanPlaySound = true;

        private DirectSoundOut SNDout;

        private bool CanPlay = true;
        private bool NotTyping = true;

        //Keys
        public string TypeModeKey;

        public string TalkBackKey;
        //-====-

        private NAudio.Wave.WaveIn sourceStream = null;
        private NAudio.Wave.DirectSoundOut waveOut = null;

        public Form1() {
            InitializeComponent();
            loadINI();
            regKeys();

            pictureBox1.AllowDrop = true;
            CanPlaySound = true;
            if(!Directory.Exists(@"Data\snd\")) {
                Directory.CreateDirectory(@"Data\snd\");
                File.Create(@"Data\iwannadie.( ͡° ͜ʖ ͡°)");
            }

            Keys keyPause;
            Enum.TryParse(TypeModeKey, out keyPause);
            Keys keyTalkBack;
            Enum.TryParse(TalkBackKey, out keyTalkBack);
            RegisterHotKey(this.Handle, 9999, (int)KeyModifier.None, (int)keyPause);
            RegisterHotKey(this.Handle, 9998, (int)KeyModifier.None, (int)keyTalkBack);
        }

        private void loadINI() {
            if(File.Exists("Settings.ini") == false) {
                var MyIni = new IniFile("Settings.ini");
                MyIni.Write("KeyList", "https://ghostbin.com/paste/kkr4f", "Keys");
                MyIni.Write("TypeModeKey", "Pause", "Keys");
                MyIni.Write("TalkBackKey", "PageDown", "Keys");
                TypeModeKey = MyIni.Read("TypeModeKey");
                TalkBackKey = MyIni.Read("TalkBackKey");
            }
            else {
                var MyIni = new IniFile("Settings.ini");
                TypeModeKey = MyIni.Read("TypeModeKey", "Keys");
                TalkBackKey = MyIni.Read("TalkBackKey", "Keys");
            }
        }

        private void button1_Click(object sender, EventArgs e) {
        }

        private void unregKeys() {
            if(File.Exists("chuuu.xml") == true) {
                XDocument doc = XDocument.Load("chuuu.xml");

                if(new FileInfo("chuuu.xml").Length > 0) {
                    foreach(var coordinate in doc.Descendants("Key")) {
                        string idString = coordinate.Element("ID").Value;

                        UnregisterHotKey(this.Handle, Convert.ToInt32(idString));
                    }
                }
            }
        }

        private void regKeys() {
            listSnd.Items.Clear();
            if(File.Exists("chuuu.xml") == true) {
                XDocument doc = XDocument.Load("chuuu.xml");

                if(new FileInfo("chuuu.xml").Length > 0) {
                    foreach(var coordinate in doc.Descendants("Key")) {
                        string idString = coordinate.Element("ID").Value;
                        string SoundFile = coordinate.Element("SoundFile").Value;
                        string KeyHot = coordinate.Element("KeyHot").Value;
                        string KeyHotExtra = coordinate.Element("KeyHotExtra").Value;
                        int ID = Int32.Parse(idString);
                        idNumber.Add(ID);
                        AudioList.Add(SoundFile);

                        UnregisterHotKey(this.Handle, Convert.ToInt32(idString));

                        Keys keyExtra;
                        Enum.TryParse(KeyHotExtra, out keyExtra);
                        Keys keyMain;
                        Enum.TryParse(KeyHot, out keyMain);

                        if(KeyHotExtra == "Shift") {
                            RegisterHotKey(this.Handle, Convert.ToInt32(idString), (int)KeyModifier.Shift, (int)keyMain);
                            Console.Write("Shift");
                        }
                        if(KeyHotExtra == "Alt") {
                            RegisterHotKey(this.Handle, Convert.ToInt32(idString), (int)KeyModifier.Alt, (int)keyMain);
                            Console.Write("Alt");
                        }
                        if(KeyHotExtra == "Control") {
                            RegisterHotKey(this.Handle, Convert.ToInt32(idString), (int)KeyModifier.Control, (int)keyMain);
                            Console.Write("Control");
                        }
                        if(KeyHotExtra == "None") {
                            RegisterHotKey(this.Handle, Convert.ToInt32(idString), (int)KeyModifier.None, (int)keyMain);
                            Console.Write("None");
                        }
                        Console.WriteLine((int)keyMain);

                        ListViewItem listSnd = new ListViewItem();

                        listSnd.Text = Path.GetFileNameWithoutExtension(SoundFile);
                        listSnd.SubItems.Add(KeyHot + "+" + keyExtra);
                        listSnd.SubItems.Add(ID.ToString());
                        this.listSnd.Items.Add(listSnd);
                    }
                }
                NumCount();
            }
        }

        private void playSnd(string AudioSND) {
            reader = new AudioFileReader(AudioSND);
            SNDout = new DirectSoundOut(directSoundOutSettingsPanel1.SelectedDevice);
            SNDout.Init(reader);
            SNDout.Play();
        }

        protected override void WndProc(ref Message m) {
            base.WndProc(ref m);
            var sndArray = AudioList.ToArray();
            if(CanPlaySound == true) {
                if(m.Msg == WM_HOTKEY) {
                    Console.WriteLine(m.Msg);
                    //SendKeys.Send(m.HWnd);
                    if((int)m.WParam == 9999) {
                        if(NotTyping == false) {
                            regKeys();
                            label1.Text = "Ready..";
                            NotTyping = true;
                        }
                        else {
                            unregKeys();
                            label1.Text = "Typing..";
                            NotTyping = false;
                        }
                    }

                    if(CanPlay == true && (int)m.WParam < 8000) {
                        if(NotTyping == true) {
                            playSnd(sndArray[(int)m.WParam - 1]);
                            CanPlay = false;
                        }
                    }
                    else if((int)m.WParam != 9999) {
                        SNDout.Stop();
                        CanPlay = true;
                    }
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e) {
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            if(File.Exists("chuuu.xml") == true) {
                XDocument doc = XDocument.Load("chuuu.xml");

                if(new FileInfo("chuuu.xml").Length > 0) {
                    foreach(var coordinate in doc.Descendants("Key")) {
                        string idString = coordinate.Element("ID").Value;

                        UnregisterHotKey(this.Handle, Convert.ToInt32(idString));
                    }
                }
            }
        }

        private void xmlCreator() {
            string hotKey_Main = "";
            string hotKey_Extra = "";
            //Locals
            Hotkey_Editor hkedit = new Hotkey_Editor();
            hkedit.StartPosition = FormStartPosition.CenterParent;
            DialogResult dr = hkedit.ShowDialog(this);

            if(dr == DialogResult.OK) {
                hotKey_Main = hkedit.comboBox2.SelectedItem.ToString();
                hotKey_Extra = hkedit.comboBox1.SelectedItem.ToString();
            }
            hkedit.Dispose();
            hotKey_Main = Regex.Replace(hotKey_Main, @"\s+", "");
            hotKey_Extra = Regex.Replace(hotKey_Extra, @"\s+", "");

            HighestNumber += 1;
            var title = Regex.Replace(Path.GetFileNameWithoutExtension(audioFile), @"\s+", "");
            string soundPath = @"data\snd\" + Path.GetFileName(audioFile);
            soundPath = soundPath.Replace(@"\\", @"\");
            if(File.Exists("chuuu.xml") == false) {
                XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
                xmlWriterSettings.Indent = true;
                xmlWriterSettings.NewLineOnAttributes = true;

                using(XmlWriter xmlWriter = XmlWriter.Create("chuuu.xml", xmlWriterSettings)) {
                    xmlWriter.WriteStartDocument();
                    xmlWriter.WriteStartElement("Chuuu");

                    xmlWriter.WriteStartElement("Key");
                    xmlWriter.WriteElementString("ID", "1");
                    xmlWriter.WriteElementString("SoundFile", soundPath);
                    xmlWriter.WriteElementString("KeyHot", hotKey_Main);
                    xmlWriter.WriteElementString("KeyHotExtra", hotKey_Extra);
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndDocument();
                    xmlWriter.Flush();
                    xmlWriter.Close();
                }
            }
            else {
                XDocument xDocument = XDocument.Load("chuuu.xml");
                XElement root = xDocument.Element("Chuuu");
                IEnumerable<XElement> rows = root.Descendants("Key");
                XElement firstRow = rows.Last();
                firstRow.AddAfterSelf(
                   new XElement("Key",
                   new XElement("ID", HighestNumber),
                   new XElement("SoundFile", soundPath),
                   new XElement("KeyHot", hotKey_Main),
                   new XElement("KeyHotExtra", hotKey_Extra)));
                xDocument.Save("chuuu.xml");
            }
            regKeys();
        }

        private void button3_Click(object sender, EventArgs e) {
            xmlCreator();
        }

        private static void WriteTop(string filename) {
            string tempfile = Path.GetTempFileName();
            using(var writer = new StreamWriter(tempfile))
            using(var reader = new StreamReader(filename)) {
                writer.WriteLine("[");
                while(!reader.EndOfStream)
                    writer.WriteLine(reader.ReadLine());
            }
            File.Copy(tempfile, filename, true);
        }

        private void Form1_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e) {
        }

        private void addSNDToolStripMenuItem_Click(object sender, EventArgs e) {
            OpenFileDialog myDialog = new OpenFileDialog();
            myDialog.Filter = "Sound Files(*.MP3;*.WAV;*.WMA;*.OGG;*.MID;)|*.MP3;*.WAV;*.WMA;*.OGG;*.MID|All files (*.*)|*.*";
            myDialog.CheckFileExists = true;
            myDialog.Multiselect = false;

            if(myDialog.ShowDialog() == DialogResult.OK) {
                audioFile = myDialog.FileName;

                if(File.Exists(audioFile)) {
                    try {
                        if(!File.Exists(@"data\snd\" + Path.GetFileName(audioFile))) {
                            Console.WriteLine(audioFile);
                            File.Copy(audioFile, @"data\snd\" + Path.GetFileName(audioFile), true);
                            xmlCreator();
                        }
                        else {
                            xmlCreator();
                            Console.WriteLine("File Already In Folder");
                        }
                    }
                    catch(System.IO.IOException) {
                        MessageBox.Show("The File Is Currlenty Open In Another Program.");
                    }
                }
            }
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e) {
            using(Pen p = new Pen(Color.DarkRed)) {
                p.DashStyle = DashStyle.Dot;

                e.Graphics.DrawRectangle(p, 0, 0, 234, 140);
            }
        }

        private void gitHubToolStripMenuItem_Click(object sender, EventArgs e) {
            System.Diagnostics.Process.Start("https://github.com/Merubokkusu/Chuuu");
        }

        private void merubokkusuToolStripMenuItem_Click(object sender, EventArgs e) {
            System.Diagnostics.Process.Start("https://merubokkusu.ga");
        }

        private void pictureBox1_DragEnter(object sender, DragEventArgs e) {
            if(e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void pictureBox1_DragDrop(object sender, DragEventArgs e) {
            string[] audioFileList = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            audioFile = audioFileList[0];
            MessageBox.Show(audioFile);
            if(File.Exists(audioFile)) {
                try {
                    if(!File.Exists(@"data\snd\" + Path.GetFileName(audioFile))) {
                        Console.WriteLine(audioFile);
                        File.Copy(audioFile, @"data\snd\" + Path.GetFileName(audioFile), true);
                        xmlCreator();
                    }
                    else {
                        xmlCreator();
                        Console.WriteLine("File Already In Folder");
                    }
                }
                catch(System.IO.IOException) {
                    MessageBox.Show("The File Is Currlenty Open In Another Program.");
                }
            }
        }

        private void button1_Click_1(object sender, EventArgs e) {
            if(listSnd.SelectedItems.Count > 0) {
                ListViewItem sndItem = listSnd.SelectedItems[0];
                string ID = sndItem.SubItems[2].Text;
                string file = sndItem.SubItems[0].Text;
                //Read and remove data in XML
                XElement xEle = XElement.Load("chuuu.xml");
                var qry = from element in xEle.Descendants()
                          where (string)element.Element("ID") == ID
                          select element;
                if(qry.Count() > 0)
                    qry.First().Remove();
                xEle.Save("chuuu.xml");
                if(idNumber.Max() == 1) {
                    if(File.Exists("chuuu.xml")) {
                        File.Delete("chuuu.xml");
                    }
                }
                regKeys();
                NumCount();
            }
        }

        private void NumCount() {
            try {
                if(idNumber.FirstOrDefault() == 0) {
                    if(File.Exists("chuuu.xml")) {
                        File.Delete("chuuu.xml");
                    }
                }
                else {
                    HighestNumber = idNumber.Max();
                }
            }
            catch(System.InvalidOperationException) {
                MessageBox.Show("MEM");
            }
        }
    }
}