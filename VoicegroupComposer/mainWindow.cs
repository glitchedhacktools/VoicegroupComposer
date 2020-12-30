using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VoicegroupComposer
{
    public partial class mainWindow : Form
    {
        // GLOBAL VARIABLES DEFINITION

        public string GlobalDefaultInstrument = "voice_square_1 60, 0, 0, 2, 0, 0, 15, 0";
        public string GlobalCryTablesEntry = ".include \"sound/cry_tables.inc\"";

        public List<string> GlobalInstrumentsList = new List<string>
        {
            "000 - Acoustic Grand Piano",
            "001 - Bright Acoustic Piano",
            "002 - Electric Grand Piano",
            "003 - Honky-tonk Piano",
            "004 - Electric Piano 1",
            "005 - Electric Piano 2",
            "006 - Harpsichord",
            "007 - Clavi",
            "008 - Celesta",
            "009 - Glockenspiel",
            "010 - Music Box",
            "011 - Vibraphone",
            "012 - Marimba",
            "013 - Xylophone",
            "014 - Tubular Bells",
            "015 - Dulcimer",
            "016 - Drawbar Organ",
            "017 - Percussive Organ",
            "018 - Rock Organ",
            "019 - Church Organ",
            "020 - Reed Organ",
            "021 - Accordion",
            "022 - Harmonica",
            "023 - Tango Accordion",
            "024 - Acoustic Guitar (nylon)",
            "025 - Acoustic Guitar (steel)",
            "026 - Electric Guitar (jazz)",
            "027 - Electric Guitar (clean)",
            "028 - Electric Guitar (muted)",
            "029 - Overdriven Guitar",
            "030 - Distortion Guitar",
            "031 - Guitar Harmonics",
            "032 - Acoustic Bass",
            "033 - Electric Bass (finger)",
            "034 - Electric Bass (pick)",
            "035 - Fretless Bass",
            "036 - Slap Bass 1",
            "037 - Slap Bass 2",
            "038 - Synth Bass 1",
            "039 - Synth Bass 2",
            "040 - Violin",
            "041 - Viola",
            "042 - Cello",
            "043 - Contrabass",
            "044 - Tremolo Strings",
            "045 - Pizzicato Strings",
            "046 - Orchestral Harp",
            "047 - Timpani",
            "048 - String Ensemble 1",
            "049 - String Ensemble 2",
            "050 - Synth Strings 1",
            "051 - Synth Strings 2",
            "052 - Choir Aahs",
            "053 - Voice Oohs",
            "054 - Synth Voice",
            "055 - Orchestra Hit",
            "056 - Trumpet",
            "057 - Trombone",
            "058 - Tuba",
            "059 - Muted Trumpet",
            "060 - French Horn",
            "061 - Brass Section",
            "062 - Synth Brass 1",
            "063 - Synth Brass 2",
            "064 - Soprano Sax",
            "065 - Alto Sax",
            "066 - Tenor Sax",
            "067 - Baritone Sax",
            "068 - Oboe",
            "069 - English Horn",
            "070 - Bassoon",
            "071 - Clarinet",
            "072 - Piccolo",
            "073 - Flute",
            "074 - Recorder",
            "075 - Pan Flute",
            "076 - Blown bottle",
            "077 - Shakuhachi",
            "078 - Whistle",
            "079 - Ocarina",
            "080 - Lead 1 (square)",
            "081 - Lead 2 (sawtooth)",
            "082 - Lead 3 (calliope)",
            "083 - Lead 4 (chiff)",
            "084 - Lead 5 (charang)",
            "085 - Lead 6 (voice)",
            "086 - Lead 7 (fifths)",
            "087 - Lead 8 (bass + lead)",
            "088 - Pad 1 (new age)",
            "089 - Pad 2 (warm)",
            "090 - Pad 3 (polysynth)",
            "091 - Pad 4 (choir)",
            "092 - Pad 5 (bowed)",
            "093 - Pad 6 (metallic)",
            "094 - Pad 7 (halo)",
            "095 - Pad 8 (sweep)",
            "096 - FX 1 (rain)",
            "097 - FX 2 (soundtrack)",
            "098 - FX 3 (crystal)",
            "099 - FX 4 (atmosphere)",
            "100 - FX 5 (brightness)",
            "101 - FX 6 (goblins)",
            "102 - FX 7 (echoes)",
            "103 - FX 8 (sci-fi)",
            "104 - Sitar",
            "105 - Banjo",
            "106 - Shamisen",
            "107 - Koto",
            "108 - Kalimba",
            "109 - Bag pipe",
            "110 - Fiddle",
            "111 - Shanai",
            "112 - Tinkle Bell",
            "113 - Agogô",
            "114 - Steel Drums",
            "115 - Woodblock",
            "116 - Taiko Drum",
            "117 - Melodic Tom",
            "118 - Synth Drum",
            "119 - Reverse Cymbal",
            "120 - Guitar Fret Noise",
            "121 - Breath Noise",
            "122 - Seashore",
            "123 - Bird Tweet",
            "124 - Telephone Ring",
            "125 - Helicopter",
            "126 - Applause",
            "127 - Gunshot"
        };

        public List<string> GlobalFilesNeeded = new List<string>
        {
            @"/songs.mk",
            @"/sound/voice_groups.inc",
            @"/include/constants/songs.h"
        };

        public List<string> GlobalFilesPaths = new List<string>();

        public string GlobalProjectPath = null;

        // BEGINING OF CODE

        public mainWindow()
        {
            bool opened;
            InitializeComponent();
            do
            {
                opened = true;
                switch (SetDirectory())
                {
                    case 1:
                        if(MessageBox.Show("Couldn't find the decomp project folder.", "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == DialogResult.Cancel)
                        {
                            Environment.Exit(1);
                        }
                        else
                        {
                            opened = false;
                        }
                        break;
                    case 2:
                        Environment.Exit(1);
                        break;
                    default:
                        break;
                }
            } while (!opened);

            GetVoicegroupList();
            OpenVoicegroupFile();
            GetVoicegroupSongs();
        }

        public int SetDirectory()
        {
            string folderDir = null;
            string currentDir = null;
            folderBrowserDialog.SelectedPath = Properties.Settings.Default.ConfigLastDirectory;
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                folderDir = @folderBrowserDialog.SelectedPath;
                foreach(string element in GlobalFilesNeeded)
                {
                    currentDir = folderDir + element;
                    if (!File.Exists(currentDir)) return 1;
                    GlobalFilesPaths.Add(currentDir);
                    GlobalProjectPath = folderDir;
                }
                Properties.Settings.Default.ConfigLastDirectory = folderDir;
                Properties.Settings.Default.Save();
                return 0;
            }
            return 2;
        }

        public bool GetVoicegroupList()
        {
            voicegroupFilesComboBox.Items.Clear();
            songsTreeView.Nodes.Clear();
            string[] voicegroups = File.ReadAllLines(GlobalFilesPaths[1]);
            foreach (string element in voicegroups)
            {
                voicegroupFilesComboBox.Items.Add("/" + element.Remove(0, 9).Trim('\"'));
                TreeNode voicegroup = new TreeNode(element.Remove(0, 28).Trim('\"'));
                if(element != GlobalCryTablesEntry) songsTreeView.Nodes.Add(voicegroup);
            }
            voicegroupFilesComboBox.Items.RemoveAt(0);
            voicegroupFilesComboBox.SelectedIndex = 0;
            return true;
        }

        public int GetVoicegroupSongs()
        {
            int i;
            bool done = true;
            do
            {
                if (!done) GetVoicegroupList();
                string[] songs = File.ReadAllLines(GlobalFilesPaths[0]);
                int vg = 0;
                string song = "";
                for (i = 5; i < songs.Length; i = i + 3)
                {
                    try
                    {
                        song = songs[i].Remove(songs[i].Length - 14).Remove(0, 14).ToUpper();
                        done = true;
                    }
                    catch
                    {
                        if (MessageBox.Show("Check all entries in /songs.mk are in the correct format.", "Error parsing /songs.mk", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == DialogResult.Cancel)
                        {
                            Environment.Exit(1);
                        }
                        else
                        {
                            done = false;
                            break;
                        }
                    }
                    try
                    {
                        vg = short.Parse(songs[i + 1].Remove(38).Remove(0, 35));
                        songsTreeView.Nodes[vg].Nodes.Add(song);
                    }
                    catch
                    {

                    }
                }
            } while (!done);
            return 0;
        }

        public bool OpenVoicegroupFile()
        {
            int i;
            instrumentsInVG.Items.Clear();
            codeInVG.Items.Clear();
            string[] code = File.ReadAllLines(Path.Combine(GlobalProjectPath + voicegroupFilesComboBox.SelectedItem.ToString()));
            for (i = 2; i < code.Length - 1; i++)
            {
                if (!code[i].Contains(GlobalDefaultInstrument))
                {
                    codeInVG.Items.Add(code[i].Remove(0,1).Remove(code[i].Length - 11, 10));
                    try
                    {
                        instrumentsInVG.Items.Add(GlobalInstrumentsList[i - 2]);
                    } catch
                    {
                        instrumentsInVG.Items.Add("--- - Not standard instrument");
                    }
                }
            }
            return true;
        }

        private void ActionOpenVoicegroupFileOnChange(object sender, EventArgs e)
        {
            OpenVoicegroupFile();
        }

        private void ActionEnableDisableButtons(object sender, EventArgs e)
        {
            switch(tabControl1.SelectedIndex)
            {
                default:
                    buttonAdd.Enabled = true;
                    buttonRemove.Enabled = true;
                    break;
                case 1:
                    buttonAdd.Enabled = false;
                    buttonRemove.Enabled = false;
                    break;
            }
        }

        private void ActionChangeVoicegroupComboBoxIndex(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeNode selectedNode = ((TreeView)sender).SelectedNode;
            int index = -1;
            if(selectedNode.Level > 0)
            {
                index = selectedNode.Parent.Index;
            }
            else
            {
                if(selectedNode.Nodes.Count == 0) index = selectedNode.Index;
            }
            if (index != -1) voicegroupFilesComboBox.SelectedIndex = index;
        }
    }

    public class InstrumentData
    {
        public static int id { get; set; }
        public static string instrument { get; set; }
        public static string[] lines { get; set; }
    }
}
