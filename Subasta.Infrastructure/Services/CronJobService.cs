using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Subasta.Infrastructure.Dtos;
using Subasta.Infrastructure.Interfaces;

namespace Subasta.Infrastructure.Services
{
    public class CronJobService: ICronJobService
    {
        private List<CronAuctionId> cronAuctionIds = [];
        private readonly object _lock = new();
        public CronJobService()
        {
            cronAuctionIds = new List<CronAuctionId>();
        }

        public void AddCronJob(string auctionId, string cronId)
        {
            lock (_lock)
            {
                cronAuctionIds.Add(new CronAuctionId { AuctionId = auctionId, Id = cronId });
            }
        }

        public string GetCronJobId(string auctionId)
        {
            lock (_lock)
            {
                return cronAuctionIds.FirstOrDefault(x => x.AuctionId == auctionId)?.Id;
            }
        }
    }
}
