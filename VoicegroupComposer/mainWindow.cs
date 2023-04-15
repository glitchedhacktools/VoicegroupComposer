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
using VoiceGroupComposer;

namespace VoicegroupComposer
{
    public partial class mainWindow : Form
    {
        // GLOBAL VARIABLES DEFINITION

        public Repository RepositoryData = new();

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
            string folderDir = Properties.Settings.Default.ConfigLastDirectory;
            RepositoryData.InitRepository(folderDir);

            GetVoicegroupList();
            OpenVoicegroupFile(0, RepositoryData.Voicegroups);
            GetSongsList(RepositoryData.Songs);
            GetVoicegroupSongs(RepositoryData.Voicegroups, RepositoryData.Songs);
            InitInstrumentData();
            FillInstrumentTreeNode(0);
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

            foreach (Voicegroup vg in RepositoryData.Voicegroups)
            {
                string strVgId = "/sound/voicegroups/voicegroup" + vg.id.ToString("D3") + ".inc";
                voicegroupFilesComboBox.Items.Add(strVgId);
                TreeNode voicegroup = new TreeNode(strVgId);
                songsTreeView.Nodes.Add(voicegroup);
            }

            voicegroupFilesComboBox.SelectedIndex = 0;
            return true;
        }

        public bool GetSongsList(List<Midi> songs)
        {
            songFilesComboBox.Items.Clear();
            foreach (Midi song in songs)
            {
                songFilesComboBox.Items.Add(song.name);
            }
            songFilesComboBox.SelectedIndex = 0;
            return true;
        }

        public bool InitInstrumentData()
        {
            instrumentsComboBox.SelectedIndex = 0;
            return true;
        }

        public bool FillInstrumentTreeNode(int instrumentId)
        {
            List<Voicegroup> vgsWithInstrument = new List<Voicegroup>();
            List<(int, Sample)> samples = new();
            List<(int, Sample)> sorted = new();
            TreeNode currentInstrument = new TreeNode();

            // Get all vg ids in which the instrument appears
            foreach (int voicegroupId in RepositoryData.Instruments[instrumentId].vgIds)
            {
                vgsWithInstrument.Add(RepositoryData.Voicegroups[voicegroupId]);
            }

            // Get all samples in each previous vg used for the instrument and the vg assigned to them
            foreach (Voicegroup voicegroup in vgsWithInstrument)
            {
                (int, Sample) entry = (voicegroup.id, voicegroup.samples[instrumentId]);
                samples.Add(entry);
            }

            // Sort the list by sample so it can be easier to create nodes.
            samples.Sort((x, y) => y.Item2.data.CompareTo(x.Item2.data));
            string previousSample = string.Empty;

            // Create the tree view
            foreach ((int, Sample) sampleData in samples) 
            {
                if (previousSample != sampleData.Item2.data)
                {
                    if (previousSample != string.Empty)
                    {
                        instrumentsTreeView.Nodes.Add(currentInstrument);
                        instrumentsTreeView.Sort();
                    }
                    currentInstrument = new TreeNode(sampleData.Item2.data);
                    previousSample = sampleData.Item2.data;
                }
                TreeNode currentVg = new TreeNode("/sound/voicegroups/voicegroup" + RepositoryData.Voicegroups[sampleData.Item1].id.ToString("D3") + ".inc");
                currentInstrument.Nodes.Add(currentVg);

            }

            return true;
        }

        public int GetVoicegroupSongs(List<Voicegroup> voicegroups, List<Midi> songs)
        {
            int i;
            bool done = true;
            do
            {
                if (!done) GetVoicegroupList();
                foreach (Midi song in songs)
                {
                    songsTreeView.Nodes[song.vg.id].Nodes.Add(song.name);
                }
            } while (!done);
            return 0;
        }

        public bool OpenVoicegroupFile(int index, List<Voicegroup> voicegroups)
        {
            instrumentsInVG.Items.Clear();
            codeInVG.Items.Clear();
            Voicegroup voicegroup = voicegroups[index];
            
            foreach (Instrument instrument in voicegroup.instruments)
            {
                string instrumentToAdd = instrument.id.ToString("D3") + " - " + instrument.name;
                string lineToAdd = voicegroup.samples[instrument.id].data;
                codeInVG.Items.Add(lineToAdd);
                instrumentsInVG.Items.Add(instrumentToAdd);
            }
            
            return true;
        }

        private void ActionOpenVoicegroupFileOnChange(object sender, EventArgs e)
        {
            OpenVoicegroupFile(voicegroupFilesComboBox.SelectedIndex, RepositoryData.Voicegroups);
        }

        private void ActionEnableDisableButtons(object sender, EventArgs e)
        {
            switch(tabControl1.SelectedIndex)
            {
                // not working yet
                //case 1:
                //    buttonAdd.Enabled = true;
                //    buttonRemove.Enabled = true;
                //    break;
                default:
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
            if (index != -1)
            {
                voicegroupFilesComboBox.SelectedIndex = index;
            }
        }

        private void ActionChangeSongFilesComboBoxIndex(object sender, EventArgs e)
        {
            ToolStripComboBox comboSongName = ((ToolStripComboBox)sender);

            voicegroupFilesComboBox.SelectedIndex = RepositoryData.Songs[comboSongName.SelectedIndex].vg.id;
        }

        private void ActionSelectInstrumentComboBox(object sender, EventArgs e)
        {
            if (RepositoryData.Voicegroups.Count > 0)
            {
                instrumentsTreeView.Nodes.Clear();
                FillInstrumentTreeNode(instrumentsComboBox.SelectedIndex);
            }
        }
    }
}
