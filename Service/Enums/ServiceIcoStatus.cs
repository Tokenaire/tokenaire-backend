using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Tokenaire.Service.Enums
{
    public enum ServiceIcoStatus {
        NotYetStarted = 1,
        Running = 2,
        Finished = 3
    }
}