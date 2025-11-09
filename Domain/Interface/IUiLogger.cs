using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaManager.Domain.Interface
{
    public interface IUiLogger
    {
        void Log(string message);
    }
}