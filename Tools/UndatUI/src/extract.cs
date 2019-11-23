﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace undat_ui
{
    public class Extractor
    {
        Action<string, int, int> updater;
        Action<string> error;

        string masterPath;
        string outputPath;
        string[] extractFiles;

        int completedFiles = 0;
        int curFile = 0;
        int numFiles = 0;

        FO1Dat dat;

        public Extractor(Action<string> error, Action<string, int, int> updater,
            string masterPath, string outputPath, string[] extractFiles)
        {
            this.updater = updater;
            this.error = error;
            this.masterPath = masterPath;
            this.outputPath = outputPath + "\\data";
            this.extractFiles = extractFiles;
            this.numFiles = this.extractFiles.Count();
        }

        public void ExtractFiles()
        {
            while (completedFiles < numFiles)
            {
                var f = GetNextFile();
                if (f == null)
                    break; // we are done.

                var ent = f.Split('\\');
                var dir = "";
                foreach (var d in ent)
                {
                    if (d.Contains("."))
                        break;
                    dir += d + "\\";
                    if (!Directory.Exists(this.outputPath + "\\" + dir))
                        Directory.CreateDirectory(this.outputPath + "\\" + dir);
                }

                var file = dat.getFile(f);
                if (file == null)
                    continue;
                File.WriteAllBytes($"{this.outputPath}\\{f}", dat.getData(file));
                
                this.updater(f, completedFiles++, this.numFiles);
            }

            this.updater("All files were extracted.", this.numFiles, this.numFiles);
        }

        public string GetNextFile()
        {
            var c = curFile++;
            if (c >= numFiles)
                return null;
            return extractFiles[c];
        }

        public void Begin()
        {
            dat = new FO1Dat();
            var error = dat.Open(masterPath);

            if (error == ReadError.FileDoesntExist)
            {
                this.error("The file you've provided as MASTER.DAT doesn't exist.");
                return;
            }

            if (error == ReadError.NotValidMasterDat)
            {
                this.error("The file is not a valid MASTER.DAT from Fallout 1.");
                return;
            }

            if (!Directory.Exists(this.outputPath))
            {
                this.error($"Directory that you've selected as destination: '{this.outputPath}' doesn't exist.");
                return;
            }

            var t = new Thread(new ThreadStart(ExtractFiles));
            t.Start();
        }

    }
}