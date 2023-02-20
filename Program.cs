using System;
using System.Collections.Generic;
using System.IO;

namespace DeOSux
{
    class ConsoleFiller
    {
        string consoletext;
        public ConsoleFiller()
        {
            consoletext = "";
            Console.Clear();
        }
        public void Write(string s, bool newline = false)
        {
            if (newline) s += '\n';
            consoletext += s;
            Console.Write(s);
        }
        public string ReadLine(File file)
        {
            string text = file.Content;
            ConsoleKeyInfo ck;
            while (true)
            {
                Console.Clear();
                Console.Write(consoletext + text);
                switch ((ck = Console.ReadKey()).Key)
                {
                    case ConsoleKey.Enter:
                        consoletext += text;
                        Console.Clear();
                        file.Content = consoletext;
                        return text;
                    case ConsoleKey.Backspace:
                        int l = text.Length;
                        if (l > 0) text = text.Substring(0,l-1);
                        break;
                    default:
                        text += ck.KeyChar;
                        break;
                }
            }
        }
        public ConsoleKey Wait()
        {
            return Console.ReadKey().Key;
        }
    }

    class Program
    {
        public static User admin = new User()
        {
            Username = "admin",
            Password = "adminpass",            
            Level = Mandates.Supervisor           
        };
        static public string logpath = @"C:\Users\SVS1984\source\repos\DeOSux\DeOSux\bin\buf.log";
        static public string fileInfoPath = @"C:\Users\SVS1984\source\repos\DeOSux\DeOSux\bin\file.ini";
        static public string accessPath = @"C:\Users\SVS1984\source\repos\DeOSux\DeOSux\bin\d_access_matrix.ini";
        static public string accessPathM = @"C:\Users\SVS1984\source\repos\DeOSux\DeOSux\bin\m_access_matrix.ini";
        static string[] CurrCommand = { "" };
        static public User CurUser;
        static public Mandate CurRole;
        static public string Mode;
        static public List<User> Users = new List<User>();
        static public List<File> Files = new List<File>();
        static bool IsLoginExist = false;

        static public void FormAccessMatrix()
        {
            
            using (StreamWriter sw = new StreamWriter(accessPath))
            {
                sw.Write("\t\t");
                foreach (File file in Files)
                {
                    sw.Write("{0,-10}", file.Name);
                }
                sw.WriteLine();
                foreach (User user in Users)
                {
                    sw.Write("{0,-10}", user.Username);
                    foreach (File file in Files)
                    {
                        sw.Write("{0,-10}", file.Access[user.Username]);
                    }
                    sw.WriteLine();
                }
            }
        }
        static public void ReadAccessMatrix()
        {
            using (StreamReader sr = new StreamReader(accessPath))
            {
                string[] head = sr.ReadLine().Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (head.Length == 0)
                {
                    Console.WriteLine("Error: access matrix can't be read correctly");                    
                }
                else
                while (!sr.EndOfStream)
                {
                   string[] u = sr.ReadLine().Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    
                   foreach (User user in Users)
                   {
                      if (user.Username == u[0] && user.Username != "admin")
                      {
                           int fileIndex = 1;
                           foreach (File file in Files)
                           {
                                    file.Access.Add(user.Username, u[fileIndex]);
                                    fileIndex++;
                           }
                      }
                   }
                }
            }
        }

        static User StartAutorisation() 
        {
            Console.Write("Login: ");
            string CurUser = Console.ReadLine();
            User u = null;
            foreach(User user in Users)
            {
                if (CurUser == user.Username)
                {
                    IsLoginExist = true;
                    Console.Write("Password: ");
                    string password = InputPassword();
                    if (password != user.Password)
                    {
                        Console.WriteLine($"\nPassword is not correct!\nRetry authenification(Y/N)?");
                        string Flag = Console.ReadLine();
                        while (Flag != "Y" && Flag != "N")
                        {
                            Console.WriteLine("Incorrect answer! Retry authentification?(Y/N)");
                            Flag = Console.ReadLine();
                            
                        }
                        if (Flag == "Y")
                            return StartAutorisation();
                        else
                            Environment.Exit(0);
                    }
                    else
                    {
                        Console.Clear();
                        u = user;                        
                        ChooseMode();                         
                        PrintUM(u);
                        
                    }
                }                
            }           
            if (!IsLoginExist)
            {
                Console.WriteLine($"\nUser doesn't exist!\nRetry authenification(Y/N)?");
                string Flag = Console.ReadLine();
                while (Flag != "Y" && Flag != "N")
                {
                    Console.WriteLine("Incorrect answer! Retry authentification?(Y/N)");
                    Flag = Console.ReadLine();
                    if (Flag == "Y")
                        StartAutorisation();
                    else
                        Environment.Exit(0);
                }
                if (Flag == "Y")
                    return StartAutorisation();
                else
                    Environment.Exit(0);
            }
            
            return u;
        }
        


