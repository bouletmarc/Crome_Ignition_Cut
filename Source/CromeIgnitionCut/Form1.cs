using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;

namespace CromeIgnitionCut
{
    public partial class Form1 : Form
    {
        string Version = "V1.0";
        string Filename = "";
        string Baserom = "";
        byte[] File_Byte = new byte[] { };
        DASM_Bytes DASM_Bytes_0;
        ASM_Bytes ASM_Bytes_0;
        int ModLocation = 0;
        int RPMLocation = 0;

        bool CanRefresh = true;

        List<string> AllLines = new List<string>();
        string LabelLocation = "";

        bool Error = false;

        //########### ADD CROME GOLD
        //########### ADD CROME GOLD
        //########### ADD CROME GOLD
        //########### ADD CROME GOLD
        //########### ADD CROME GOLD

        public Form1()
        {
            InitializeComponent();

            this.Text = "Crome Ignition Cut " + Version;

            LogText("Initialized");

            groupBox1.Enabled = false;
            groupBox2.Enabled = false;

            button1.Enabled = false;

            DASM_Bytes_0 = new DASM_Bytes();
            ASM_Bytes_0 = new ASM_Bytes();
        }

        private void textBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                button1.Enabled = true;

                this.textBox1.Text = Path.GetFileName(openFileDialog1.FileName);
                Filename = openFileDialog1.FileName;

                LogText("File Selected: " + Path.GetFileName(Filename));

                if (!HasMod(true)) LogText("Baseom NOT Modded!");
                else LogText("Baseom Already Modded!");

