using System.Runtime.Serialization;

namespace MultiPlug.Ext._4IR.SMTMachine.Models.Exchange
{
    public class AdjustmentsEvent : Base.Exchange.Event
    {
        [DataMember]
        public string OkayValue { get; set; } = "Okay";

        public static bool Merge(AdjustmentsEvent theMerged, AdjustmentsEvent theMergeFrom, bool shouldMergeSubjects)
        {
            if (theMergeFrom.OkayValue != null && theMergeFrom.OkayValue != theMerged.OkayValue)
            {
                theMerged.OkayValue = theMergeFrom.OkayValue;
            }

            return Base.Exchange.Event.Merge(theMerged, theMergeFrom, shouldMergeSubjects);
        }
    }
}