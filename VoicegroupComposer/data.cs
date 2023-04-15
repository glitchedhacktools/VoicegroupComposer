using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace VoiceGroupComposer
{
    public class Repository
    {
        public string? RootDir = null,
            MakefileDir = null,
            VoicegroupsDir = null,
            SongsDir = null,
            SongsTableDir = null;

        public List<Instrument> Instruments = new List<Instrument>();
        public List<Voicegroup> Voicegroups = new List<Voicegroup>();
        public List<Midi> Songs             = new List<Midi>();

        public List<string> VoicegroupFiles = new List<string>();

        public Repository()
        {

        }

        public void InitRepository(string root)
        {
            RootDir = root + '/';
            InitDirectories();
            ReadInstrumentFile();
            ReadVoicegroupListFile();
            ReadVoicegroupFiles();
            ReadSongsFiles();
        }

        public void InitDirectories()
        {
            MakefileDir       = RootDir + "songs.mk";
            VoicegroupsDir    = RootDir + "sound/voice_groups.inc";
            SongsDir          = RootDir + "include/constants/songs.h";
            SongsTableDir     = RootDir + "sound/song_table.inc";
        }

        public Instrument NonStandardInstrument()
        {
            Instrument nonStandard = new Instrument(255, "Not standard instrument");
            return nonStandard;
        }

        public void ReadInstrumentFile()
        {
            string[] instrumentsFile = File.ReadAllLines("midi_instruments.json");
            for(int i = 1; i < instrumentsFile.Count() - 1; i++)
            {
                string instrumentsFileLine = instrumentsFile[i].TrimStart().TrimEnd(',');
                string instrumentId = instrumentsFileLine.Split(':')[0].Trim('\"');
                string instrumentName = instrumentsFileLine.Split(':')[1].Remove(0,1).Trim('\"');
                Instrument newInstrument = new(Int16.Parse(instrumentId), instrumentName);
                Instruments.Add(newInstrument);
            }
            //Instruments.Add(NonStandardInstrument());
        }

        public void ReadVoicegroupListFile()
        {
            string cryTablesEntry = ".include \"sound/cry_tables.inc\"";
            string[] voicegroupsFile = File.ReadAllLines(VoicegroupsDir);
            foreach(string line in voicegroupsFile)
            {
                if (line != cryTablesEntry)
                {
                    VoicegroupFiles.Add(line.Remove(0, 9).Trim('\"'));
                }
            }
        }

        public void ReadVoicegroupFiles()
        {
            string defaultInstrument = "voice_square_1 60, 0, 0, 2, 0, 0, 15, 0";
            foreach (string file in VoicegroupFiles)
            {
                Voicegroup voicegroup = new Voicegroup(file);
                string[] vgContent = File.ReadAllLines(RootDir + file);
                for(int i = 2; i < vgContent.Length; i++)
                {
                    int id = i - 2;
                    string route = vgContent[i].Trim();
                    if(route.Contains("@"))
                    {
                        route = route.Remove(vgContent[i].Length - 12);
                    }
                    if(route != defaultInstrument && route != "")
                    {
                        if (id < Instruments.Count)
                        {
                            voicegroup.AddInstrument(Instruments[id]);
                            Instruments[id].AddVoicegroupId(voicegroup.id);
                        }
                        else
                        {
                            voicegroup.AddInstrument(Instruments[Instruments.Count - 1]);
                            Instruments[Instruments.Count - 1].AddVoicegroupId(voicegroup.id);
                        }
                    }
                    Sample sample = new Sample(id, route);
                    voicegroup.AddSample(sample);
                }
                Voicegroups.Add(voicegroup);
            }
        }

        public void ReadSongsFiles()
        {
            SortedList<string, int> MidiFiles       = ReadSongTable();
            SortedList<int, string> SongNames       = ReadSongConstantsFile();
            SortedList<string, int> SongVoicegroups = ReadMakefile();

            foreach(string file in SongVoicegroups.Keys)
            {
                int songId          = MidiFiles[file];
                int voicegroupId    = SongVoicegroups[file];
                string songName     = SongNames[songId];

                Midi Song = new(songId, songName, file, Voicegroups[voicegroupId]);

                Songs.Add(Song);
            }
        }

        public SortedList<string, int> ReadSongTable()
        {
            SortedList<string, int> MidiFiles = new SortedList<string, int>();
            string[] songsTable = File.ReadAllLines(SongsTableDir);

            // Loop to extract the relevant info from /sound/song_table.inc
            for (int i = 3; i < songsTable.Length; i++)
            {
                int fileId = i - 3;
                string[] tableData = songsTable[i].Trim().Split(' ');
                // Check if there is an entry (it has always 4 items: 1. keyword 'song', 2. song file name, 3. and 4. two integers)
                if (tableData.Length >= 4)
                {
                    // Adds the .s extension to get the file name.
                    string songFile = tableData[1].Trim(',') + ".s";
                    if(!MidiFiles.Keys.Contains(songFile)) MidiFiles.Add(songFile, fileId);
                }

            }

            return MidiFiles;
        }

        public SortedList<int, string> ReadSongConstantsFile()
        {
            string[] songsFile = File.ReadAllLines(SongsDir);
            SortedList<int, string> SongNames = new SortedList<int, string>();

            // Loop to extract the relevant info from /include/constants/songs.h
            for (int i = 5; i < songsFile.Length; i++)
            {
                List<string> headerLineData = new List<string>();
                foreach (string slot in songsFile[i].Split(' '))
                {
                    if (slot != "")
                    {
                        headerLineData.Add(slot);
                    }
                }

                // Check if there is an entry (it begins with #define)
                if (headerLineData.Count >= 3 && headerLineData.Contains("#define"))
                {
                    // Not very common, but sometimes an entry uses an alias instead an integer ID. This skips it, not a real thing.
                    if (short.TryParse(headerLineData[2], out _))
                    {
                        int songId = short.Parse(headerLineData[2]);
                        string songName = songsFile[i].Split(' ')[1];
                        SongNames.Add(songId, songName);
                    }
                    headerLineData.Clear();
                }
            }
            return SongNames;
        }
    
        public SortedList<string, int> ReadMakefile()
        {
            SortedList<string, int> Voicegroups = new SortedList<string, int>();
            string[] makefile = File.ReadAllLines(MakefileDir);

            // Add mus_dummy to avoid explosions.
            Voicegroups.Add("mus_dummy.s", 0);

            // Loop to extract the relevant info from /songs.mk
            for (int i = 5; i < makefile.Length; i += 3)
            {
                // Get the filename
                string name = makefile[i].Substring(14).Split(' ')[0].Trim(':');
                // Get the voicegroup
                string[] parameters = makefile[i + 1].Trim().Split(' ');
                foreach (string param in parameters)
                {
                    if(param.Contains("-G"))
                    {
                        int vg = int.Parse(param.Substring(2));
                        Voicegroups.Add(name, vg);
                    }
                }
            }

            return Voicegroups;
        }
    }

    public class Midi
    {
        public int id;
        public string name;
        public string file;
        public Voicegroup vg;

        public Midi(int id, string name, string file, Voicegroup vg)
        {
            this.id = id;
            this.name = name;
            this.file = file;
            this.vg = vg;
        }
    }

    public class Voicegroup
    {
        public int id;
        public List<Instrument> instruments;
        public List<Sample> samples;

        public Voicegroup(string vgLine) {
            string vgNum = vgLine.Remove(0, 28).Trim('\"').Substring(0,3);
            id = Int16.Parse(vgNum);
            instruments = new List<Instrument>();
            samples = new List<Sample>();
        }

        public void AddInstrument(Instrument newInstrument) 
        {
            instruments.Add(newInstrument);
        }

        public void AddSample(Sample newSample)
        {
            samples.Add(newSample);
        }
    }

    public class Instrument
    {
        public int id;
        public string name;
        public List<int> vgIds;

        public Instrument(int id, string name)
        {
            this.id = id;
            this.name = name;
            vgIds = new List<int>();
        }

        public void AddVoicegroupId(int vgId)
        {
            vgIds.Add(vgId);
        }
    }

    public class Sample
    {
        public int id;
        public string data;

        public Sample(int id, string route)
        {
            this.id = id;
            this.data = route;
        }

        public bool isDefaultSample()
        {
            string defaultInstrument = "voice_square_1 60, 0, 0, 2, 0, 0, 15, 0";
            if (this.data == defaultInstrument) return true;
            return false;
        }
    }
}
