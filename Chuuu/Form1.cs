using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace Chuuu {

    public partial class Form1 : Form {

        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private enum KeyModifier {
            None = 0,
            Alt = 1,
            Control = 2,
            Shift = 4,
            WinKey = 8
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

        public Form1() {
            InitializeComponent();
            regKeys();
            CanPlaySound = true;
            if (!Directory.Exists(@"Data\snd\")) {
                Directory.CreateDirectory(@"Data\snd\");
                File.Create(@"Data\iwannadie.( ͡° ͜ʖ ͡°)");
            }

            RegisterHotKey(this.Handle, 9999, (int)KeyModifier.None, (int)Keys.Pause);
        }

        private void NumCount() {
            HighestNumber = idNumber.Max();
            Console.WriteLine("This is the highest number " + HighestNumber);
        }

        private void button1_Click(object sender, EventArgs e) {
        }

        private void unregKeys() {
            if (File.Exists("chuuu.xml") == true) {
                XDocument doc = XDocument.Load("chuuu.xml");

                if (new FileInfo("chuuu.xml").Length > 0) {
                    foreach (var coordinate in doc.Descendants("Key")) {
                        string idString = coordinate.Element("ID").Value;

                        UnregisterHotKey(this.Handle, Convert.ToInt32(idString));
                    }
                }
            }
        }

        private void regKeys() {
            listBox1.Items.Clear();
            if (File.Exists("chuuu.xml") == true) {
                XDocument doc = XDocument.Load("chuuu.xml");

                if (new FileInfo("chuuu.xml").Length > 0) {
                    foreach (var coordinate in doc.Descendants("Key")) {
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

                        if (KeyHotExtra == "Shift") {
                            RegisterHotKey(this.Handle, Convert.ToInt32(idString), (int)KeyModifier.Shift, (int)keyMain);
                        }
                        if (KeyHotExtra == "Alt") {
                            RegisterHotKey(this.Handle, Convert.ToInt32(idString), (int)KeyModifier.Alt, (int)keyMain);
                        }
                        if (KeyHotExtra == "Control") {
                            RegisterHotKey(this.Handle, Convert.ToInt32(idString), (int)KeyModifier.Control, (int)keyMain);
                        }

                        listBox1.Items.Add("SND:" + Path.GetFileNameWithoutExtension(SoundFile) + " KEY:" + KeyHot + "+" + keyExtra);
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
            if (CanPlaySound == true) {
                if (m.Msg == WM_HOTKEY) {
                    Console.WriteLine(m.Msg);
                    //SendKeys.Send(m.HWnd);
                    if ((int)m.WParam == 9999) {
                        if (NotTyping == false) {
                            regKeys();
                            label1.Text = "READY";
                            NotTyping = true;
                        } else {
                            unregKeys();
                            label1.Text = "TYPING";
                            NotTyping = false;
                        }
                    }

                    if (CanPlay == true && (int)m.WParam != 9999) {
                        if (NotTyping == true) {
                            playSnd(sndArray[(int)m.WParam - 1]);
                            CanPlay = false;
                        }
                    } else if ((int)m.WParam != 9999) {
                        SNDout.Stop();
                        CanPlay = true;
                    }
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e) {
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            if (File.Exists("chuuu.xml") == true) {
                XDocument doc = XDocument.Load("chuuu.xml");

                if (new FileInfo("chuuu.xml").Length > 0) {
                    foreach (var coordinate in doc.Descendants("Key")) {
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

            if (dr == DialogResult.OK) {
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
            if (File.Exists("chuuu.xml") == false) {
                XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
                xmlWriterSettings.Indent = true;
                xmlWriterSettings.NewLineOnAttributes = true;

                using (XmlWriter xmlWriter = XmlWriter.Create("chuuu.xml", xmlWriterSettings)) {
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
            } else {
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
            using (var writer = new StreamWriter(tempfile))
            using (var reader = new StreamReader(filename)) {
                writer.WriteLine("[");
                while (!reader.EndOfStream)
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

            if (myDialog.ShowDialog() == DialogResult.OK) {
                audioFile = myDialog.FileName;

                if (File.Exists(audioFile)) {
                    try {
                        if (!File.Exists(@"data\snd\" + Path.GetFileName(audioFile))) {
                            Console.WriteLine(audioFile);
                            File.Copy(audioFile, @"data\snd\" + Path.GetFileName(audioFile), true);
                            xmlCreator();
                        } else {
                            xmlCreator();
                            Console.WriteLine("File Already In Folder");
                        }
                    }
                    catch (System.IO.IOException) {
                        MessageBox.Show("The File Is Currlenty Open In Another Program.");
                    }
                }
            }
        }
    }
}