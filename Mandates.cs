using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeOSux
{
    class Mandates
    {
        static public Mandate Low = new Mandate() { Name = "low", AccessLevel = 0 };
        static public Mandate Medium = new Mandate() { Name = "medium", AccessLevel = 1 };
        static public Mandate High = new Mandate() { Name = "high", AccessLevel = 2 };
        static public Mandate Supervisor = new Mandate() { Name = "supervisor", AccessLevel = 3 };
        static public List<Mandate> ListOfMandates = new List<Mandate>() { Low, Medium, High, Supervisor };

        static public bool Contains(string mandate)
        {
            bool IsContain = false;
            foreach (Mandate m in ListOfMandates)
            {
                if (m.Name == mandate)
                    IsContain = true;
            }
            return IsContain;
        }
        static public Mandate DetectMandate(string mandate)
        {
           
            foreach (Mandate m in ListOfMandates)
            {
                if (m.Name == mandate)
                    return m;                    
            }
            return null;
        }

        static public Mandate GetLevel(int AccessLevel)
        {
            switch (AccessLevel)
            {
                case 0:
                    return Low;                    
                case 1:
                    return Medium;                    
                case 2:
                    return High;
                case 3:
                    return Supervisor;
            }
            return null;
        }
    }
}
