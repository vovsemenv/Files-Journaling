using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Windows;

namespace Journaling
{
    public class FileInfo
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

    public class Dispatcher
    {
        public const string LOGGING_FILE = "LOGS.json";

        public const string FILES_INFO = "FILES.json";

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
            MessageBoxResult res = MessageBoxResult.Yes ;
            var LogsExist = File.Exists(ExectNameGeneratorProgramFilesDirectory(LOGGING_FILE));
            var FilesExist = File.Exists(ExectNameGeneratorProgramFilesDirectory(FILES_INFO));
            //if (LogsExist && FilesExist)
            //{
            //    res = MessageBox.Show("Найденны данные предыдущей сесии. Загрузить их?", "Старая сессия", MessageBoxButton.YesNo);

            //}

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
            if (res == MessageBoxResult.No)
            {
                Directory.Delete(BackupFilesDirectory, true);
                Directory.Delete(FilesDirectory, true);
                File.Delete(ExectNameGeneratorProgramFilesDirectory(LOGGING_FILE));
                File.Delete(ExectNameGeneratorProgramFilesDirectory(FILES_INFO));
                Journal = new JournalStructure();
                Files = new Dictionary<string, FileInfo>();
                Directory.CreateDirectory(BackupFilesDirectory);
                Directory.CreateDirectory(FilesDirectory);
                Journal.AddEvent(LOGGING_FILE, EventTypes.OpenProgramAsNew, EventStatus.Started, "Первый запуск работы журналирвоания");
                Journal.AddEvent(LOGGING_FILE, EventTypes.OpenProgramAsNew, EventStatus.Done, "Запуск ситемы журналирвоания завершен удачно", Journal.GetLastEvent(LOGGING_FILE).Uuid);
            }
            else
            {
                Journal.AddEvent(LOGGING_FILE, EventTypes.OpenProgramFromSave, EventStatus.Started, "Начало запска программы");
                Journal.AddEvent(LOGGING_FILE, EventTypes.OpenProgramFromSave, EventStatus.Done, "Запуск программы завершен", Journal.GetLastEvent(LOGGING_FILE).Uuid);
            }
            if (FilesExist)
            {
                foreach (var VarFile in Files)
                {
                    try { 
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
                    catch
                    {
                        Journal.AddEvent(VarFile.Key, EventTypes.RepairedFromBackup, EventStatus.Started, "Найден повреженный файл");
                        Journal.AddEvent(VarFile.Key, EventTypes.RepairedFromBackup, EventStatus.Done, "Найден повреженный файл,подтверждение", Journal.GetLastEvent(VarFile.Key).Uuid);
                        MessageBox.Show($"Файл {VarFile.Key} был изменен извне, изменеиня будут сброшены и восстановится версия на момент прошлой сесиии {VarFile.Value.LastEdit.ToString()}");
                        File.Copy(ExectNameGeneratorBackupDirectory(VarFile.Key), ExectNameGeneratorFilesDirectory(VarFile.Key),true);
                        Journal.AddEvent(VarFile.Key, EventTypes.RepairedFromBackup, EventStatus.Started,"Началось восстановление из бэкапа");
                        Journal.AddEvent(VarFile.Key, EventTypes.RepairedFromBackup, EventStatus.Done, "Восставновление из бэкапа закончено", Journal.GetLastEvent(VarFile.Key).Uuid);
                    }


                }
            }
            


            
            CheckLogsCorectness();



        }
        public void Summs()
        {
            foreach (var VarFile in Files)
            {
                try
                {
                    if (!File.Exists(ExectNameGeneratorFilesDirectory(VarFile.Key)))
                    {
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
                catch
                {
                    Journal.AddEvent(VarFile.Key, EventTypes.RepairedFromBackup, EventStatus.Started, "Найден повреженный файл");
                    Journal.AddEvent(VarFile.Key, EventTypes.RepairedFromBackup, EventStatus.Done, "Найден повреженный файл,подтверждение", Journal.GetLastEvent(VarFile.Key).Uuid);
                    MessageBox.Show($"Файл {VarFile.Key} был изменен извне, изменеиня будут сброшены и восстановится версия на момент прошлой сесиии {VarFile.Value.LastEdit.ToString()}");
                    File.Copy(ExectNameGeneratorBackupDirectory(VarFile.Key), ExectNameGeneratorFilesDirectory(VarFile.Key), true);
                    Journal.AddEvent(VarFile.Key, EventTypes.RepairedFromBackup, EventStatus.Started, "Началось восстановление из бэкапа");
                    Journal.AddEvent(VarFile.Key, EventTypes.RepairedFromBackup, EventStatus.Done, "Восставновление из бэкапа закончено", Journal.GetLastEvent(VarFile.Key).Uuid);
                }


            }
        }
        public string getlast(string str)
        {

            return str.Substring(str.LastIndexOf("/")+1);
        }
        public void CheckLogsCorectness()
        {
            var wrongs = Journal.GetStartedEvents();
            foreach (var err in wrongs)
            {
                Journal.AddEvent(err.FileName, EventTypes.OpenProgramFromSave, EventStatus.Started, "Найдена незавершенная операция");
                Journal.AddEvent(err.FileName, EventTypes.OpenProgramFromSave, EventStatus.Done, "Найдена незавершенная операция, подтверждение",Journal.GetLastEvent(err.FileName).Uuid);
                var rs = MessageBox.Show($"Не завершена операция {err.Type} над файлом {err.FileName}. Завершить ее?", "", MessageBoxButton.YesNo);
                if (rs == MessageBoxResult.Yes)
                {
                    switch (err.Type)
                    {
                        case EventTypes.Create:
                            {
                                
                                Journal.AddEvent(err.FileName, err.Type, EventStatus.Repaired, "", err.Uuid);
                                Create(getlast(err.FileName));
                                Journal.AddEvent(err.FileName, EventTypes.RepairedFromBackup, EventStatus.Started, "Продолжение операции создания файла");

                                Journal.AddEvent(err.FileName, EventTypes.RepairedFromBackup, EventStatus.Started, "Завершение продолжения операции создания файла", Journal.GetLastEvent(err.FileName).Uuid);
                                break;

                            }
                        case EventTypes.Delete:
                            {
                                Journal.AddEvent(err.FileName, err.Type, EventStatus.Repaired, "", err.Uuid);
                                
                                Delete(getlast(err.FileName));
                                Journal.AddEvent(err.FileName, EventTypes.RepairedFromBackup, EventStatus.Started, "Продолжение операции удаления файла");

                                Journal.AddEvent(err.FileName, EventTypes.RepairedFromBackup, EventStatus.Started, "Завершение продолжения операции удаления файла", Journal.GetLastEvent(err.FileName).Uuid);
                                break;
                            }
                        case EventTypes.Edit:
                            {
                                Journal.AddEvent(err.FileName, err.Type, EventStatus.Repaired, err.Addition, err.Uuid);
                                Edit(getlast(err.FileName), err.Addition);
                                Journal.AddEvent(err.FileName, EventTypes.RepairedFromBackup, EventStatus.Started, "Продолжение операции изменения файла");
                                Journal.AddEvent(err.FileName, EventTypes.RepairedFromBackup, EventStatus.Started, "Завершение продолжения операции изменения файла", Journal.GetLastEvent(err.FileName).Uuid);
                                break;
                            }
                    }
                }
                else
                {
                    switch (err.Type)
                    {
                        case EventTypes.Create:
                            {

                                Journal.AddEvent(err.FileName, err.Type, EventStatus.Declined, "", err.Uuid);
                                
                                break;
                            }
                        case EventTypes.Delete:
                            {

                                Journal.AddEvent(err.FileName, err.Type, EventStatus.Declined, "", err.Uuid);
                                
                                break;
                            }
                        case EventTypes.Edit:
                            {

                                Journal.AddEvent(err.FileName, err.Type, EventStatus.Declined, err.Addition, err.Uuid);
                                
                                break;
                            }
                    }
                }
            }

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
        public void Create(String FileName,int freeze=0)
        {
            try { 
            var ExectFileName = ExectNameGeneratorFilesDirectory(FileName);
            if (File.Exists(ExectFileName))
            {
                throw new Exception($"File {FileName} Allready exists");
            };
            Journal.AddEvent(ExectFileName, EventTypes.Create, EventStatus.Started);
            File.WriteAllText(ExectNameGeneratorProgramFilesDirectory(LOGGING_FILE), JsonConvert.SerializeObject(Journal));

            File.WriteAllText(ExectNameGeneratorProgramFilesDirectory(FILES_INFO), JsonConvert.SerializeObject(Files));

            Thread.Sleep(freeze);
            var EventUuid = Journal.GetLastEvent(ExectFileName).Uuid;
            var Created = File.Create(ExectFileName);
            Created.Close();
            Files.Add(FileName, new FileInfo(ExectFileName, DateTime.Now, ComputeMD5Checksum(ExectFileName)));
            Journal.AddEvent(ExectFileName, EventTypes.Create, EventStatus.Done, "Nothing Strange",EventUuid);
            MessageBox.Show("Done");
            }
            catch
            {
                var res = MessageBox.Show($"Такой файл уже существует Создать файл (Копия){FileName}", "Question", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.Yes)
                {
                    this.Create($"(Копия){FileName}");                
            }
        }
            foreach (var VarFile in Files)
            {
                File.Copy(ExectNameGeneratorFilesDirectory(VarFile.Key), ExectNameGeneratorBackupDirectory(VarFile.Key), true);

            }
        }
        public void Delete(String FileName, int freeze = 0)
        {
            var ExectFileName = ExectNameGeneratorFilesDirectory(FileName);
            if (!File.Exists(ExectFileName))
            {
                throw new Exception($"File {FileName} doesn't exist");
            };
            Journal.AddEvent(ExectFileName, EventTypes.Delete, EventStatus.Started);
            var EventUuid = Journal.GetLastEvent(ExectFileName).Uuid;
            File.WriteAllText(ExectNameGeneratorProgramFilesDirectory(LOGGING_FILE), JsonConvert.SerializeObject(Journal));

            File.WriteAllText(ExectNameGeneratorProgramFilesDirectory(FILES_INFO), JsonConvert.SerializeObject(Files));

            Thread.Sleep(freeze);


            File.Delete(ExectFileName);
            File.Delete(ExectNameGeneratorBackupDirectory(FileName));
            Files.Remove(FileName);
            Journal.AddEvent(ExectFileName, EventTypes.Delete, EventStatus.Done, "Nothing Strange", EventUuid);
            foreach (var VarFile in Files)
            {
                File.Copy(ExectNameGeneratorFilesDirectory(VarFile.Key), ExectNameGeneratorBackupDirectory(VarFile.Key), true);

            }
        }
        
        public String Read(String FileName, int freeze = 0)
        {
            var ExectFileName = ExectNameGeneratorFilesDirectory(FileName);
            if (!File.Exists(ExectFileName))
            {
                throw new Exception($"File {FileName} doesn't exist");
            };
            return File.ReadAllText(ExectFileName);
        }
        public void Edit(String FileName, String NewText,int freeze=0)
        {
            var ExectFileName = ExectNameGeneratorFilesDirectory(FileName);
            if (!File.Exists(ExectFileName))
            {
                throw new Exception($"File {FileName} doesn't exist");
            };
            Journal.AddEvent(ExectFileName, EventTypes.Edit, EventStatus.Started,NewText);
            var EventUuid = Journal.GetLastEvent(ExectFileName).Uuid;
            File.WriteAllText(ExectNameGeneratorProgramFilesDirectory(LOGGING_FILE), JsonConvert.SerializeObject(Journal));

            File.WriteAllText(ExectNameGeneratorProgramFilesDirectory(FILES_INFO), JsonConvert.SerializeObject(Files));

            Thread.Sleep(freeze);


            File.WriteAllText(ExectFileName, NewText);
            Files[FileName] = new FileInfo(FileName, DateTime.Now, ComputeMD5Checksum(ExectFileName));
            Journal.AddEvent(ExectFileName, EventTypes.Edit, EventStatus.Done, "Nothing Strange", EventUuid);
            foreach (var VarFile in Files)
            {
                File.Copy(ExectNameGeneratorFilesDirectory(VarFile.Key), ExectNameGeneratorBackupDirectory(VarFile.Key), true);

            }
        }
        ~Dispatcher()
        {
            foreach (var VarFile in Files)
            {
                File.Copy(ExectNameGeneratorFilesDirectory(VarFile.Key), ExectNameGeneratorBackupDirectory(VarFile.Key),true);

            }
            Journal.AddEvent(LOGGING_FILE, EventTypes.Edit, EventStatus.Done, "Начало закрытия программы");
            Journal.AddEvent(LOGGING_FILE, EventTypes.Edit, EventStatus.Done, "Закрытие программы завершено",Journal.GetLastEvent(LOGGING_FILE).Uuid);
            File.WriteAllText(ExectNameGeneratorProgramFilesDirectory(LOGGING_FILE), JsonConvert.SerializeObject(Journal));

            File.WriteAllText(ExectNameGeneratorProgramFilesDirectory(FILES_INFO), JsonConvert.SerializeObject(Files));
            
        }
    }
}
