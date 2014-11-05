using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JMMClient.ViewModel
{
    public class AdminMessage
    {
        public int AdminMessageId { get; set; }
        public long MessageDate { get; set; }
        public int MessageType { get; set; }
        public string Message { get; set; }
        public string MessageURL { get; set; }

        public AdminMessage()
        {

        }

        public AdminMessage(JMMServerBinary.Contract_AdminMessage contract)
        {
            AdminMessageId = contract.AdminMessageId;
            MessageDate = contract.MessageDate;
            MessageType = contract.MessageType;
            Message = contract.Message;
            MessageURL = contract.MessageURL;
        }

        public DateTime MessageDateAsDate
        {
            get
            {

                return TimeZone.CurrentTimeZone.ToLocalTime(Utils.GetAniDBDateAsDate((int)MessageDate).Value);
            }
        }

        public bool HasMessageURL
        {
            get
            {

                return !string.IsNullOrEmpty(MessageURL);
            }
        }

        public override string ToString()
        {
            return string.Format("{0} - {1} - {2}", AdminMessageId, MessageDateAsDate, Message);
        }
    }
}
