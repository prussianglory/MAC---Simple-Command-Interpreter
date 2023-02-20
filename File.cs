using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeOSux
{
    class File
    {
       public string Name; 
       public User Creator;
       public string Content = "";
       public Dictionary<string, string> Access = new Dictionary<string, string>() { { "admin", "-rw" } };
       public int AccessLevel;        
    }
}
