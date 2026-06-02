
using System.Runtime.Serialization;

namespace MultiPlug.Ext._4IR.SMTMachine.Models.Exchange
{
    public class ReadyEvent : Base.Exchange.Event
    {
        [DataMember]
        public string ReadyValue { get; set; } = "1";
        [DataMember]
        public string NotReadyValue { get; set; } = "0";

        public static bool Merge(ReadyEvent theMerged, ReadyEvent theMergeFrom, bool shouldMergeSubjects)
        {
            if (theMergeFrom.ReadyValue != null && theMergeFrom.ReadyValue != theMerged.ReadyValue)
            {
                theMerged.ReadyValue = theMergeFrom.ReadyValue;
            }
            if (theMergeFrom.NotReadyValue != null && theMergeFrom.NotReadyValue != theMerged.NotReadyValue)
            {
                theMerged.NotReadyValue = theMergeFrom.NotReadyValue;
            }

            return Base.Exchange.Event.Merge(theMerged, theMergeFrom, shouldMergeSubjects);
        }
    }
}
