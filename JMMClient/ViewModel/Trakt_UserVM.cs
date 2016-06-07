using System;

namespace JMMClient.ViewModel
{
    public class Trakt_UserVM
    {
        public int Trakt_FriendID { get; set; }
        public string Username { get; set; }
        public string Full_name { get; set; }
        public string Gender { get; set; }
        public object Age { get; set; }
        public string Location { get; set; }
        public string About { get; set; }
        public int Joined { get; set; }
        public DateTime? JoinedDate { get; set; }
        public string Avatar { get; set; }
        public string Url { get; set; }

        public Trakt_UserVM(JMMServerBinary.Contract_Trakt_User contract)
        {
            this.Trakt_FriendID = contract.Trakt_FriendID;
            this.Username = contract.Username;
            this.Full_name = contract.Full_name;
            this.Gender = contract.Gender;
            this.Age = contract.Age;
            this.Location = contract.Location;
            this.About = contract.About;
            this.Joined = contract.Joined;
            this.JoinedDate = contract.JoinedDate;
            this.Avatar = contract.Avatar;
            this.Url = contract.Url;
        }
    }
}
