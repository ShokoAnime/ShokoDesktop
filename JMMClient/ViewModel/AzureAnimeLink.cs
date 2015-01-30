using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JMMClient.ViewModel
{
    public class AzureAnimeLink
    {
        public int RandomAnimeID { get; set; }
        public int AnimeNeedingApproval { get; set; }

        public override string ToString()
        {
            return string.Format("{0} - {1}", AnimeNeedingApproval, RandomAnimeID);
        }

        public AzureAnimeLink()
        {

        }

        public AzureAnimeLink(JMMServerBinary.Contract_Azure_AnimeLink contract)
        {
            this.RandomAnimeID = contract.RandomAnimeID;
            this.AnimeNeedingApproval = contract.AnimeNeedingApproval;
        }
    }
}
