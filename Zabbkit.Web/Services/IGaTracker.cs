using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Zabbkit.Web.Services
{
    public interface IGaTracker : IDisposable
    {
        Task TrackPage(string pageTitle, string pageUrl);
        Task TrackEvent(string category, string action);
    }
}
