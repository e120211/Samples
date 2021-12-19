using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BaseLibrary;
using FluentFTP;
using UIModules;

namespace MyApp
{
    public class FZ44OrgProcessor : BaseTenderProcessor
    {
        private FtpFileManager ftp;
        private FtpListItem currentFile;
        private string _tmp;

        private string tempFolder
        {
            get
            {
                return ConfigurationManager.AppSettings["FileStorage"] + "Temp/" + _tmp + "/";
            }
        }

        public FZ44OrgProcessor()
        {
            BaseLibrary.Security.PasswordGenerator pw = new BaseLibrary.Security.PasswordGenerator();
            _tmp = "" + pw.Generate();
            LogType = 1;
            Source = 1001;
            ftp = new FtpFileManager(ConfigurationManager.AppSettings["FTPHost44"], ConfigurationManager.AppSettings["FTPLogin44"], ConfigurationManager.AppSettings["FTPPassword44"]);
        }

        #region Private methods
        public void Run()
        {
            if (!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
            }
            EventLog("Запущено обновление справочника организаций");
            Error = 0;
            process();
            Directory.Delete(tempFolder);
            EventLog("Завершено обновление справочника организаций (" + Error + " errors)!");
        }
        #endregion

        private void process()
        {
            FtpListItem[] list = ftp.GetDirectoryList("/fcs_nsi/nsiOrganization/");
            foreach (FtpListItem file in list)
            {
                if (Convert.ToBoolean(Convert.ToInt32(LogsBLL.IsFileProcessed(1, LogType, Source, file.Name).Result)))
                {
                    LogHelper.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - " + file.Name + " .......... skip!");
                    continue;
                }
                if (file.Type == FtpFileSystemObjectType.File)
                {
                    currentFile = file;
                    ftp.DownloadFile(file.FullName, tempFolder + file.Name);
                    try
                    {
                        using (ZipArchive archive = ZipFile.OpenRead(tempFolder + file.Name))
                        {
                            foreach (var item in archive.Entries)
                            {
                                byte[] buf = new byte[item.Length];
                                item.Open().Read(buf, 0, Convert.ToInt32(item.Length));
                                string xml = XmlHelper.XmlToJson(Encoding.UTF8.GetString(buf));
                                JObject obj = JObject.Parse(xml);
                                foreach (JObject org in obj["export"]["nsiOrganizationList"]["nsiOrganization"] as JArray)
                                {
                                    try
                                    {
                                        JObject contact = new JObject();
                                        contact["person"] = org["oos:contactPerson"] != null && org["oos:contactPerson"]["oos:lastName"] != null ? org["oos:contactPerson"]["oos:lastName"].ToString() + " " : "";
                                        contact["person"] += org["oos:contactPerson"] != null && org["oos:contactPerson"]["oos:firstName"] != null ? org["oos:contactPerson"]["oos:firstName"].ToString() + " " : "";
                                        contact["person"] += org["oos:contactPerson"] != null && org["oos:contactPerson"]["oos:middleName"] != null ? org["oos:contactPerson"]["oos:middleName"].ToString() : "";
                                        contact["person"] = contact["person"].ToString().Trim();
                                        contact["phone"] = org["oos:phone"] != null ? org["oos:phone"].ToString() : "";
                                        contact["fax"] = org["oos:fax"] != null ? org["oos:fax"].ToString() : "";
                                        contact["url"] = org["oos:url"] != null ? org["oos:url"].ToString() : "";
                                        contact["email"] = org["oos:email"] != null ? org["oos:email"].ToString() : "";
                                        contact["contactemail"] = org["oos:email"] != null ? org["oos:email"].ToString() : "";
                                        JObject timezone = new JObject();
                                        timezone["offset"] = org["oos:timeZone"] != null ? org["oos:timeZone"].ToString() : "";
                                        timezone["utcoffset"] = org["oos:timeZoneUtcOffset"] != null ? org["oos:timeZoneUtcOffset"].ToString() : "";
                                        timezone["name"] = org["oos:timeZoneOlson"] != null ? org["oos:timeZoneOlson"].ToString() : "";
                                        OrgEntry entry = new OrgEntry()
                                        {
                                            OrgNumber = org["oos:regNumber"].ToString(),
                                            Source = new JArray() { Source },
                                            Inn = org["oos:INN"] != null ? org["oos:INN"].ToString() : "0",
                                            Kpp = org["oos:KPP"] != null ? org["oos:KPP"].ToString() : "0",
                                            Ogrn = org["oos:OGRN"] != null ? org["oos:OGRN"].ToString() : "0",
                                            ShortName = org["oos:shortName"] != null ? org["oos:shortName"].ToString() : "",
                                            FullName = org["oos:fullName"] != null ? org["oos:fullName"].ToString() : "",
                                            FactAddress =
                                                (org["oos:factualAddress"] != null && org["oos:factualAddress"]["oos:country"] != null ? org["oos:factualAddress"]["oos:country"]["oos:countryFullName"] + ", " : "") +
                                                (org["oos:factualAddress"] != null && org["oos:factualAddress"]["oos:zip"] != null ? org["oos:factualAddress"]["oos:zip"] + ", " : "") +
                                                (org["oos:factualAddress"] != null && org["oos:factualAddress"]["oos:region"] != null ? org["oos:factualAddress"]["oos:region"]["oos:fullName"] + ", " : "") +
                                                (org["oos:factualAddress"] != null && org["oos:factualAddress"]["oos:area"] != null ? org["oos:factualAddress"]["oos:area"]["oos:fullName"] + ", " : "") +
                                                (org["oos:factualAddress"] != null && org["oos:factualAddress"]["oos:settlement"] != null ? org["oos:factualAddress"]["oos:settlement"]["oos:fullName"] + ", " : "") +
                                                (org["oos:factualAddress"] != null && org["oos:factualAddress"]["oos:shortStreet"] != null ? org["oos:factualAddress"]["oos:shortStreet"] + ", " : "") +
                                                (org["oos:factualAddress"] != null && org["oos:factualAddress"]["oos:building"] != null ? org["oos:factualAddress"]["oos:building"] : ""),
                                            PostAddress = org["oos:postalAddress"] != null ? org["oos:postalAddress"].ToString() : org["oos:oos:factualAddress"] != null ? org["oos:factualAddress"]["oos:addressLine"].ToString() : "",
                                            Actual = org["oos:actual"] != null && org["oos:actual"].ToString() == "true" ? 1 : 0,
                                            Contact = contact,
                                            Timezone = timezone
                                        };
                                        if (entry.FullName.ToLower().Contains("тестовая"))
                                        {
                                            continue;
                                        }
                                        DALResult result = NsiBLL.OrgCreate(1, entry);
                                        if (!Convert.ToBoolean(Convert.ToInt32(result.Result)))
                                        {
                                            throw new Exception("Error creating record: " + result.Error);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ErrorLog(item.Name, currentFile.Name, "FZ44OrgProcessor:" + ex.Message);
                                    }
                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        ErrorLog(file.Name, file.Name, "Error processing file: " + ex.Message);
                    }
                    LogHelper.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - " + file.Name + " .......... done!");
                    LogsBLL.SetFileProcessed(1, LogType, Source, file.Name, file.FullName);
                    File.Delete(tempFolder + file.Name);
                }
            }
        }
    }
}