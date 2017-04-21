using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zabbkit.Web.Models;

namespace Zabbkit.Web.Services
{
    public interface IDeviceService
    {
        void Create(Device device);
        void RenewTokent(TokenRenewRequest renewRequest);
        void Delete(string id);
        IEnumerable<Device> GetAll();
        Device Get(string id);
    }
}
