using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using System.Web.Services;

namespace WebRole1
{
    /// <summary>
    /// Summary description for WebService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class WebService : System.Web.Services.WebService
    {
        public static TrieStuff s = new TrieStuff();
        /// <summary>
        /// This method downloads the wiki data from blob
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public bool DownloadFileFromBlob()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("hw2");

            CloudBlockBlob blockBlob = container.GetBlockBlobReference("new_wiki_clean");
            if (blockBlob.Exists())
            {
                //string url = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToString() + "\\new_wiki_clean";
                string filePath = HostingEnvironment.ApplicationPhysicalPath + "new_wiki_clean";
                using (var fileStream = System.IO.File.OpenWrite(@filePath))
                //using (var fileStream = System.IO.File.OpenWrite(@"C:\Users\Chun-Cheng Chang\Downloads\test123\file52"))
                {
                    blockBlob.DownloadToStream(fileStream);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        
        /// <summary>
        /// Reads the wiki data line by line and builds a Trie structure
        /// </summary>
        [WebMethod]
        public string buildTrieStructure()
        {
            float memCheck_base = GetAzureMem();
            float memCheck_now;
            
            //string url1 = "C:\\Users\\Chun-Cheng Chang\\Downloads\\test123\\wiki_data";
            //string url2 = "C:\\Users\\Chun-Cheng Chang\\Downloads\\test123\\new_wiki_clean";
            //string url = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToString() + "\\new_wiki_clean";
            string filePath = HostingEnvironment.ApplicationPhysicalPath + "new_wiki_clean";
            using (StreamReader sr = new StreamReader(@filePath))
            //using (StreamWriter sw = new StreamWriter(@url2))
            {
                sr.ReadLine();
                int line_count = 0;
                while (sr.EndOfStream == false)
                {
                    //if (line_count >5000)
                    //{
                    //    memCheck_now = GetAzureMem();
                    //    //if (memCheck_now / memCheck_base < 0.5)
                    //    if (memCheck_now < 500)
                    //    {
                            
                    //        break; 
                    //    }
                    //    line_count = 0;
                    //}
                    //line_count++;
                    memCheck_now = GetAzureMem();
                    if (memCheck_now < 300) { break; }
                    string line = sr.ReadLine();
                    if (Regex.IsMatch(line, "^[a-zA-Z_]+$") == true)
                    {
                        //sw.WriteLine(line);
                        s.addToTrie(line.ToLower()); 
                    }
                }
            }
            return "finished building Trie";
        }

        /// <summary>
        /// Returns result from Trie traversal
        /// </summary>
        /// <param name="input">Ajax keystroke string input</param>
        /// <returns>result list in JSON format</returns>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string getSearchResult(string input)
        {
            buildTrieStructure();
            List<string> result = new List<string>();
            result = s.searchPrefix(input);
            return new JavaScriptSerializer().Serialize(result);
        }


        /// <summary>
        /// Notifies how much memory is available in Azure
        /// </summary>
        private PerformanceCounter memProcess = new PerformanceCounter("Memory", "Available MBytes");
        [WebMethod]
        public float GetAzureMem()
        {
            float memUsage = memProcess.NextValue();
            return memUsage;
        }

        [WebMethod]
        public string testFilePath()
        {
            string filePath = HostingEnvironment.ApplicationPhysicalPath + "new_wiki_clean";
            return filePath;
        }

    
    }//Closes Class

}//Closes Namespace
