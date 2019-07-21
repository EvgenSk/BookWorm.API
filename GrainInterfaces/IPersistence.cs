using System;
using System.Threading.Tasks;

namespace GrainInterfaces
{
    public interface IPersistence : Orleans.IGrainWithIntegerKey
    {
        Task<DateTime> GetCreationTime();
    }
}
