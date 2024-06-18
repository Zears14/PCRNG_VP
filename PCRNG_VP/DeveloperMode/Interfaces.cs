using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCRNG_VP.DeveloperMode
{

    public interface ICommand
    {
        string Description { get; }
        string Execute(string[] args);
    }

}
