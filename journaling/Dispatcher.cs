using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace Journaling
{
    class FileInfo
    {
        public String FileName;
        public DateTime LastEdit;
        public String LastHash;

        public FileInfo(string fileName, DateTime lastEdit, string lastHash)
        {
            FileName = fileName;
            LastEdit = lastEdit;
            LastHash = lastHash;
        }
    }

    class Dispatcher
    {
        public const string LOGGING_FILE = "LOGS.txt";

        public const string FILES_INFO = "FILES.txt";

        public Dictionary<String, FileInfo> Files;
        public JournalStructure Journal;
        public String FilesDirectory;
        public static string ComputeMD5Checksum(string path)
        {
            using (FileStream fs = System.IO.File.OpenRead(path))
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] fileData = new byte[fs.Length];
                fs.Read(fileData, 0, (int)fs.Length);
                byte[] checkSum = md5.ComputeHash(fileData);
                string result = BitConverter.ToString(checkSum).Replace("-", String.Empty);
                return result;
            }
        }
        public string ProgramFilesDirectory;

        public string BackupFilesDirectory;
        public Dispatcher()
        {
            
            ProgramFilesDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).ToString();
            BackupFilesDirectory = $"{ProgramFilesDirectory}/Backup";
            FilesDirectory = $"{ProgramFilesDirectory}/Files";
            if (!Directory.Exists(BackupFilesDirectory)) {
                Directory.CreateDirectory(BackupFilesDirectory);
            }
            if (!Directory.Exists(FilesDirectory))
            {
                Directory.CreateDirectory(FilesDirectory);
            }
            var LogsExist = File.Exists(ExectNameGeneratorProgramFilesDirectory(LOGGING_FILE));
            var FilesExist = File.Exists(ExectNameGeneratorProgramFilesDirectory(FILES_INFO));
            
            if (LogsExist)
            {
                var LastLog = File.ReadAllText(ExectNameGeneratorProgramFilesDirectory(LOGGING_FILE));
                Journal = JsonConvert.DeserializeObject<JournalStructure>(LastLog);
            }
            else
            {
                Journal = new JournalStructure();

            }

            if (FilesExist)
            {
                var LastFiles = File.ReadAllText(ExectNameGeneratorProgramFilesDirectory(FILES_INFO));
                Files = JsonConvert.DeserializeObject<Dictionary<string, FileInfo>>(LastFiles);
            }
            else
            {
                Files = new Dictionary<string, FileInfo>();

            }
            if (FilesExist)
            {
                foreach (var VarFile in Files)
                {
                    if (!File.Exists(ExectNameGeneratorFilesDirectory(VarFile.Key))){
                        throw new Exception($"File { VarFile.Key} was deleted dy forigen");
                    }
                    else
                    {
                        var md5 = ComputeMD5Checksum(ExectNameGeneratorFilesDirectory(VarFile.Key));
                        if (md5 != VarFile.Value.LastHash)
                        {
                            throw new Exception($"File { VarFile.Key} was edited by forigen");
                        }
                    }


                }
            }
            Journal.GetStartedEvents();



        }
        public void CheckLogsCorectness()
        {



        }
        public string ExectNameGeneratorFilesDirectory(string FileName)
        {
            return $"{FilesDirectory}/{FileName}";
        }
        public string ExectNameGeneratorBackupDirectory(string FileName)
        {
            return $"{BackupFilesDirectory}/{FileName}";
        }
        public string ExectNameGeneratorProgramFilesDirectory(string FileName)
        {
            return $"{ProgramFilesDirectory}/{FileName}";
        }
        public void Create(String FileName)
        {
            var ExectFileName = ExectNameGeneratorFilesDirectory(FileName);
            if (File.Exists(ExectFileName))
            {
                throw new Exception($"File {FileName} Allready exists");
            };
            Journal.AddEvent(ExectFileName, EventTypes.Create, EventStatus.Started);

            var EventUuid = Journal.GetLastEvent(ExectFileName).Uuid;
            var Created = File.Create(ExectFileName);
            Created.Close();
            Files.Add(FileName, new FileInfo(ExectFileName, DateTime.Now, ComputeMD5Checksum(ExectFileName)));
            Journal.AddEvent(ExectFileName, EventTypes.Create, EventStatus.Done, "Nothing Strange",EventUuid);


        }
        public void Delete(String FileName)
        {
            var ExectFileName = ExectNameGeneratorFilesDirectory(FileName);
            if (!File.Exists(ExectFileName))
            {
                throw new Exception($"File {FileName} doesn't exist");
            };
            Journal.AddEvent(ExectFileName, EventTypes.Delete, EventStatus.Started);
            var EventUuid = Journal.GetLastEvent(ExectFileName).Uuid;


            File.Delete(ExectFileName);
            Files.Remove(FileName);
            Journal.AddEvent(ExectFileName, EventTypes.Delete, EventStatus.Done, "Nothing Strange", EventUuid);
        }
        
        public String Read(String FileName)
        {
            var ExectFileName = ExectNameGeneratorFilesDirectory(FileName);
            if (!File.Exists(ExectFileName))
            {
                throw new Exception($"File {FileName} doesn't exist");
            };
            return File.ReadAllText(ExectFileName);
        }
        public void Edit(String FileName, String NewText)
        {
            var ExectFileName = ExectNameGeneratorFilesDirectory(FileName);
            if (!File.Exists(ExectFileName))
            {
                throw new Exception($"File {FileName} doesn't exist");
            };
            Journal.AddEvent(ExectFileName, EventTypes.Edit, EventStatus.Started);
            var EventUuid = Journal.GetLastEvent(ExectFileName).Uuid;

            File.WriteAllText(ExectFileName, NewText);
            Files[FileName] = new FileInfo(FileName, DateTime.Now, ComputeMD5Checksum(ExectFileName));
            Journal.AddEvent(ExectFileName, EventTypes.Edit, EventStatus.Done, "Nothing Strange", EventUuid);

        }
        ~Dispatcher()
        {
            foreach (var VarFile in Files)
            {
                File.Copy(ExectNameGeneratorFilesDirectory(VarFile.Key), ExectNameGeneratorBackupDirectory(VarFile.Key),true);

            }
            var s = new JsonSerializerSettings();
            s.CheckAdditionalContent = true;
            File.WriteAllText(ExectNameGeneratorProgramFilesDirectory(LOGGING_FILE), JsonConvert.SerializeObject(Journal, s));

            File.WriteAllText(ExectNameGeneratorProgramFilesDirectory(FILES_INFO), JsonConvert.SerializeObject(Files, s));
            
        }
    }
}
