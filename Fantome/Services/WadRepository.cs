using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fantome.Services
{
    public interface IWadRepositoryService
    {
        public Task Initialize(string gameLocation);
    }

    public class WadRepositoryService : IWadRepositoryService
    {
        public Task Initialize(string gameLocation) => throw new NotImplementedException();
    }
}
