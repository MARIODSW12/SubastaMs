using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subasta.Infrastructure.Interfaces
{
    public interface ICronJobService
    {
        void AddCronJob(string auctionId, string cronId);
        string GetCronJobId(string auctionId);
    }
}
