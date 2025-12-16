using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koala.Yedpa.Core.Services
{
    public interface ILicenseValidator
    {
        bool IsLicenseValid();
        string? GetXKey();
        string? GetApplicationId();
    }
}
