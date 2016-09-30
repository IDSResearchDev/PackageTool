using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Rnd.Common
{
    public class Utilities
    {

        /// <summary>
        ///Helper method to open any application 
        /// </summary>
        /// <param name="executable">The full path of the application</param>
        public void Run(string executable)
        {
            Process.Start(executable);
        }


        /// <summary>
        /// Check if an app is currenlty running
        /// </summary>
        /// <param name="appname">The application name</param>
        /// <returns></returns>
        public bool IsRunning(string appname)
        {
            return true;
        }
        /// <summary>
        /// returns string location of selected folder
        /// </summary>
        /// <returns></returns>
        public string BrowseFolder()
        {
            var folderDialog = new FolderBrowserDialog();
            folderDialog.ShowDialog();
            return folderDialog.SelectedPath;
        }
        /// <summary>
        /// Browse folder and select a value path.
        /// First item object can be cast to Dialogresult and second item is selected value path
        /// </summary>
        /// <returns>
        ///     returns object which is a dialog result and string selected folder path
        /// </returns>
        public Tuple<DialogResult, string> FolderDialog ()
        {
            var folderDialog = new FolderBrowserDialog();
            return new Tuple<DialogResult, string>(folderDialog.ShowDialog(), folderDialog.SelectedPath);
        }

        /// <summary>
        /// Represents an XML document.
        /// </summary>
        /// <param name="path">Folder path of an XML document.</param>
        /// <returns>Returns an XML document base on specified path.</returns>
        public XDocument XdocReadXml(string path)
        {
            var newdoc = XDocument.Load(path);
            return newdoc; 

        }
        /// <summary>
        /// Represents an XML document.
        /// </summary>
        /// <param name="doc">XML document.</param>
        /// <returns>Returns a root element of a specified XML document.</returns>
        public XElement GetRootElement(XDocument doc)
        {
            return doc.Root;
        }
        /// <summary>
        /// Provides single element on a specified XML document.
        /// </summary>
        /// <param name="path">Path of an XML document.</param>
        /// <param name="searchXElementName">Element being search under root node.</param>
        /// <returns>Returns innertext of XML element in string value.</returns>
        public string GetSingleXElementXml(string path, string searchXElementName)
        {
            var value = string.Empty; 
            if (CheckIfFileExists(path))
            {
                var xmldoc = XdocReadXml(path);
                var rootxml = GetRootElement(xmldoc);
                
                foreach (var elements in rootxml.Elements())
                {
                    foreach (var content in elements.Elements().Where(content => content.Name == searchXElementName))
                    {
                        value = content.Value;
                    }
                }
            }
            return value;
        }
        /// <summary>
        /// Write something on a text files e.g.(.txt, .ini)
        /// </summary>
        /// <param name="textfile">The full path of the text file</param>
        /// <param name="value">Text value to write</param>
        public void WriteText(string textfile,string value)
        {
            
        }

        /// <summary>
        /// Helper method to create a directory anywhere if path is specified
        /// </summary>
        /// <param name="dir"></param>
        public void CreateDirectory(string dir)
        {            
            if (!CheckIfDirectoryExists(dir))
                Directory.CreateDirectory(dir);
        }

        /// <summary>
        /// Get files on a specified path and extension
        /// </summary>
        /// <param name="path"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public FileInfo[] GetFiles(string path, string extension)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            FileInfo[] files = dir.GetFiles("*." + extension);
            return files;
        }

        public void GetSubDir(string dir)
        {
            var directories = Directory.GetDirectories(dir);
            foreach (var directory in directories)
            {
                LoadDirectories(directory);
            }
        }

        public void LoadDirectories(string dir)
        {
            string[] subdirectories = Directory.GetDirectories(dir);
            foreach (var subdirectory in subdirectories)
            {
                LoadDirectories(subdirectory);
            }
        }

        /// <summary>
        /// Returns string of a specific file by paramenters given (path of file, name of file and it's extension).
        /// </summary>
        /// <param name="path">string directory where file is located.</param>
        /// <param name="filename">Specific file name.</param>
        /// <param name="extension">The type of file being created (e.g. .ini, .txt, .doc, etc.).
        ///     The dot (.) symbol is required in this parameter to specify it's file type.</param>
        /// <returns></returns>
        public string PathFilename(string path, string filename, string extension)
        {
            return (extension[0] == '.')
                ? Path.Combine(path, filename + extension)
                : Path.Combine(path, filename + "." + extension);
        }
        /// <summary>
        /// Checks if directory exists
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public bool CheckIfDirectoryExists(string dir)
        {
            return (Directory.Exists(dir));
        }

        /// <summary>
        /// Check if files are not existing returns false boolean value.
        /// </summary>
        /// <param name="pathfilename">String full path and filename</param>
        /// <returns></returns>
        public bool CheckIfFileExists(string pathfilename)
        {
            return (File.Exists(pathfilename));
        }        
        /// <summary>
        /// Create any type of file in a given location
        /// </summary>
        /// <param name="path">The location where the file will be created</param>
        /// <param name="filename"></param>
        /// <param name="extension">The kind of file to be create e.g. .ini, .txt, .doc)</param>
        public void CreateFile(string path,string filename, string extension)
        {
            var ext = extension;
            if (extension[0] != '.') extension = string.Concat(".", ext);

            File.Create(Path.Combine(path,filename+extension)).Close();
        }        
        /// <summary>
        /// Initialize new instance of StreamWriter and StreamReader of a specified file.
        /// Read lines of a file and append the value on top most line on the given params.
        /// </summary>
        /// <param name="filename">Full path and filename.</param>
        /// <param name="appendvalue">Value to write.</param>
        /// <param name="existingvalue">Check and append existing line value.</param>
        public static void ReadWriteLines(string filename, string appendvalue, string existingvalue)
        {
            var tempfile = Path.GetTempFileName();
            using (var writer = new StreamWriter(tempfile))
            using (var reader = new StreamReader(filename))
            {
                writer.WriteLine(appendvalue);
                while (!reader.EndOfStream)
                {
                    var readLine = reader.ReadLine();
                    if (readLine != null)
                    {
                        if (!readLine.Contains(existingvalue))
                        {
                            writer.WriteLine(readLine);
                        }
                    }
                }

            }
            File.Copy(tempfile, filename, true);
            
        }
        /// <summary>
        /// Get the location of appdata folder
        /// </summary>
        public string Appdata
        {
            get { return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData); }
        }
        /// <summary>
        /// Get the location of local appdata folder %localappdata%
        /// </summary>
        public string LocalAppData
        {
            get { return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData); }
        }
        /// <summary>
        /// Read all text of a specified filename
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public string[] ReadAllLines(string filename)
        {            
            if(CheckIfFileExists(filename))
            {
                return File.ReadAllLines(filename);                                
            }
            else
            {
                return null;
            }
            
        }

        /// <summary>
        /// Get the Server Name in the file .This_is_multiuser_model
        /// located in the specified param
        /// </summary>
        /// <param name="modelPath"></param>
        /// <returns></returns>
        public string GetServerName(string modelPath)
        {
            var temp = ReadAllLines(modelPath + "\\.This_is_multiuser_model");
            string computerName = string.Empty, serverName = string.Empty, system = string.Empty;
            if (temp != null)
            {
                foreach(string x in temp)
                {                   
                    if(x.Contains("tcpip:"))
                    {
                        var n = x.Replace("tcpip:", string.Empty).Split(',');
                        computerName = n[0]+";";
                    }
                    if (x.Contains("_1238.db"))
                    {
                        var str = x.Split('\\');
                        system = str[2] == "system32" ? "32" : "64";
                        var n = str[str.Length - 1].Replace("tcpip_",string.Empty).Replace("_1238.db",string.Empty);                        
                        serverName = n+";";
                    }                    
                }
            }
            return computerName + serverName + system;
        }

        /// <summary>
        /// Create a new file with specified text
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="text"></param>
        public void CreateFileWithText(string filename, string text)
        {
            using (StreamWriter sw = File.CreateText(filename))
            {
                sw.Write(text);
            }  
        }

        /// <summary>
        /// Set tekla server details in MultiUser file (.This_is_multiuser_model)
        /// </summary>
        /// <param name="modelPath"></param>
        /// <param name="computerName"></param>
        /// <param name="serverName"></param>
        /// <param name="port"></param>
        public void SetServerName(string modelPath, string computerName, string serverName, string port, string targetArch)
        {
            var filename = modelPath + "\\.This_is_multiuser_model";

            string multiUserTemplate = "tcpip:{0},{1}\nC:\\Windows\\{2}\\TeklaStructuresServer\\tcpip_{3}_{4}.db\nThis is a multiuser model.\nThis model should not be opened with single user mode!";

            CreateFileWithText(filename, string.Format(multiUserTemplate, computerName, port, targetArch == "32" ? "system32" : "SysWOW64", serverName, port));
        }        

        /// <summary>
        /// Check if the model is a Multi-User
        /// Check if the .This_is_multiuser_model file exists
        /// </summary>
        /// <param name="modelPath"></param>
        /// <returns></returns>
        public bool IsMultiUser(string modelPath)
        {
            var filename = modelPath + "\\.This_is_multiuser_model";

            return CheckIfFileExists(filename);
        }

        /// <summary>
        /// Load and Deserialized bin file
        /// </summary>
        /// <typeparam name="T">Cast type</typeparam>
        /// <param name="fileFullPath">Loading path loaction</param>
        /// <returns></returns>
        public T DeserializeBinFile<T>(string fileFullPath) where T : new()
        {
            T obj = default(T);
            if (File.Exists(fileFullPath))
            {
                using (Stream stream = File.Open(fileFullPath, FileMode.Open))
                {
                    BinaryFormatter bin = new BinaryFormatter();

                    obj = (T)bin.Deserialize(stream);
                }
            }
            return obj;
        }

        /// <summary>
        /// Save and Serialize bin file
        /// </summary>
        /// <param name="fileFullPath">Saving path location</param>
        /// <param name="obj">Serializable obj</param>
        public void SerializeBinFile(string fileFullPath, object obj)
        {
            using (Stream stream = File.Open(fileFullPath, FileMode.Create))
            {
                BinaryFormatter bin = new BinaryFormatter();
                bin.Serialize(stream, obj);
            }
        }
        /// <summary>
        /// Create a Log file
        /// </summary>
        /// <param name="logMessage">Message Log</param>
        /// <param name="path">Location of the Log file</param>
        /// <param name="filename">File name without extension file</param>
        public void CreateLogFile(string logMessage, string path, string filename)
        {
            this.CreateDirectory(path);
            string fullFilePath = Path.Combine(path, filename + ".log");
            using (StreamWriter w = File.AppendText(fullFilePath))
            {
                w.Write("Log Entry : ");
                w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                    DateTime.Now.ToLongDateString());
                w.WriteLine("  :{0}", logMessage);
                w.WriteLine("-------------------------------\r\n");
            }
        }

        /// <summary>
        /// Copy files to another directory
        /// </summary>
        /// <param name="source">Directory of the files to copy</param>
        /// <param name="destination"></param>
        public void CopyFilesToLocation(string source, string destination, string extension)
        {
            if (Directory.Exists(destination))
            {
                var util = new Common.Utilities();
                var items = util.GetFiles(source, extension);

                foreach (var item in items)
                {
                    File.Copy(item.FullName, Path.Combine(destination, item.Name), true);
                }
            }
        }

        /// <summary>
        /// Update a text file with specified attribute value
        /// </summary>
        /// <param name="filePath">File location</param>
        /// <param name="delimiter"></param>
        /// <param name="attribute">keyword to search</param>
        /// <param name="newValue">New value</param>
        public void UpdateTextFileValue(string filePath, char delimiter, string attribute, string newValue)
        {
            var content = this.ReadAllLines(filePath);
            int index = 0;

            if(content != null)
            {
                foreach (var item in content)
                {
                    var i = item.Split(delimiter);
                    if (i[0].Replace(" ", "").ToLower().Equals(attribute.ToLower()))
                    {
                        content[index] = i[0] + delimiter + newValue;
                        break;
                    }
                    index++;
                }
                File.WriteAllLines(filePath, content);
            }
        }

        public string GetTextFileValue(string filePath, char delimiter, string attribute)
        {
            var content = this.ReadAllLines(filePath);
            string value = string.Empty;

            if (content != null)
            {
                foreach (var item in content)
                {
                    var i = item.Split(delimiter);
                    if (i[0].Replace(" ", "").ToLower().Equals(attribute.ToLower()))
                    {
                        value = i[1].Trim();
                        break;
                    }
                }                
            }
            return value;
        }
    }
}