        static public void PrintUM(User user, bool writeToLog = true)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"{user.Username}-{Mode}@deosux:~$ ");
            Console.ForegroundColor = ConsoleColor.White;
            if (writeToLog)
            {
                using (StreamWriter sw = new StreamWriter(logpath, true))
                {
                    sw.Write("|UM| ");
                }
            }
        }
        static public string InputPassword()
        {
            string Password = "";
            while (true)
            {

                var key = Console.ReadKey(true);//не отображаем клавишу - true

                if (key.Key == ConsoleKey.Enter) break; //enter - выходим из цикла
                if (key.Key == ConsoleKey.Backspace)
                {
                    if (Password.Length > 0)
                        Password = Password.Remove(Password.Length - 1);
                }
                else
               {
                    //Console.Write("*");//рисуем звезду вместо нее
                    Password += key.KeyChar; //копим в пароль символы
                }

            }
            return Password;
        }
        static string[] ParseCommand(string Command)
        {
            string[] ParsedCommand = Command.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return ParsedCommand;
        }
        static public void LoadUsers()
        {            
            using (StreamReader sr = new StreamReader(ComInt.usersIni))
            {
                
                    while (!sr.EndOfStream)
                    {
                        User us = null;
                        string[] u = sr.ReadLine().Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        if (u.Length > 0)
                        {
                             us = new User()
                             {
                                 Username = u[0],
                                 Password = u[1],
                                 Level = Mandates.DetectMandate(u[2])
                        }    ;
                             Users.Add(us);
                        }
                    }                           
               
            }
        }

        static public void LoadFiles()
        {
            using (StreamReader sr = new StreamReader(fileInfoPath))
            {
                while (!sr.EndOfStream)
                {
                    string[] u = sr.ReadLine().Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (u.Length > 0)
                    {
                        File file = new File()
                        {
                            Name = u[0],
                            AccessLevel = Convert.ToInt32(u[2])
                        };
                        foreach (User user in Users)
                        {
                            if (u[1] == user.Username)
                                file.Creator = user;
                        }
                        if (System.IO.File.Exists(ComInt.HomeDir + u[0] + ".txt"))
                        {
                            using (StreamReader sr2 = new StreamReader(ComInt.HomeDir + u[0] + ".txt"))
                            {
                                file.Content = sr2.ReadToEnd();
                            }
                        }
                        else
                        {
                            System.IO.File.CreateText(ComInt.HomeDir + u[0] + ".txt");
                            file.Content = "";
                        }
                        Files.Add(file);
                    }
                }
            }
        }

        static public void WriteToBuf(string output, bool endfile = true)
        {
            
            using (StreamWriter sw = new StreamWriter(logpath, true))
            {
                if (endfile == true)
                    sw.Write(output + '\n');
                else sw.Write(output);
            }
        }
        static public void ClearBuf()
        {
            if (System.IO.File.Exists(logpath))
            {
                System.IO.File.Delete(logpath);
            }            
            System.IO.File.Create(logpath).Close();
        }
        static public void ChooseMode(bool exit = true)
        {
            Console.WriteLine("Choose the mode of access control");
            Console.WriteLine("Press D for Discrete AC or M for Mandatory AC");
            string key = Console.ReadLine();
            string k2 = null;
            switch (key.ToUpper())
            {
                case "D":                    
                case "M":                
                    Mode = key.ToLower();
                    Console.Clear();
                    break;
                default:
                    while (k2 != "Y" && k2 != "N")
                    {
                        Console.WriteLine("Incorrect answer! Would you like to retry? (Y/N)");
                        k2 = Console.ReadLine().ToUpper();
                    }
                    if (k2 == "Y") ChooseMode();
                    else
                        if (exit) Environment.Exit(0); else PrintUM(CurUser);
                    break;

            }
        }

        static void Main(string[] args)
        {
            
            Users.Add(admin);
            LoadUsers();
            LoadFiles();
            CurUser = StartAutorisation();            
            
            
            //FormAccessMatrix();            
            ReadAccessMatrix();
            while (true)
            {
                string Command = Console.ReadLine();
                CurrCommand = ParseCommand(Command);
                WriteToBuf(Command);
                ComInt.ExecuteCommand(CurrCommand);                
                PrintUM(CurUser);
                
            }          
              
            
            //затестить
            //добавить регистрацию
            //пользователь может завершать или не завершать сессию после выхода или в процессе пользования программой
            //пользователь может видеть недоступные файлы
            
        }
    }
}
