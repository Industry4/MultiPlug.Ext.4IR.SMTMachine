using System.Runtime.Serialization;

namespace MultiPlug.Ext._4IR.SMTMachine.Models.Exchange
{
    public class ReadySubscription : Base.Exchange.Subscription
    {
        [DataMember]
        public string ReadyValue { get; set; } = "1";
        [DataMember]
        public string NotReadyValue { get; set; } = "0";

        public ReadySubscription()
        {
        }

        public ReadySubscription(string theGuid, string theId)
        {
            Id = theId;
            Guid = theGuid;
        }
        public static bool Merge(ReadySubscription theSubscriptionInto, ReadySubscription theSubscriptionFrom)
        {
            if ((!string.IsNullOrEmpty(theSubscriptionFrom.ReadyValue)) && theSubscriptionInto.ReadyValue != theSubscriptionFrom.ReadyValue)
            {
                theSubscriptionInto.ReadyValue = theSubscriptionFrom.ReadyValue;
            }

            if ((!string.IsNullOrEmpty(theSubscriptionFrom.NotReadyValue)) && theSubscriptionInto.NotReadyValue != theSubscriptionFrom.NotReadyValue)
            {
                theSubscriptionInto.NotReadyValue = theSubscriptionFrom.NotReadyValue;
            }

            return Base.Exchange.Subscription.Merge(theSubscriptionInto, theSubscriptionFrom);
        }
    }
}
