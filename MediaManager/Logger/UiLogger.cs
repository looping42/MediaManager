using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaManager.Logger
{
    public class UiLogger : IUiLogger
    {
        public void Log(string message)
        {
            Console.WriteLine(message); // ou Debug.WriteLine
        }
    }
}