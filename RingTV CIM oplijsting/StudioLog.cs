using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace RingTV_CIM_oplijsting
{
    class StudioLog
    {
        public static StudioLog ParseCSV(string filePath)
        {
            StudioLog response = new StudioLog();
            StreamReader streamReader = new StreamReader(filePath);
            LogEntry lastEntry = new LogEntry();
            while (!streamReader.EndOfStream)
            {
                string line = streamReader.ReadLine();
                LogEntry entry = new LogEntry();
                entry = ParseEntry(line.Split(new char[] { ',' }));
                if (entry != null)
                {
                    response.entries.Add(entry);
                    if (lastEntry != null)
                        lastEntry.duration = entry.AirTime.Subtract(lastEntry.AirTime);
                    lastEntry = entry;
                }
            }
            return response;
        }

        private static LogEntry ParseEntry(string[] values)
        {
            LogEntry response = new LogEntry();
            response.values = values;

            if (!System.Net.IPAddress.TryParse(values[0], out response.IP))
            {
                Debug.Fail("No valid IP");
            }


            if (!ParseDate(values[1], out response.TimeStamp))
            {
                string value;
                response.HOST = values[1];
                //if(values.Length ==11)

                if (!Uri.TryCreate(values[4], UriKind.RelativeOrAbsolute, out response.file))
                    Debug.Fail("No valid File");

                if (!DateTime.TryParse(values[5], out response.TimeStamp))
                    Debug.Fail("No valid Date");

                if (!TimeSpan.TryParse(values[6], out response.AirTime))
                    Debug.Fail("No valid TimeStamp");

                response.setAction(values[7]);
                response.title = values[8].Trim();
                if (values.Length == 11)
                    response.code = "NONE";
                else if (values.Length == 12)
                {
                    value = values[9].ToUpper().Trim();
                    response.code = value;
                    if (response.code == "")
                        response.code = "NONE";

                    if (response.action != "SKIPPED")
                    {
                        TimeSpan timeSpan;
                        value = values[10];
                        if (value == "")
                        {
                            value = values[9];
                            response.code = "NONE";
                        }
                        if (!TimeSpan.TryParse(value.Substring(0, 8), out timeSpan))
                        {
                            Debug.Fail("No valid TimeStamp");
                        }
                        else
                        {
                            response.duration = timeSpan;
                        }

                    }
                    if (response.code.Substring(0, 1) != "Q" && response.code != "NONE")
                    {
                        response.code = "NONE";
                    }
                }
                else if (values.Length == 15)
                {
                    value = values[9].ToUpper().Trim();
                    if (values[9].Substring(0, 1) == "Q")
                        response.code = value;
                    else
                        response.SetAdvertiserId(value);
                    response.SetBlokType(values[11]);
                    value = values[12];
                    if (value.Length < 8)
                        value = values[13];

                }
                else if (values.Length == 14)
                {
                    value = values[12];
                    if (value.Length < 8)
                        value = values[13];

                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(values.Length);
                }
                if (response.title.ToUpper().Contains("KEA"))
                    response.AdvertiserID = "KEA";
                if (response.title.ToUpper().Contains("EXTERN"))
                    response.AdvertiserID = "EXTERN";
                if (response.title.ToUpper().Contains("FING"))
                    response.AdvertiserID = "FINGER";
                if (response.title.ToUpper().Contains("FINGER"))
                    response.AdvertiserID = "FINGER";
                if (response.title.ToUpper().Contains("RTVM"))
                    response.AdvertiserID = "RTVM";
                if (response.title.ToUpper().Contains("TV OOST"))
                    response.AdvertiserID = "TV OOST";
            }
            else
            {
                return null;
            }


            if (response.title.Contains("(") && response.title.Contains(")"))
            {
                int indexLeft = response.title.LastIndexOf("(") + 1;
                int indexRight = response.title.LastIndexOf(")");
            }
            return response;
        }

        private static bool ParseDate(string input, out DateTime timeStamp)
        {
            timeStamp = new DateTime();
            int Year, Month, Day, Hour, Minute, Second;
            if (input.Length != 22)
                return false;
            if (!int.TryParse(input.Substring(0, 4), out Year))
                return false;
            if (!int.TryParse(input.Substring(5, 2), out Month))
                return false;
            if (!int.TryParse(input.Substring(8, 2), out Day))
                return false;
            if (!int.TryParse(input.Substring(11, 2), out Hour))
                return false;
            if (!int.TryParse(input.Substring(14, 2), out Minute))
                return false;
            if (!int.TryParse(input.Substring(17, 2), out Second))
                return false;
            timeStamp = new DateTime(Year, Month, Day, Hour, Minute, Second);
            return true;
        }

        public List<LogEntry> entries = new List<LogEntry>();
    }

    class LogEntry
    {
        public string[] values;
        public System.Net.IPAddress IP;
        public DateTime TimeStamp;
        public TimeSpan AirTime;
        public TimeSpan duration;
        public string HOST;
        public string action;
        public Uri file;
        public string title;
        public string code = "";
        public string AdvertiserID = "";
        public string BlokType = "";


        internal void setAction(string type)
        {
            action = type;
            if (!CIMTypes.EntryActions.Contains(type))
                CIMTypes.EntryActions.Add(type);
        }

        internal void SetBlokType(string type)
        {
            BlokType = type;
            if (!CIMTypes.BlockTypes.Contains(type))
                CIMTypes.BlockTypes.Add(type);

        }
        internal void SetAdvertiserId(string type)
        {
            AdvertiserID = type;
            if (!CIMTypes.AdvertiserIDs.Contains(type))
                CIMTypes.AdvertiserIDs.Add(type);
        }
    }

}
