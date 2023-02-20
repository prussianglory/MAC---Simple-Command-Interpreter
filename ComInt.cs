using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DeOSux
{
    class ComInt // интерпретатор команд
    {
        static public string HomeDir = @"C:\Users\SVS1984\source\repos\DeOSux\DeOSux\HomeDir\";
        
        static public string usersIni = @"C:\Users\SVS1984\source\repos\DeOSux\DeOSux\bin\users.ini";        
        static Dictionary<string, int> NeccessaryArgs = new Dictionary<string, int>
        {
            {"touch", 2},
            {"rm", 2},
            {"deluser", 2},
            {"useradd", 2},
            {"ls", 1},
            {"allusers", 1},
            {"cat", 2},
            {"exit", 1},
            {"access", 1},
            {"su", 2},
            {"chmod", 4},
            {"changemode", 1},
            {"help", 1}
        };      
        

        static public void Error(int code, string arg = null)
        {
            string ErrorOutput = "";
            switch(code)
            {
                case 1:
                    ErrorOutput = $"Error: max length of file name is 10";                    
                    break;
                case 2:
                    ErrorOutput = $"Error: file '{arg}' already exist";
                    break;
                case 3:
                    ErrorOutput = "Error: max length of username is 10";
                    break;
                case 4:
                    ErrorOutput = "Error: passwords are not equal";
                    break;
                case 5:
                    ErrorOutput = $"Error: file '{arg}' doesn't exist";
                    break;
                case 6:
                    ErrorOutput = "Error: incorrect password";
                    break;
                case 7:
                    ErrorOutput = "Error: rights must be \"---\" or \"-r-\" or \"-rw\"";
                    break;
                case 8:
                    ErrorOutput = $"Error: user '{arg}' doesn't exist";
                    break;
                case 9:
                    ErrorOutput = $"Error: command '{arg}' requires {NeccessaryArgs[arg] - 1}";
                    
                    if (NeccessaryArgs[arg] > 2 || NeccessaryArgs[arg] == 1)
                    {
                        ErrorOutput += " arguments";
                    }
                    else ErrorOutput += " argument";
                    break;
                case 10:
                    ErrorOutput = "Access denied";
                    break;
                case 11:
                    ErrorOutput = $"Error: user '{arg}' already exists'";
                    break;
                case 12:
                    ErrorOutput = $"Error: command '{arg}' doesn't exist";
                    break;
                case 13:
                    ErrorOutput = "Error: password is neccessary";
                    break;
                case 14:
                    ErrorOutput = $"Error: level '{arg}' doesn't exist";
                    break;
                case 15:
                    ErrorOutput = $"Error: role '{arg}' doesn't exist";
                    break;
                case 16:
                    ErrorOutput = $"Error: level must be from list (low, medium, high, supervisor)";
                    break;
            }
            Console.WriteLine(ErrorOutput);
            Program.WriteToBuf(ErrorOutput);
        }
       

        static void touch(string name) //создать файл
        {
            if (name.Length > 10)
            {
                Error(1);
                return;
            }
            bool IsFileExist = false;
            foreach (File f in Program.Files)
            {
                if (name == f.Name)
                {
                    Error(2, name);
                    IsFileExist = true;
                }
            }
            if (!IsFileExist){
                
                File file = new File()
                {
                    Name = name,
                    Creator = Program.CurUser,
                    AccessLevel = Program.CurUser.Level.AccessLevel

                };
                if (Program.CurUser != Program.admin)
                {
                    file.Access[Program.CurUser.Username] = "-rw";
                }
                foreach (User user in Program.Users)
                {
                    if (user != file.Creator && user!=Program.admin)
                    {
                        file.Access.Add(user.Username, "---");
                    }
                }
                Program.Files.Add(file);
                using (StreamWriter sw = new StreamWriter(Program.fileInfoPath, true))
                {
                    sw.WriteLine($"{name}\t{Program.CurUser.Username}\t{file.AccessLevel}");
                }
                System.IO.File.CreateText(HomeDir + name + ".txt").Close();
                Program.FormAccessMatrix();
            }
        }
       
        static void rm(string name) //удалить файл
        {
            foreach (File file in Program.Files)
            {
                if (name == file.Name)
                {
                    if (Program.CurUser == Program.admin || Program.CurUser == file.Creator)
                    {
                        Program.Files.Remove(file);
                        OverwriteFileInfo();
                        if (System.IO.File.Exists(HomeDir + name + ".txt"))
                        {
                            System.IO.File.Delete(HomeDir + name + ".txt");
                        }

                    }
                    else Error(5, file.Name);
                    break;
                }
            }
            Program.FormAccessMatrix();
        }
        static void ls() //вывести список файлов
        {
            using (StreamWriter sw = new StreamWriter(Program.logpath, true))
            {
                foreach (File file in Program.Files)
                {
                    switch (Program.Mode)
                    {
                        case "d":
                        if (file.Access[Program.CurUser.Username] == "-r-" || file.Access[Program.CurUser.Username] == "-rw")
                        {
                            Console.WriteLine(file.Name);
                            sw.WriteLine(file.Name);

                        }
                        break;
                        case "m":
                            if (Program.CurUser == Program.admin || Program.CurUser.Level == Mandates.High)
                            {
                                Console.WriteLine(file.Name+" "+Mandates.GetLevel(file.AccessLevel).Name);
                                sw.WriteLine(file.Name + " " + Mandates.GetLevel(file.AccessLevel).Name);
                            }
                            else
                            {
                                Console.WriteLine(file.Name);
                                sw.WriteLine(file.Name);
                            }
                            break;                        
                    }
                }
            }
        }       

        static void useradd(string newUserName) //добавить пользователя
        {            
            if (newUserName.Length > 10)
            {
                Error(3);
                return;
            }
            string pass1, pass2;
            Console.WriteLine("Input password for new user");
            Program.WriteToBuf("Input password for new user");
            pass1 = Program.InputPassword();
            if (pass1 == "")
            {
                Error(13);
            }
            Console.WriteLine("Repeat password");
            Program.WriteToBuf("Repeat password");
            pass2 = Program.InputPassword();
            if (pass1 == pass2)
            {

                User user = new User()
                {
                    Username = newUserName,
                    Password = pass1,
                    Level = ChooseAccessLevel()                    
                };
                if (user.Level == null)
                {
                    Error(14);
                    return;
                }                
                Program.Users.Add(user);
                using (StreamWriter sw1 = new StreamWriter(usersIni, true))
                {
                    sw1.WriteLine($"{user.Username}\t{user.Password}\t{user.Level.Name}");
                }                
                foreach (File file in Program.Files)
                {
                    file.Access.Add(user.Username, "---");
                    file.AccessLevel = file.Creator.Level.AccessLevel;
                }
                Program.FormAccessMatrix();
            }
            else
            {
                Error(4);
            }
        }

        static Mandate ChooseAccessLevel()
        {
            Console.Write("Choose access level: ");
            foreach (Mandate m in Mandates.ListOfMandates)
            {
                Console.Write(m.Name + " ");
            }
            Console.WriteLine();
            string mName = Console.ReadLine();
            return Mandates.DetectMandate(mName);
        }
        

        static void OverWriteUsers()
        {
            FileInfo fileInf = new FileInfo(usersIni);
            if (fileInf.Exists)
            {
                fileInf.Delete();                
            }
            using (StreamWriter sw = fileInf.CreateText())
            {
                foreach (User user in Program.Users)
                {
                    if (user != Program.admin)
                    sw.WriteLine($"{user.Username}\t{user.Password}\t{user.Level.Name}");
                }
            }            
            /*
            Program.Users.Clear();
            Program.Users.Add(Program.admin);
            Program.LoadUsers();
            */
        }

        static void OverwriteFileInfo()
        {
            FileInfo fileInf = new FileInfo(Program.fileInfoPath);
            if (fileInf.Exists)
            {
                fileInf.Delete();
            }
            using (StreamWriter sw = fileInf.CreateText())
            {
                foreach (File file in Program.Files)
                {
                    sw.WriteLine($"{file.Name}\t{file.Creator.Username}\t{file.Creator.Level.AccessLevel}");
                }
            }
        }
        
        static void changemode()
        {
            Console.Clear();
            Program.ClearBuf();
            Program.ChooseMode(false);
            PrintBuf();
        }

        static void allusers()
        {
            using (StreamWriter sw = new StreamWriter(Program.logpath,true))
            {
                foreach (User user in Program.Users)
                {
                    if (user != Program.admin)
                    {
                        if (Program.Mode == "m")
                        {
                            Console.WriteLine($"{user.Username} {user.Level.Name}");
                            sw.WriteLine($"{user.Username} {user.Level.Name}");
                        }
                        else
                        {
                            Console.WriteLine($"{user.Username}");
                            sw.WriteLine($"{user.Username}");
                        }
                    }
                }
            }
            /*
            using (StreamReader sr = new StreamReader(usersIni))
            {
                while (!sr.EndOfStream)
                {
                    string s = sr.ReadLine();
                    Console.WriteLine(s);                    
                }
            }
            */
        }

        static void deluser(string name)
        {
            
            foreach (User user in Program.Users)
            {                
                if (name== user.Username)
                {
                    Program.Users.Remove(user);
                    foreach (File file in Program.Files)
                    {
                        file.Access.Remove(user.Username);
                    }   
                    OverWriteUsers();
                    return;
                }
            }            
            Program.FormAccessMatrix();
        }

        static bool WriteAndRead(File f)
        {
            
            ConsoleFiller cf = new ConsoleFiller();
            string s = cf.ReadLine(f);
            if (System.IO.File.Exists(HomeDir + f.Name + ".txt"))
            {
                using (StreamWriter sw = new StreamWriter(HomeDir + f.Name + ".txt"))
                {
                    sw.Write(s);
                }
            }
            PrintBuf();
            return true;
        }

        static bool ReadFile(File f)
        {
            string str;
            using (StreamReader sr = new StreamReader(HomeDir + f.Name + ".txt"))
            {
                while (!sr.EndOfStream)
                {
                    str = sr.ReadLine();
                    Console.WriteLine(str);
                    Program.WriteToBuf(str);
                }
            };
            return true;
        }

        static bool WriteFile(File f)
        {
            using (StreamWriter sw = new StreamWriter(HomeDir + f.Name + ".txt", true))
            {
                string text = Console.ReadLine();
                sw.Write(text);
                f.Content += text;
            }
            return true;
        }

        static void cat(string name)
        {
            /* 
             гость - может читать файлы от администрации
             солдатня - может читать все файлы, кроме высшего командования
             повар - не может ничего читать, но может писать файлы, которые могут прочитать все
             младшие офицеры - то же что и солдатня, но могут писать файлы, доступные военным чинам
            администрация не может читать файлы, но может писать файлы только для гостей
            старшие офицеры - полный доступ ко всем файлам, их файлы невидимы для всех

            гость
            солдатня+повар
            мл офицеры + администрация
            старшие офицеры + младшие офицеры

                */

            bool IsFileExist = false;
                foreach (File f in Program.Files)
                {
                    if (f.Name == name)
                    {
                        switch (Program.Mode)
                        {
                            case "d":
                            #region Дискреционная модель
                            if (f.Access[Program.CurUser.Username] == "-rw")
                            {
                                IsFileExist = WriteAndRead(f);
                            }
                            else if (f.Access[Program.CurUser.Username] == "-r-")
                            {
                                IsFileExist = ReadFile(f);
                            }
                            else
                            {
                                IsFileExist = true;
                                Error(10);
                            }
                            #endregion
                            break;

                            case "m":
                            #region Мандатная модель
                            if (f.AccessLevel == Program.CurUser.Level.AccessLevel)
                            {
                                IsFileExist = WriteAndRead(f);
                                return;
                            }
                            else if (f.AccessLevel < Program.CurUser.Level.AccessLevel)
                            {
                                IsFileExist = ReadFile(f);
                                return;
                            }
                            else
                            {
                                IsFileExist = WriteFile(f);
                                return;
                            }
                            #endregion
                            break;                            
                        }
                    }
                                     
                
                }
            if (!IsFileExist)
            {
                Error(5, name);
            }
        }

        static void help()
        {
            List<string> AdminCommands = new List<string>()
            {
                
                "useradd u - Create new user named u\n",
                "deluser u - Delete user named u\n",
                "changemode m - Change model of access - d for Discrete AC, m for Mandatory AC\n",
                "chmod f u r - Change rights for user u on file f: ",
            };
            if (Program.Mode == "d")
            {
                AdminCommands.Add("---, -r- or -rw \n");
                AdminCommands.Add("access - Print access matrix of files and users");
            }
            else
            {
                AdminCommands.Add("low, medium or high\n") ;               
            }
            List<string> HelpStr = new List<string>()
            {
                "touch f - Create file named f",
                "rm f - Delete file named f",
                "ls - Print list of files",
                "allusers - Print list of all users",
                "su - Enter as another user",
                "cat f - Open file named f",
                "exit - Power off DeOSux Machine"                
            };
            using (StreamWriter sw = new StreamWriter(Program.logpath, true))
            {
                if (Program.CurUser == Program.admin)
                {
                    foreach (string command in AdminCommands)
                    {
                        Console.Write(command);
                        sw.Write(command);
                    }
                }
                foreach (string command in HelpStr)
                {
                    Console.WriteLine(command);
                    sw.WriteLine(command);
                }
            }
        }

        static void access(bool printToLog = true)
        {
            Console.Write("\t");
            foreach (File file in Program.Files)
            {
                Console.Write("{0,-10}", file.Name);
            }
            Console.WriteLine();
            foreach (User user in Program.Users)
            {
                Console.Write("{0,-10}", user.Username);
                foreach (File file in Program.Files)
                {
                   Console.Write("{0,-10}", file.Access[user.Username]);
                }
                Console.WriteLine();
            }
            if (printToLog)
            {
                using (StreamWriter sw = new StreamWriter(Program.logpath, true))
                {
                    sw.WriteLine("|A|");
                }
            }
        }

        static void su(string name)
        {
            bool IsUserExist = false;
            foreach (User user in Program.Users)
            {
                if (user.Username == name)
                {
                    IsUserExist = true;
                    using (StreamWriter sw = new StreamWriter(Program.logpath, true))
                    {
                        Console.WriteLine("Enter password:");
                        sw.WriteLine("Enter password:");
                    }
                    string pass = Program.InputPassword();
                    if (pass == user.Password)
                    {                       
                        Program.CurUser = user;
                        Console.Clear();
                        Program.ClearBuf();
                        return;
                    }
                    else
                    {
                        Error(6);
                    }
                }
            }
            if (!IsUserExist) Error(8, name);
        }

        static void chmod(string filename, string username, string newRights)
        {
            switch (Program.Mode)
            {
                case "d":
                    List<string> DRights = new List<string>() { "---", "-r-", "-rw" };
                    if (!DRights.Contains(newRights))
                    {
                        Error(7);
                        return;
                    }
                    break;
                case "m":
                    List<string> MLevels = new List<string>() { "low", "medium", "high", "supervisor" };
                    if (!MLevels.Contains(newRights))
                    {
                        Error(16);
                        return;
                    }
                    break;                
            }
            foreach (File file in Program.Files)
            {
                if (file.Name == filename)
                {
                    
                    foreach (User user in Program.Users)
                    {
                        if (username == user.Username)   
                        {   
                            switch (Program.Mode)
                            {
                                case "d":
                                    file.Access[user.Username] = newRights;
                                    Program.FormAccessMatrix();
                                    break;
                                case "m":
                                    foreach (Mandate mandate in Mandates.ListOfMandates)
                                    {
                                        if (mandate.Name == newRights.ToLower())
                                        {
                                            user.Level = mandate;
                                            break;
                                        }
                                    }
                                    break;                               
                            }                            
                            return;
                        }
                    }
                    Error(8);
                    return;
                }
            }
            Error(5, filename);
        }

        static bool CheckArgsCount(string[] command)
        {
            bool flag = true;
            if (command.Count() != NeccessaryArgs[command[0]])
            {
                Error(9, command[0]);
                flag = false;
            }
            return flag;
        }

        static void PrintBuf()
        {
            using (StreamReader sr = new StreamReader(Program.logpath))
            {
                while (!sr.EndOfStream)
                {
                    string[] str = sr.ReadLine().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (str.Count() >= 1)
                    {
                        bool IsCommand = false;
                        if (str[0] == "|UM|")
                        {
                            IsCommand = true;
                            Program.PrintUM(Program.CurUser, false);
                        }
                        else if (str[0] == "|A|")
                        {
                            IsCommand = true;
                            access(false);
                        }
                        int start;
                        start = (IsCommand) ? 1 : 0;
                        for (int i = start; i < str.Count(); i++)
                        {
                            Console.Write(str[i] + " ");
                        }
                        Console.WriteLine();
                    }
                }
            }
        }
        

        static public void ExecuteCommand(string[] command)
        {
            switch(command[0])
            {
                case "cat":
                    if (CheckArgsCount(command))
                    cat(command[1]);
                    break;
                case "deluser":
                    if (Program.CurUser != Program.admin)
                    {
                        Error(10);
                        return;
                    }
                    if (CheckArgsCount(command))
                        deluser(command[1]);
                        break;
                case "allusers":                   
                    if (CheckArgsCount(command))
                    allusers();
                    break;
                case "touch":
                    if (CheckArgsCount(command))
                    touch(command[1]);                    
                    break;
                case "rm":
                    if (CheckArgsCount(command))                       
                    rm(command[1]);
                    break;
                case "ls":
                    if (CheckArgsCount(command))
                    ls();
                    break;
                case "exit":
                    if (CheckArgsCount(command))
                    {
                        Program.ClearBuf();
                        Environment.Exit(0);
                    }
                    break;
                case "useradd":
                    if (Program.CurUser != Program.admin)
                    {
                        Error(10);
                        return;
                    }
                    if (CheckArgsCount(command))
                    {
                        foreach (User user in Program.Users)
                        {
                            if (command[1] == user.Username)
                            {
                                Error(11, command[1]);
                                return;
                            }
                        }                    
                        useradd(command[1]);                  
                    }
                    break;
                case "access":
                    if (Program.CurUser != Program.admin)
                    {
                        Error(10);
                        return;
                    }
                    if (Program.Mode == "m")
                    {
                        Error(12, command[0]);
                        return;
                    }
                    if (CheckArgsCount(command))
                    {
                        access();
                    }
                    break;
                case "su":
                    if (CheckArgsCount(command))
                    {
                        su(command[1]);
                    }
                    break;
                case "chmod":
                    if (CheckArgsCount(command))
                    {
                        if (Program.CurUser == Program.admin)
                            chmod(command[1], command[2], command[3]);
                        else
                            Error(10);
                    }
                    break;
                case "changemode":
                    if (CheckArgsCount(command))
                    {
                        if (Program.CurUser == Program.admin)
                        {
                            changemode();
                        }
                        else
                        {
                            Error(10);
                        }
                    }
                    break;
                case "help":
                    if (CheckArgsCount(command))
                    {
                        help();
                    }
                    break;
                default:
                    Error(12, command[0]);
                    break;
            }
        }
    }
}