                //CheckRom();
            }
        }

        private void GetBaseromType()
        {
            if ((File_Byte[528] == 137 && File_Byte[529] == 0xc6 && File_Byte[530] == 0xab) ||
                (File_Byte[528] == 0x56 && File_Byte[529] == 0xd0 && File_Byte[530] == 0xf3)) Baserom = "GOLD";

            if ((File_Byte[528] == 212 && File_Byte[529] == 0x1a && File_Byte[530] == 0x02) ||
                (File_Byte[528] == 0x04 && File_Byte[529] == 0xe4 && File_Byte[530] == 0xf8)) Baserom = "P28";

            if ((File_Byte[528] == 0x2e && File_Byte[529] == 0xf9 && File_Byte[530] == 0x7d) ||
                (File_Byte[528] == 0x54 && File_Byte[529] == 0x7b && File_Byte[530] == 0xc4)) Baserom = "P30";

            if ((File_Byte[528] == 228 && File_Byte[529] == 0xf8 && File_Byte[530] == 0xa2) ||
                (File_Byte[528] == 0x10 && File_Byte[529] == 0x8a && File_Byte[530] == 0xc4)) Baserom = "P72";
        }

        //62E600
        private bool HasMod(bool CanReload)
        {
            bool Modded = false;
            Baserom = "";
            RPMLocation = 0;
            groupBox2.Enabled = false;

            try
            {
                File_Byte = File.ReadAllBytes(Filename);
                GetBaseromType();

                for (int i = 0; i < File_Byte.Length; i++)
                {
                    if (!Modded)
                    {
                        if (File_Byte[i] == 0x62 && File_Byte[i + 1] == 0xe6 && File_Byte[i + 2] == 0x00)
                        {
                            Modded = true;
                            ModLocation = i;
                        }
                    }

                    //C4233A
                    if (File_Byte[i] == 0xc4 && File_Byte[i + 1] == 0x23 && File_Byte[i + 2] == 0x3a)
                    {
                        if (Baserom != "GOLD" && Baserom != "")
                        {
                            string LocArea = File_Byte[i + 6].ToString("x2") + File_Byte[i + 5].ToString("x2");
                            RPMLocation = int.Parse(LocArea, System.Globalization.NumberStyles.HexNumber);
                        }
                        else
                        {
                            RPMLocation = i + 17;
                        }
                    }
                }
            }
            catch {
                Modded = false;
                Error = true;
            }

            groupBox1.Enabled = Modded;

            if (CanReload)
            {
                if (Modded)
                {
                    checkBox1.Checked = false;
                    checkBox2.Checked = false;
                    if (File_Byte[ModLocation - 3] == 0xff) checkBox2.Checked = true;
                    if (File_Byte[ModLocation + 8] == 0x00) checkBox1.Checked = true;
                }

                if (RPMLocation != 0)
                {
                    groupBox2.Enabled = true;
                    GetRPM();
                }
            }

            if (Modded) button2.Text = "Remove Mod";
            else button2.Text = "Add Mod";

            //Disable
            button2.Enabled = !Modded;

            return Modded;
        }

        private void GetRPM()
        {
            int LowR = 0;
            int LowS = 0;
            int HiR = 0;
            int HiS = 0;

            if (Baserom != "GOLD" && Baserom != "")
            {
                LowR = 1875000 / (File_Byte[RPMLocation + 2] + (0xff * File_Byte[RPMLocation + 2 + 1]));
                LowS = 1875000 / (File_Byte[RPMLocation + 2 + 6] + (0xff * File_Byte[RPMLocation + 2 + 7]));
                HiR = 1875000 / (File_Byte[RPMLocation + 2 + 12] + (0xff * File_Byte[RPMLocation + 2 + 13]));
                HiS = 1875000 / (File_Byte[RPMLocation + 2 + 18] + (0xff * File_Byte[RPMLocation + 2 + 19]));
            }
            else if(Baserom == "GOLD" && Baserom != "")
            {
                HiR = 1875000 / (File_Byte[RPMLocation] + (0xff * File_Byte[RPMLocation + 1]));
                HiS = 1875000 / (File_Byte[RPMLocation + 3] + (0xff * File_Byte[RPMLocation + 4]));
                LowR = 1875000 / (File_Byte[RPMLocation + 9] + (0xff * File_Byte[RPMLocation + 10]));
                LowS = 1875000 / (File_Byte[RPMLocation + 12] + (0xff * File_Byte[RPMLocation + 13]));
            }
            textBox3.Text = LowS.ToString();
            textBox4.Text = LowR.ToString();
            textBox6.Text = HiS.ToString();
            textBox5.Text = HiR.ToString();
        }

        private void CheckRom()
        {
            Baserom = "";
            Error = false;
            progressBar1.Value = 0;

            try
            {
                File_Byte = File.ReadAllBytes(Filename);
                GetBaseromType();
                if (Baserom != "") LogText("Baserom " + Baserom + " Found!");
            }
            catch
            {
                Error = true;
            }

            if (Baserom == "" || Error) LogText("Can't reconize the baserom type!");
            else
            {
                if (!HasMod(true))
                {
                    LogText("########################");
                    LogText("NOW ADDING SCRIPT TO BASEROM");

                    progressBar1.Value = 30;
                    DASM();
                    progressBar1.Value = 60;
                    if (!Error) ASM();
                    if (!Error)
                    {
                        if (HasMod(true)) LogText("Ignition cut succesfully added!");
                        else LogText("Ignition cut not properlly added!");
                        LogText("########################");
                    }
                }
                else
                {
                    LogText("Baseom Already Modded!");
                }
            }
            
            progressBar1.Value = 0;
        }

        private void DASM()
        {
            try
            {
                LogText("Disassembling binary...");

                File.Create(Application.StartupPath + @"\FileName.bin").Dispose();
                File.WriteAllBytes(Application.StartupPath + @"\FileName.bin", File_Byte);

                File.Create(Application.StartupPath + @"\dasm662.exe").Dispose();
                File.WriteAllBytes(Application.StartupPath + @"\dasm662.exe", DASM_Bytes_0.ThisBytes);

                //Create DASM.bat
                string BatTxt = "dasm662.exe FileName.bin FileName.asm";
                File.Create(Application.StartupPath + @"\DASM.bat").Dispose();
                File.WriteAllText(Application.StartupPath + @"\DASM.bat", BatTxt);

                //Load DASM.bat
                Process p = new Process();
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.FileName = Application.StartupPath + @"\DASM.bat";
                p.StartInfo.CreateNoWindow = true;
                //p.StartInfo.Verb = "runas";
                p.Start();

                Thread.Sleep(500);
                Process[] pname = Process.GetProcessesByName("DASM.bat");
                while (pname.Length > 1)
                {
                    Thread.Sleep(1);
                    pname = Process.GetProcessesByName("DASM.bat");
                }
                Thread.Sleep(2000);

                if (File.Exists(Application.StartupPath + @"\FileName.asm"))
                {
                    AddIgn();
                    RemoveFile();
                }
                else
                {
                    Error = true;
                    RemoveFile();
                    LogText("Can't disassemble binary");
                }
            }
            catch
            {
                Error = true;
                LogText("Can't add script to this file!");
            }
        }

        private void ASM()
        {
            LogText("Assembling binary...");

            File.Create(Application.StartupPath + @"\FileName.asm").Dispose();
            File.WriteAllLines(Application.StartupPath + @"\FileName.asm", AllLines);

            //Create ASM.exe
            File.Create(Application.StartupPath + @"\asm662.exe").Dispose();
            File.WriteAllBytes(Application.StartupPath + @"\asm662.exe", ASM_Bytes_0.ThisBytes);
            //Create ASM.bat
            string BatTxt = "asm662 FileName.asm FileName.bin";
            File.Create(Application.StartupPath + @"\ASM.bat").Dispose();
            File.WriteAllText(Application.StartupPath + @"\ASM.bat", BatTxt);

            //Load ASM.bat
            Process p = new Process();
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.StartInfo.FileName = Application.StartupPath + @"\ASM.bat";
            p.StartInfo.CreateNoWindow = true;
            //p.StartInfo.Verb = "runas";
            p.Start();

            Thread.Sleep(500);
            Process[] pname = Process.GetProcessesByName("ASM.bat");
            while (pname.Length > 1)
            {
                Thread.Sleep(1);
                pname = Process.GetProcessesByName("ASM.bat");
            }
            Thread.Sleep(2000);

            //Copy ASM File
            string ThisPath = Path.GetDirectoryName(Filename);
            string ThisFN = Path.GetFileNameWithoutExtension(Filename);
            string ThisNewPath = ThisPath + @"\" + ThisFN + "_IGNCUT.bin";
            File.Create(ThisNewPath).Dispose();
            File.WriteAllBytes(ThisNewPath, File.ReadAllBytes(Application.StartupPath + @"\FileName.bin"));

            LogText("File Saved: " + Path.GetFileName(ThisNewPath));

            this.textBox1.Text = Path.GetFileName(ThisNewPath);
            Filename = ThisNewPath;

            LogText("File Selected: " + Path.GetFileName(Filename));

            RemoveFile();
        }

        private void RemoveFile()
        {
            //Remove DASM
            if (File.Exists(Application.StartupPath + @"\FileName.bin")) File.Delete(Application.StartupPath + @"\FileName.bin");
            if (File.Exists(Application.StartupPath + @"\FileName.asm")) File.Delete(Application.StartupPath + @"\FileName.asm");
            if (File.Exists(Application.StartupPath + @"\dasm662.exe")) File.Delete(Application.StartupPath + @"\dasm662.exe");
            if (File.Exists(Application.StartupPath + @"\DASM.bat")) File.Delete(Application.StartupPath + @"\DASM.bat");
            if (File.Exists(Application.StartupPath + @"\asm662.exe")) File.Delete(Application.StartupPath + @"\asm662.exe");
            if (File.Exists(Application.StartupPath + @"\ASM.bat")) File.Delete(Application.StartupPath + @"\ASM.bat");
        }

        private void AddIgn()
        {
            LogText("Adding script...");
            string[] AllLinesT = File.ReadAllLines(Application.StartupPath + @"\FileName.asm");

            AllLines.Clear();
            for (int i = 0; i < AllLinesT.Length; i++) AllLines.Add(AllLinesT[i]);

            if (Baserom == "GOLD") AddIgnGOLD();
            if (Baserom == "P28") AddIgnP28();
            if (Baserom == "P30") AddIgnP30();
            if (Baserom == "P72") AddIgnP72();
        }

        //#########################################################################
        //#########################################################################

        private void AddIgnGOLD()
        {
            int CurrentMod = 0;
            int LastIndex = 0;

            for (int i = 0; i < AllLines.Count; i++)
            {
                if (CurrentMod == 0 && AllLines[i].Contains("off(000d0h).3,"))
                {
                    LabelLocation = AllLines[i + 3].Substring(AllLines[i + 3].LastIndexOf("_") + 1, 4);
                    AllLines.Insert(i + 2, "                JBS     off(000e6h).0, label_" + LabelLocation);
                    CurrentMod++;

                    LastIndex = i;
                    i = AllLines.Count;
                }
            }
            for (int i = LastIndex; i < AllLines.Count; i++)
            {
                if (CurrentMod == 1 && AllLines[i].Contains("off(TCON3), #008h"))
                {
                    LabelLocation = AllLines[i + 1].Substring(AllLines[i + 1].LastIndexOf("_") + 1, 4);
                    AllLines.Insert(i, "                JBS     off(000e6h).0, label_" + LabelLocation);
                    CurrentMod++;
                    LastIndex = i;
                    i = AllLines.Count;
                }
            }
            for (int i = LastIndex; i < AllLines.Count; i++)
            {
                if (CurrentMod == 2 && AllLines[i].Contains("off(TMR3), er3"))
                {
                    LabelLocation = AllLines[i - 1].Substring(AllLines[i - 1].LastIndexOf("_") + 1, 4);
                    AllLines.Insert(i + 1, "                JBS     off(000e6h).0, label_" + LabelLocation);
                    CurrentMod++;
                    LastIndex = i;
                    i = AllLines.Count;
                }
            }
            for (int i = LastIndex; i < AllLines.Count; i++)
            {
                if (CurrentMod == 3 && AllLines[i].Contains("C, (002ffh-00280h)[USP].7"))
                {
                    LabelLocation = AllLines[i + 1].Substring(AllLines[i + 1].LastIndexOf("_") + 1, 4);
                    AllLines.RemoveRange(i + 2, 5);
                    AllLines[i + 2] = "                SJ      label_" + LabelLocation;
                    CurrentMod++;
                    LastIndex = i - 5;
                    i = AllLines.Count;
                }
            }
            for (int i = LastIndex; i < AllLines.Count; i++)
            {
                if (CurrentMod == 4 && AllLines[i].Contains("off(00117h).2,"))
                {
                    LabelLocation = AllLines[i].Substring(AllLines[i].LastIndexOf("_") + 1, 4);
                    AllLines.Insert(i - 1, "                JBS     off(00117h).2, label_" + LabelLocation);
                    CurrentMod++;
                    LastIndex = i;
                    i = AllLines.Count;
                }
            }
            for (int i = LastIndex; i < AllLines.Count; i++)
            {
                if (CurrentMod == 5 && AllLines[i].Contains("off(0011ch).5,"))
                {
                    LabelLocation = AllLines[i + 3].Substring(AllLines[i + 3].LastIndexOf("_") + 1, 4);
                    int LabelLocationNumber = int.Parse(LabelLocation, System.Globalization.NumberStyles.HexNumber);

                    string LineStr = "";
                    LineStr += "                LB      A, #0ffh" + Environment.NewLine;
                    LineStr += "                JEQ     label_" + (LabelLocationNumber + 1).ToString("x4") + Environment.NewLine;
                    LineStr += "                MOV     DP, #000e6h" + Environment.NewLine;
                    LineStr += "                MB      [DP].0, C" + Environment.NewLine;
                    LineStr += "label_" + (LabelLocationNumber + 1).ToString("x4") + ":     JGE     label_" + (LabelLocationNumber + 3).ToString("x4") + Environment.NewLine;
                    LineStr += "                LB      A, #0ffh" + Environment.NewLine;
                    LineStr += "                JNE     label_" + (LabelLocationNumber + 2).ToString("x4") + Environment.NewLine;
                    LineStr += "label_" + (LabelLocationNumber + 2).ToString("x4") + ":     SB      off(0011ch).2" + Environment.NewLine;
                    LineStr += "label_" + (LabelLocationNumber + 3).ToString("x4") + ":";

                    AllLines[i + 4] = LineStr;

                    CurrentMod++;
                }
            }
            for (int i = LastIndex; i < AllLines.Count; i++)
            {
                if (CurrentMod == 6 && AllLines[i].Contains("C, off(00211h).7"))
                {
                    string LineStr = "";
                    LineStr += "                ROLB    A" + Environment.NewLine;
                    LineStr += "                MOV     DP, #000e6h" + Environment.NewLine;
                    LineStr += "                MB      C, [DP].0";

                    AllLines.Insert(i + 1, LineStr);
                    CurrentMod++;
                    LastIndex = i;
                    i = AllLines.Count;
                }
            }

            //Remove NOP
            int NOPRemoved = 0;
            LastIndex = 0;

            while (NOPRemoved < 23)
            {
                for (int i = LastIndex; i < AllLines.Count; i++)
                {
                    if (AllLines[i].Contains("NOP") && !AllLines[i].Contains("_"))
                    {
                        AllLines.RemoveAt(i);
                        NOPRemoved++;
                        LastIndex = i;
                        i = AllLines.Count;
                    }
                }
            }
        }

        //#########################################################################
        //#########################################################################

        private void AddIgnP28()
        {
            int CurrentMod = 0;
            int LastIndex = 0;

            for (int i = 0; i < AllLines.Count; i++)
            {
                if (CurrentMod == 0 && AllLines[i].Contains("off(000a0h).3,"))
                {
                    LabelLocation = AllLines[i + 3].Substring(AllLines[i + 3].LastIndexOf("_") + 1, 4);
                    AllLines.Insert(i + 2, "                JBS     off(000e6h).0, label_" + LabelLocation);
                    CurrentMod++;

                    LastIndex = i;
                    i = AllLines.Count;
                }
            }
            for (int i = LastIndex; i < AllLines.Count; i++)
            {
                if (CurrentMod == 1 && AllLines[i].Contains("off(TCON3), #008h"))
                {
                    LabelLocation = AllLines[i + 1].Substring(AllLines[i + 1].LastIndexOf("_") + 1, 4);
                    AllLines.Insert(i, "                JBS     off(000e6h).0, label_" + LabelLocation);
                    CurrentMod++;
                    LastIndex = i;
                    i = AllLines.Count;
                }
            }
            for (int i = LastIndex; i < AllLines.Count; i++)
            {
                if (CurrentMod == 2 && AllLines[i].Contains("off(TMR3), er3"))
                {
                    LabelLocation = AllLines[i - 1].Substring(AllLines[i - 1].LastIndexOf("_") + 1, 4);
                    AllLines.Insert(i + 1, "                JBS     off(000e6h).0, label_" + LabelLocation);
                    CurrentMod++;
                    LastIndex = i;
                    i = AllLines.Count;
                }
            }
            for (int i = LastIndex; i < AllLines.Count; i++)
            {
                if (CurrentMod == 3 && AllLines[i].Contains("0c4h, #000e2h"))
                {
                    LabelLocation = AllLines[i + 1].Substring(AllLines[i + 1].LastIndexOf("_") + 1, 4);
                    AllLines.RemoveRange(i + 2, 3);
                    AllLines[i + 2] = "                SJ      label_" + LabelLocation;
                    CurrentMod++;
                    LastIndex = i - 3;
                    i = AllLines.Count;
                }
            }
            for (int i = LastIndex; i < AllLines.Count; i++)
            {
                if (CurrentMod == 4 && AllLines[i].Contains("off(0011fh).2,"))
                {
                    LabelLocation = AllLines[i].Substring(AllLines[i].LastIndexOf("_") + 1, 4);
                    AllLines.Insert(i - 1, "                JBS     off(0011fh).2, label_" + LabelLocation);
                    CurrentMod++;
                    LastIndex = i;
                    i = AllLines.Count;
                }
            }
            for (int i = LastIndex; i < AllLines.Count; i++)
            {
                if (CurrentMod == 5 && AllLines[i].Contains("off(00124h).5,"))
                {
                    LabelLocation = AllLines[i + 3].Substring(AllLines[i + 3].LastIndexOf("_") + 1, 4);
                    int LabelLocationNumber = int.Parse(LabelLocation, System.Globalization.NumberStyles.HexNumber);

                    string LineStr = "";
                    LineStr += "                LB      A, #0ffh" + Environment.NewLine;
                    LineStr += "                JEQ     label_" + (LabelLocationNumber + 1).ToString("x4") + Environment.NewLine;
                    LineStr += "                MOV     DP, #000e6h" + Environment.NewLine;
                    LineStr += "                MB      [DP].0, C" + Environment.NewLine;
                    LineStr += "label_" + (LabelLocationNumber + 1).ToString("x4") + ":     JGE     label_" + (LabelLocationNumber + 3).ToString("x4") + Environment.NewLine;
                    LineStr += "                LB      A, #0ffh" + Environment.NewLine;
                    LineStr += "                JNE     label_" + (LabelLocationNumber + 2).ToString("x4") + Environment.NewLine;
                    LineStr += "label_" + (LabelLocationNumber + 2).ToString("x4") + ":     SB      off(00124h).2" + Environment.NewLine;
                    LineStr += "label_" + (LabelLocationNumber + 3).ToString("x4") + ":";

                    AllLines[i + 4] = LineStr;

                    CurrentMod++;
                }
            }
            for (int i = LastIndex; i < AllLines.Count; i++)
            {
                if (CurrentMod == 6 && AllLines[i].Contains("C, off(00211h).7"))
                {
                    string LineStr = "";
                    LineStr += "                ROLB    A" + Environment.NewLine;
                    LineStr += "                MOV     DP, #000e6h" + Environment.NewLine;
                    LineStr += "                MB      C, [DP].0";

                    AllLines.Insert(i + 1, LineStr);
                    CurrentMod++;
                    LastIndex = i;
                    i = AllLines.Count;
                }
            }

            //Remove NOP
            int NOPRemoved = 0;
            LastIndex = 0;

            while (NOPRemoved < 26)
            {
                for (int i = LastIndex; i < AllLines.Count; i++)
                {
                    if (AllLines[i].Contains("NOP") && !AllLines[i].Contains("_"))
                    {
                        AllLines.RemoveAt(i);
                        NOPRemoved++;
                        LastIndex = i;
                        i = AllLines.Count;
                    }
                }
            }
        }

        //#########################################################################
        //#########################################################################

        private void AddIgnP30()
        {
            int CurrentMod = 0;
            int LastIndex = 0;

            for (int i = 0; i < AllLines.Count; i++)
            {
                if (CurrentMod == 0 && AllLines[i].Contains("off(000d0h).3,"))
                {
                    LabelLocation = AllLines[i + 3].Substring(AllLines[i + 3].LastIndexOf("_") + 1, 4);
                    AllLines.Insert(i + 2, "                JBS     off(000e6h).0, label_" + LabelLocation);
                    CurrentMod++;

                    LastIndex = i;
                    i = AllLines.Count;
                }
            }
            for (int i = LastIndex; i < AllLines.Count; i++)
            {
                if (CurrentMod == 1 && AllLines[i].Contains("off(TCON3), #008h"))
                {
                    LabelLocation = AllLines[i + 1].Substring(AllLines[i + 1].LastIndexOf("_") + 1, 4);
                    AllLines.Insert(i, "                JBS     off(000e6h).0, label_" + LabelLocation);
                    CurrentMod++;
                    LastIndex = i;
                    i = AllLines.Count;
                }
            }
            for (int i = LastIndex; i < AllLines.Count; i++)
            {
                if (CurrentMod == 2 && AllLines[i].Contains("off(TMR3), er3"))
                {
                    LabelLocation = AllLines[i - 1].Substring(AllLines[i - 1].LastIndexOf("_") + 1, 4);
                    AllLines.Insert(i + 1, "                JBS     off(000e6h).0, label_" + LabelLocation);
                    CurrentMod++;
                    LastIndex = i;
                    i = AllLines.Count;
                }
            }
            for (int i = LastIndex; i < AllLines.Count; i++)
            {
                if (CurrentMod == 3 && AllLines[i].Contains("0ach, #000e2h"))
                {
                    LabelLocation = AllLines[i + 1].Substring(AllLines[i + 1].LastIndexOf("_") + 1, 4);
                    AllLines.RemoveRange(i + 2, 3);
                    AllLines[i + 2] = "                SJ      label_" + LabelLocation;
                    CurrentMod++;
                    LastIndex = i - 3;
                    i = AllLines.Count;
                }
            }
            for (int i = LastIndex; i < AllLines.Count; i++)
            {
                if (CurrentMod == 4 && AllLines[i].Contains("off(00117h).2,"))
                {
                    LabelLocation = AllLines[i].Substring(AllLines[i].LastIndexOf("_") + 1, 4);
                    AllLines.Insert(i - 1, "                JBS     off(00117h).2, label_" + LabelLocation);
                    CurrentMod++;
                    LastIndex = i;
                    i = AllLines.Count;
                }
            }
            for (int i = LastIndex; i < AllLines.Count; i++)
            {
                if (CurrentMod == 5 && AllLines[i].Contains("off(0011ch).5,"))
                {
                    LabelLocation = AllLines[i + 3].Substring(AllLines[i + 3].LastIndexOf("_") + 1, 4);
                    int LabelLocationNumber = int.Parse(LabelLocation, System.Globalization.NumberStyles.HexNumber);

                    string LineStr = "";
                    LineStr += "                LB      A, #0ffh" + Environment.NewLine;
                    LineStr += "                JEQ     label_" + (LabelLocationNumber + 1).ToString("x4") + Environment.NewLine;
                    LineStr += "                MOV     DP, #000e6h" + Environment.NewLine;
                    LineStr += "                MB      [DP].0, C" + Environment.NewLine;
                    LineStr += "label_" + (LabelLocationNumber + 1).ToString("x4") + ":     JGE     label_" + (LabelLocationNumber + 3).ToString("x4") + Environment.NewLine;
                    LineStr += "                LB      A, #0ffh" + Environment.NewLine;
                    LineStr += "                JNE     label_" + (LabelLocationNumber + 2).ToString("x4") + Environment.NewLine;
                    LineStr += "label_" + (LabelLocationNumber + 2).ToString("x4") + ":     SB      off(0011ch).2" + Environment.NewLine;
                    LineStr += "label_" + (LabelLocationNumber + 3).ToString("x4") + ":";

                    AllLines[i + 4] = LineStr;

                    CurrentMod++;
                }
            }
            for (int i = LastIndex; i < AllLines.Count; i++)
            {
                if (CurrentMod == 6 && AllLines[i].Contains("C, off(00211h).7"))
                {
                    string LineStr = "";
                    LineStr += "                ROLB    A" + Environment.NewLine;
                    LineStr += "                MOV     DP, #000e6h" + Environment.NewLine;
                    LineStr += "                MB      C, [DP].0";

                    AllLines.Insert(i + 1, LineStr);
                    CurrentMod++;
                    LastIndex = i;
                    i = AllLines.Count;
                }
            }

            //Remove NOP
            int NOPRemoved = 0;
            LastIndex = 0;

            while (NOPRemoved < 25)
            {
                for (int i = LastIndex; i < AllLines.Count; i++)
                {
                    if (AllLines[i].Contains("NOP") && !AllLines[i].Contains("_"))
                    {
                        AllLines.RemoveAt(i);
                        NOPRemoved++;
                        LastIndex = i;
                        i = AllLines.Count;
                    }
                }
            }
        }

        //#########################################################################
        //#########################################################################

        private void AddIgnP72()
        {
            int CurrentMod = 0;
            int LastIndex = 0;

            for (int i = 0; i < AllLines.Count; i++)
            {
                if (CurrentMod == 0 && AllLines[i].Contains("off(000a0h).3,"))
                {
                    LabelLocation = AllLines[i + 3].Substring(AllLines[i + 3].LastIndexOf("_") + 1, 4);
                    AllLines.Insert(i + 2, "                JBS     off(000e6h).0, label_" + LabelLocation);
                    CurrentMod++;
                    LastIndex = i;
                    i = AllLines.Count;
                }
            }
            for (int i = LastIndex; i < AllLines.Count; i++)
            {
                if (CurrentMod == 1 && AllLines[i].Contains("off(TCON3), #008h"))
                {
                    LabelLocation = AllLines[i + 1].Substring(AllLines[i + 1].LastIndexOf("_") + 1, 4);
                    AllLines.Insert(i, "                JBS     off(000e6h).0, label_" + LabelLocation);
                    CurrentMod++;
                    LastIndex = i;
                    i = AllLines.Count;
                }
            }
            for (int i = LastIndex; i < AllLines.Count; i++)
            {
                if (CurrentMod == 2 && AllLines[i].Contains("off(TMR3), er3"))
                {
                    LabelLocation = AllLines[i - 1].Substring(AllLines[i - 1].LastIndexOf("_") + 1, 4);
                    AllLines.Insert(i + 1, "                JBS     off(000e6h).0, label_" + LabelLocation);
                    CurrentMod++;
                    LastIndex = i;
                    i = AllLines.Count;
                }
            }
            for (int i = LastIndex; i < AllLines.Count; i++)
            {
                if (CurrentMod == 3 && AllLines[i].Contains("0c4h, #000e7h"))
                {
                    LabelLocation = AllLines[i + 1].Substring(AllLines[i + 1].LastIndexOf("_") + 1, 4);
                    AllLines.RemoveRange(i + 2, 3);
                    AllLines[i + 2] = "                SJ      label_" + LabelLocation;
                    CurrentMod++;
                    LastIndex = i - 3;
                    i = AllLines.Count;
                }
            }
            for (int i = LastIndex; i < AllLines.Count; i++)
            {
                if (CurrentMod == 4 && AllLines[i].Contains("off(0011fh).2,"))
                {
                    LabelLocation = AllLines[i].Substring(AllLines[i].LastIndexOf("_") + 1, 4);
                    AllLines.Insert(i - 1, "                JBS     off(0011fh).2, label_" + LabelLocation);
                    CurrentMod++;
                    LastIndex = i;
                    i = AllLines.Count;
                }
            }
            for (int i = LastIndex; i < AllLines.Count; i++)
            {
                if (CurrentMod == 5 && AllLines[i].Contains("off(00129h).1,") && AllLines[i + 1].Contains("NOP") && AllLines[i + 9].Contains("NOP"))
                {
                    AllLines.RemoveRange(i + 1, 9);
                    CurrentMod++;
                    LastIndex = i;
                    i = AllLines.Count;
                }
            }
            for (int i = LastIndex; i < AllLines.Count; i++)
            {
                if (CurrentMod == 6 && AllLines[i].Contains("off(00124h).5,"))
                {
                    LabelLocation = AllLines[i + 3].Substring(AllLines[i + 3].LastIndexOf("_") + 1, 4);
                    int LabelLocationNumber = int.Parse(LabelLocation, System.Globalization.NumberStyles.HexNumber);

                    string LineStr = "";
                    LineStr += "                LB      A, #0ffh" + Environment.NewLine;
                    LineStr += "                JEQ     label_" + (LabelLocationNumber + 1).ToString("x4") + Environment.NewLine;
                    LineStr += "                MOV     DP, #000e6h" + Environment.NewLine;
                    LineStr += "                MB      [DP].0, C" + Environment.NewLine;
                    LineStr += "label_" + (LabelLocationNumber + 1).ToString("x4") + ":     JGE     label_" + (LabelLocationNumber + 3).ToString("x4") + Environment.NewLine;
                    LineStr += "                LB      A, #0ffh" + Environment.NewLine;
                    LineStr += "                JNE     label_" + (LabelLocationNumber + 2).ToString("x4") + Environment.NewLine;
                    LineStr += "label_" + (LabelLocationNumber + 2).ToString("x4") + ":     SB      off(00124h).2" + Environment.NewLine;
                    LineStr += "label_" + (LabelLocationNumber + 3).ToString("x4") + ":";

                    AllLines[i + 4] = LineStr;

                    CurrentMod++;
                }
            }
            for (int i = LastIndex; i < AllLines.Count; i++)
            {
                if (CurrentMod == 7 && AllLines[i].Contains("ADCR7H"))
                {
                    string LineStr = "";
                    LineStr += "                ROLB    A" + Environment.NewLine;
                    LineStr += "                MOV     DP, #000e6h" + Environment.NewLine;
                    LineStr += "                MB      C, [DP].0";

                    AllLines.Insert(i + 23, LineStr);
                    CurrentMod++;
                    LastIndex = i;
                    i = AllLines.Count;
                }
            }



            //Remove NOP
            int NOPRemoved = 0;
            LastIndex = 0;

            while (NOPRemoved < 17) {
                for (int i = LastIndex; i < AllLines.Count; i++)
                {
                    if (AllLines[i].Contains("NOP") && !AllLines[i].Contains("_"))
                    {
                        AllLines.RemoveAt(i);
                        NOPRemoved++;
                        LastIndex = i;
                        i = AllLines.Count;
                    }
                }
            }
        }

        //#########################################################################
        //#########################################################################

        private void LogText(string ThisT)
        {
            textBox2.AppendText(ThisT + Environment.NewLine);
        }

        private void SaveChange()
        {
            File.Create(Filename).Dispose();
            File.WriteAllBytes(Filename, File_Byte);

            LogText("File Saved: " + Path.GetFileName(Filename));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CanRefresh = false;

            try
            {
                File_Byte = File.ReadAllBytes(Filename);

                HasMod(false);
                ApplyCut();
                ApplyRev();
                SaveChange();
                HasMod(true);

                /*if (!HasMod(false)) CheckRom();
                else
                {
                    ApplyCut();
                    ApplyRev();
                    SaveChange();
                    HasMod(true);
                }*/
            }
            catch
            {
                LogText("Unable to Apply");
            }

            CanRefresh = true;
        }

        private void ApplyCut()
        {
            LogText("Applying Cut Settings");

            byte FuelCut = 0xff;
            byte IgnCut = 0x00;
            if (checkBox1.Checked) FuelCut = 0x00;
            if (checkBox2.Checked) IgnCut = 0xff;
            
            File_Byte[ModLocation - 3] = IgnCut;
            File_Byte[ModLocation + 8] = FuelCut;
        }

        private void ApplyRev()
        {
            LogText("Applying Rev Limit Settings");

            byte RevB1 = 0;
            byte RevB2 = 0;

            int ByteNum = 0;

            ByteNum = 1875000 / int.Parse(textBox3.Text);
            while (ByteNum > 0xff) { RevB2++; ByteNum = ByteNum - 0xff; }
            RevB1 = (byte) ByteNum;

            if (Baserom != "GOLD" && Baserom != "")
            {
                File_Byte[RPMLocation + 2 + 6] = RevB1;
                File_Byte[RPMLocation + 2 + 7] = RevB2;
            }
            else if (Baserom == "GOLD" && Baserom != "")
            {
                File_Byte[RPMLocation + 12] = RevB1;
                File_Byte[RPMLocation + 13] = RevB2;
            }

            //########################################
            RevB1 = 0;
            RevB2 = 0;
            ByteNum = 1875000 / int.Parse(textBox4.Text);
            while (ByteNum > 0xff) { RevB2++; ByteNum = ByteNum - 0xff; }
            RevB1 = (byte)ByteNum;

            if (Baserom != "GOLD" && Baserom != "")
            {
                File_Byte[RPMLocation + 2] = RevB1;
                File_Byte[RPMLocation + 2 + 1] = RevB2;
            }
            else if (Baserom == "GOLD" && Baserom != "")
            {
                File_Byte[RPMLocation + 9] = RevB1;
                File_Byte[RPMLocation + 10] = RevB2;
            }

            //########################################
            RevB1 = 0;
            RevB2 = 0;
            ByteNum = 1875000 / int.Parse(textBox5.Text);
            while (ByteNum > 0xff) { RevB2++; ByteNum = ByteNum - 0xff; }
            RevB1 = (byte)ByteNum;

            if (Baserom != "GOLD" && Baserom != "")
            {
                File_Byte[RPMLocation + 2 + 12] = RevB1;
                File_Byte[RPMLocation + 2 + 13] = RevB2;
            }
            else if (Baserom == "GOLD" && Baserom != "")
            {
                File_Byte[RPMLocation] = RevB1;
                File_Byte[RPMLocation + 1] = RevB2;
            }

            //########################################
            RevB1 = 0;
            RevB2 = 0;
            ByteNum = 1875000 / int.Parse(textBox6.Text);
            while (ByteNum > 0xff) { RevB2++; ByteNum = ByteNum - 0xff; }
            RevB1 = (byte)ByteNum;

            if (Baserom != "GOLD" && Baserom != "")
            {
                File_Byte[RPMLocation + 2 + 18] = RevB1;
                File_Byte[RPMLocation + 2 + 19] = RevB2;
            }
            else if(Baserom == "GOLD" && Baserom != "")
            {
                File_Byte[RPMLocation + 3] = RevB1;
                File_Byte[RPMLocation + 4] = RevB2;
            }
        }

        private void RefreshRPM()
        {
            LogText("Refreshing Rev Limit Values");

            byte RevB1 = 0;
            byte RevB2 = 0;

            int ByteNum = 0;

            ByteNum = 1875000 / int.Parse(textBox3.Text);
            while (ByteNum > 0xff) { RevB2++; ByteNum = ByteNum - 0xff; }
            RevB1 = (byte)ByteNum;
            int ThisRPM = 1875000 / (RevB1 + (0xff * RevB2));
            textBox3.Text = ThisRPM.ToString();

            RevB1 = 0;
            RevB2 = 0;
            ByteNum = 1875000 / int.Parse(textBox4.Text);
            while (ByteNum > 0xff) { RevB2++; ByteNum = ByteNum - 0xff; }
            RevB1 = (byte)ByteNum;
            ThisRPM = 1875000 / (RevB1 + (0xff * RevB2));
            textBox4.Text = ThisRPM.ToString();

            RevB1 = 0;
            RevB2 = 0;
            ByteNum = 1875000 / int.Parse(textBox5.Text);
            while (ByteNum > 0xff) { RevB2++; ByteNum = ByteNum - 0xff; }
            RevB1 = (byte)ByteNum;
            ThisRPM = 1875000 / (RevB1 + (0xff * RevB2));
            textBox5.Text = ThisRPM.ToString();

            RevB1 = 0;
            RevB2 = 0;
            ByteNum = 1875000 / int.Parse(textBox6.Text);
            while (ByteNum > 0xff) { RevB2++; ByteNum = ByteNum - 0xff; }
            RevB1 = (byte)ByteNum;
            ThisRPM = 1875000 / (RevB1 + (0xff * RevB2));
            textBox6.Text = ThisRPM.ToString();
        }

        private void textBox3_Validated(object sender, EventArgs e)
        {
            if (CanRefresh) RefreshRPM();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            CanRefresh = false;

            try
            {
                File_Byte = File.ReadAllBytes(Filename);

                if (!HasMod(false)) CheckRom();
                //else RemovedMod();

                //Reload HasMod
                HasMod(false);
            }
            catch
            {
                LogText("Unable to Add/Remove Mod");
            }

            CanRefresh = true;
        }
    }
}
