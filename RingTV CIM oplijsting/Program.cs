using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RingTV_CIM_oplijsting
{
    class Program
    {
        static string studioLogPath = "E:\\OAStudioLogs";
        static List<String> PrintoutValues = new List<string>();
        static void Main(string[] args)
        {
            Trace.Listeners.Clear();

            TextWriterTraceListener twtl = new TextWriterTraceListener("Log " + DateTime.Now.ToString("yy-MM-dd hh-mm-ss") + ".log");
            twtl.Name = "TextLogger";
            twtl.TraceOutputOptions = TraceOptions.ThreadId | TraceOptions.DateTime;

            ConsoleTraceListener ctl = new ConsoleTraceListener(false);
            ctl.TraceOutputOptions = TraceOptions.DateTime;

            Trace.Listeners.Add(twtl);
            Trace.Listeners.Add(ctl);
            Trace.AutoFlush = true;
            List<StudioLog> CSVs = GetCSVs(studioLogPath);
            Trace.WriteLine("Q codes:");
            Trace.WriteLine("");
            foreach (string host in PrintoutValues)
                Trace.WriteLine(host );
            System.Diagnostics.Debug.WriteLine(PrintoutValues.Count);
            Trace.WriteLine("");
            Trace.WriteLine("");
            Trace.WriteLine("");
            double numComs = 0;
            double numAdvertisers = 0;
            double numEntries = 0;
            double numFinger = 0;
            double numExtern = 0;
            double numRTVM = 0;
            double numTVOOST = 0;
            double numKea = 0;
            int Year = 2011;
            foreach(var log in CSVs)
            {
                foreach(var entry in log.entries)
                {
                    if(Year != entry.TimeStamp.Year)
                    {
                        Trace.WriteLine("");
                        Trace.WriteLine("");
                        Trace.WriteLine(Year);
                        Trace.WriteLine("");
                        Trace.WriteLine((int)numComs + " entries with QIRRAP = " + (int)(numComs*100/ numEntries) + "%");
                        if(numAdvertisers != 0)
                        {
                            Trace.WriteLine((int)numFinger + " entries with Finger = " + (int)(numFinger * 100 / numAdvertisers) + " % ");
                            Trace.WriteLine((int)numExtern + " entries with Extern = " + (int)(numExtern * 100 / numAdvertisers) + " % ");
                            Trace.WriteLine((int)numRTVM + " entries with RTVM = " + (int)(numRTVM * 100 / numAdvertisers) + " % ");
                            Trace.WriteLine((int)numTVOOST + " entries with TV OOST = " + (int)(numTVOOST * 100 / numAdvertisers) + " % ");
                            Trace.WriteLine((int)numKea + " entries with KEA = " + (int)(numKea * 100 / numAdvertisers) + " % ");
                            Trace.WriteLine("Of "+ (int)numAdvertisers + " advertisers = " + (int)(numAdvertisers * 100 / numEntries) + " % ");
                        }
                        Trace.WriteLine("Of " + (int)numEntries + " entries");
                        Year = entry.TimeStamp.Year;
                        numComs = 0;
                        numAdvertisers = 0;
                        numEntries = 0;
                        numFinger = 0;
                        numExtern = 0;
                        numRTVM = 0;
                        numTVOOST = 0;
                        numKea = 0;
                    }
                    numEntries++;
                    if (entry.code == "QIRRAP")
                        numComs++;
                    if (entry.AdvertiserID == AdvertiserID.EXTERN)
                        numExtern++;
                    if (entry.AdvertiserID == AdvertiserID.FINGER)
                        numFinger++;
                    if (entry.AdvertiserID == AdvertiserID.TV_OOST)
                        numTVOOST++;
                    if (entry.AdvertiserID == AdvertiserID.RTVM)
                        numRTVM++;
                    if (entry.AdvertiserID == AdvertiserID.KEA)
                        numKea++;
                    if(entry.AdvertiserID != AdvertiserID.NONE)
                        numAdvertisers++;
                }
            }
            Trace.WriteLine("");
            Trace.WriteLine("");
            Trace.WriteLine(Year);
            Trace.WriteLine("");
            Trace.WriteLine((int)numComs + " entries with QIRRAP = " + (int)(numComs * 100 / numAdvertisers) + "%");
            Trace.WriteLine((int)numFinger + " entries with Finger = " + (int)(numFinger * 100 / numAdvertisers) + " % ");
            Trace.WriteLine((int)numExtern + " entries with Extern = " + (int)(numExtern * 100 / numAdvertisers) + " % ");
            Trace.WriteLine((int)numRTVM + " entries with RTVM = " + (int)(numRTVM * 100 / numAdvertisers) + " % ");
            Trace.WriteLine((int)numTVOOST + " entries with TV OOST = " + (int)(numTVOOST * 100 / numAdvertisers) + " % ");
            Trace.WriteLine((int)numKea + " entries with KEA = " + (int)(numKea * 100 / numAdvertisers) + " % ");
            Trace.WriteLine("Of " + (int)numAdvertisers + " advertisers = " + (int)(numAdvertisers * 100 / numEntries) + " % ");
            Trace.WriteLine("Of " + (int)numEntries + " entries");
            Console.ReadLine();

        }

        private static List<StudioLog> GetCSVs(string studioLogPath)
        {
            List<StudioLog> response = new List<StudioLog>();
            foreach(string filePath in System.IO.Directory.GetFiles(studioLogPath))
            {
                //Console.WriteLine(filePath);
                response.Add(ParseLog(filePath));
            }
            return response;
        }

        private static StudioLog ParseLog(string filePath)
        {
            StudioLog response = new StudioLog();
            System.IO.StreamReader streamReader = new System.IO.StreamReader(filePath);
            while (!streamReader.EndOfStream)
            {
                string line = streamReader.ReadLine();
                LogEntry entry = new LogEntry();
                entry= ParseEntry(line.Split(new char[]{','}));
                if(entry != null)
                    response.entries.Add(entry);                
            }
            return response;
        }

        private static LogEntry ParseEntry(string[] values)
        {
            LogEntry response = new LogEntry();
            response.values = values;

            if(!System.Net.IPAddress.TryParse(values[0], out response.IP))
            {
                System.Diagnostics.Debug.Fail("No valid IP");
            }


            if(!ParseDate(values[1], out response.TimeStamp))
            {
                string value;
                response.HOST = values[1];
                //if(values.Length ==11)

                if(!Uri.TryCreate(values[4], UriKind.RelativeOrAbsolute,out response.file))
                    System.Diagnostics.Debug.Fail("No valid File");

                if (!DateTime.TryParse(values[5],out response.TimeStamp))
                    System.Diagnostics.Debug.Fail("No valid Date");

                if(!TimeSpan.TryParse(values[6], out response.AirTime))
                    System.Diagnostics.Debug.Fail("No valid TimeStamp");

                response.setAction(values[7]);
                response.title = values[8].Trim();
                if(values.Length == 11)
                    response.code = "NONE";
                else if(values.Length == 12)
                {
                    value = values[9].ToUpper().Trim();
                    response.code = value;
                    if (response.code == "")
                        response.code = "NONE";

                    if (response.action != entryAction.SKIPPED)
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
                            System.Diagnostics.Debug.Fail("No valid TimeStamp");
                        }
                        else
                        {
                            response.inpoint = timeSpan;
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
                    response.AdvertiserID = AdvertiserID.KEA;
                if (response.title.ToUpper().Contains("EXTERN"))
                    response.AdvertiserID = AdvertiserID.EXTERN;
                if (response.title.ToUpper().Contains("FING"))
                    response.AdvertiserID = AdvertiserID.FINGER;
                if (response.title.ToUpper().Contains("FINGER"))
                    response.AdvertiserID = AdvertiserID.FINGER;
                if (response.title.ToUpper().Contains("RTVM"))
                    response.AdvertiserID = AdvertiserID.RTVM;
                if (response.title.ToUpper().Contains("TV OOST"))
                    response.AdvertiserID = AdvertiserID.TV_OOST;
            }
            else
            {
                return null;
            }
            

            if(response.title.Contains("(") && response.title.Contains(")"))
            {
                int indexLeft = response.title.LastIndexOf("(")+1;
                int indexRight = response.title.LastIndexOf(")");/*
                if (response.title.EndsWith("(Kea"))
                   // response.AdvertiserID = "KEA";
                else
                    //response.AdvertiserID = response.title.Substring(indexLeft, indexRight - indexLeft).ToUpper(); ;
                if (response.AdvertiserID.Contains("IDEM"))
                    //response.AdvertiserID = "";*/
            }
            if (!PrintoutValues.Contains(response.code))
                PrintoutValues.Add(response.code);

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
    }

}
