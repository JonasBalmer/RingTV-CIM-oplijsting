using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RingTV_CIM_oplijsting
{
    class StudioLog
    {
        public List<LogEntry> entries = new List<LogEntry>();
    }
    class LogEntry
    {
        public string[] values;
        public System.Net.IPAddress IP;
        public DateTime TimeStamp;
        public TimeSpan AirTime;
        public TimeSpan inpoint;
        public string HOST;
        public entryAction action;
        public Uri file;
        public string title;
        public string code;
        public AdvertiserID AdvertiserID = AdvertiserID.NONE;
        public BlokType BlokType;

        internal void setAction(string type)
        {
            switch (type)
            {
                case "Start":
                    action = entryAction.Start;
                    break;
                case "Manual":
                    action = entryAction.Manual;
                    break;
                case "STARTED":
                    action = entryAction.STARTED;
                    break;
                case "ClockResync":
                    action = entryAction.ClockResync;
                    break;
                case "OACError":
                    action = entryAction.OACError;
                    break;
                case "Synchronization":
                    action = entryAction.Synchronization;
                    break;
                case "SKIPPED":
                    action = entryAction.SKIPPED;
                    break;
                case "SUBSTREAM":
                    action = entryAction.SUBSTREAM;
                    break;
                case "STARTEDSUBSTREAM":
                    action = entryAction.STARTEDSUBSTREAM;
                    break;
                default:
                    System.Diagnostics.Debug.Fail("No valid action: " + type);
                    break;

            }
        }

        internal void SetBlokType(string type)
        {
            switch (type)
            {
                case "TRA":
                    BlokType = BlokType.TRA;
                    break;
                case "COM":
                    BlokType = BlokType.COM;
                    break;
                case "CON":
                    BlokType = BlokType.CON;
                    break;
                case "BAN":
                    BlokType = BlokType.BAN;
                    break;
                case "0":
                    BlokType = BlokType.ZERO;
                    break;
                default:
                    System.Diagnostics.Debug.WriteLine("No valid blocktype: " + type);
                    break;

            }
        }
        internal void SetAdvertiserId(string type)
        {
            switch (type)
            {
                case "KEA":
                    AdvertiserID = AdvertiserID.KEA;
                    break;
                case "EXTERN":
                    AdvertiserID = AdvertiserID.EXTERN;
                    break;
                case "FINGER":
                    AdvertiserID = AdvertiserID.FINGER;
                    break;
                case "FING":
                    AdvertiserID = AdvertiserID.FINGER;
                    break;
                case "RTV":
                    AdvertiserID = AdvertiserID.RTVM;
                    break;
                case "TV OOST":
                    AdvertiserID = AdvertiserID.TV_OOST;
                    break;
                case "RTVM":
                    AdvertiserID = AdvertiserID.RTVM;
                    break; 
                default:
                    System.Diagnostics.Debug.WriteLine("No valid AdvertiserID: " + type);
                    break;
            }
        }
    }
    public enum entryAction { Start, Manual, STARTED, ClockResync, OACError, Synchronization, SKIPPED,  SUBSTREAM , STARTEDSUBSTREAM}
    public enum BlokType { TRA,COM,CON,BAN,ZERO}
    public enum AdvertiserID {KEA,EXTERN,FINGER,TV_OOST,RTVM, NONE}
}
