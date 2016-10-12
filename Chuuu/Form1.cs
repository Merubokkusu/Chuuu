using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using NAudio.Wave;
using System.IO;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using System.Xml;
using System.Xml.Linq;

namespace Chuuu
{

    public partial class Form1 : Form
    {
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);
        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        enum KeyModifier
        {
            None = 0,
            Alt = 1,
            Control = 2,
            Shift = 4,
            WinKey = 8
        }
        public class MyItem
        {
            public int ID;
            public string displayName;
            public string name;
            public string slug;
            public string imageUrl;
        }
        int[] IDnumbers;
        List<int> idNumber = new List<int>();
        int HighestNumber;

        String audioFile;

        AudioFileReader reader;
        String AudioSND = @"C:\Users\Liam\Desktop\Music\Friends Are There.mp3";
        bool CanPlaySound = true;
       private DirectSoundOut SNDout;
        public Form1() {
            InitializeComponent();
            regKeys();
            CanPlaySound = true;

        }
        void NumCount() {
            HighestNumber = idNumber.Max();
            Console.WriteLine("This is the highest number " + HighestNumber);
        }

      

        private void button1_Click(object sender, EventArgs e) {

            OpenFileDialog myDialog = new OpenFileDialog();
            myDialog.Filter = "Sound Files(*.MP3;*.WAV;*.WMA;*.OGG;*.MID;)|*.MP3;*.WAV;*.WMA;*.OGG;*.MID|All files (*.*)|*.*";
            myDialog.CheckFileExists = true;
            myDialog.Multiselect = false;

            if (myDialog.ShowDialog() == DialogResult.OK) {
                audioFile = myDialog.FileName;

                if (File.Exists(audioFile)) {
                    Console.WriteLine(audioFile);
                    File.Copy(audioFile,@"data\snd\" + Path.GetFileName(audioFile),true);
                    JsonCreator();
                    }

                
            
        }

    }

        private void button2_Click(object sender, EventArgs e) {
            if (SNDout != null) {
                SNDout.Stop();
            }
        }


       

        void regKeys() {
            string xmlElementName;
            XDocument doc = XDocument.Load("test.xml");

            if (new FileInfo("test.xml").Length > 0) {



                foreach (var coordinate in doc.Descendants("test")) {

                    string idString = coordinate.Element("ID").Value;
                    string SoundFile = coordinate.Element("SoundFile").Value;
                    string KeyHot = coordinate.Element("KeyHot").Value;
                    int ID = Int32.Parse(idString);
                    idNumber.Add(ID);

                    Console.WriteLine(SoundFile);
                    Console.WriteLine(KeyHot);
                    Console.WriteLine("Id Number "+ID);
                    // do whatever you want to do with those items of information now
                }

                 // UnregisterHotKey(this.Handle, (int)item.ID);
                 //   RegisterHotKey(this.Handle, (int)item.ID, (int)KeyModifier.None, (int)Keys.NoName);
                }
               // NumCount(); 
            //RegisterHotKey(this.Handle, 2, (int)KeyModifier.None, (int)Keys.F2);
            }

            
            
           
        

        void playSnd() {
            var jsonSource = File.ReadAllText("person.json");
            if (new FileInfo("person.json").Length > 0) {

                dynamic dynJson = JsonConvert.DeserializeObject(jsonSource);
                foreach (var item in dynJson) {
                    item.SoundFile = audioFile;

                }
                reader = new AudioFileReader(AudioSND);
                SNDout = new DirectSoundOut(directSoundOutSettingsPanel1.SelectedDevice); // or WaveOutEvent()
                SNDout.Init(reader);
                SNDout.Play();
            }
        }
        protected override void WndProc(ref Message m) {
            if(CanPlaySound == true) { 
            if (m.Msg == 0x0312 && m.WParam.ToInt32() == 1) {
                playSnd();
            }

            }//Can play sound end.
            base.WndProc(ref m);
        }

        private void Form1_Load(object sender, EventArgs e) {

        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
        }
        void JsonCreator() {

           
                string soundPath =  @"data\snd\" + Path.GetFileName(audioFile);
                soundPath = soundPath.Replace(@"\\", @"\");
                Console.WriteLine(soundPath);

            using (FileStream fs = File.Open("test.xml", FileMode.Append,FileAccess.Write))
            using (StreamWriter sw = new StreamWriter(fs))
            using (XmlWriter writer = XmlWriter.Create(fs)){
                writer.WriteStartDocument();

                if (new FileInfo("test.xml").Length < 0) {

                    }


                writer.WriteStartElement(Path.GetFileNameWithoutExtension(audioFile));
                if (new FileInfo("test.xml").Length < 0) {
                    writer.WriteElementString("ID", "1");
                }
                else {
                    writer.WriteElementString("ID", HighestNumber + 1.ToString());
                }
                writer.WriteElementString("SoundFile", soundPath);
                writer.WriteElementString("KeyHot", "null");

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
            regKeys();

        }

        private void button3_Click(object sender, EventArgs e) {
            JsonCreator();
        }
        static void WriteTop(string filename) {
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
           Console.WriteLine(e.KeyCode.ToString());
        }
    }

}

