using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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

            Console.WriteLine("Parsing CSV's");

            List<StudioLog> CSVs = GetCSVs(studioLogPath);

            Console.WriteLine("Creating CIM objects");

            var YearlyCimList = CIMFactory.GetYearlyCIMS(CSVs);
            PrintOutput(CSVs);
            CSVs.Clear();
            Console.WriteLine("Creating CIM excel sheets");
            foreach (var yearly in YearlyCimList)
                foreach (var monthly in CIMFactory.GetMonthlyCIMS(yearly.Value))
                    foreach (var daily in CIMFactory.GetDailyCIMs(monthly.Value))
                    {
                        Directory.CreateDirectory("E:\\" + yearly.Key + "\\" + monthly.Key + "\\");
                        string filename = "E:\\" + yearly.Key + "\\" + monthly.Key + "\\" + daily.Key + ".xls";
                        if (!File.Exists(filename))
                        {
                            var spreadsheet = CIMSpreadSheetFactory.getSimpleWorkbook(daily.Value);
                            spreadsheet.SaveAs(filename, Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookNormal);
                            spreadsheet.Close();
                            Console.WriteLine(daily.Key + ".xls created");
                        }
                    }

            CIMSpreadSheetFactory.releaseResources();

            foreach(var list in YearlyCimList)
                list.Value.Save(new Uri("E:\\test"+list.Key +".xml"));



            Console.ReadLine();

        }

        private static void PrintOutput(List<StudioLog> CSVs)
        {
            double numAdvertisers = 0;
            double numEntries = 0;
            Dictionary<string, double> Advertisers = new Dictionary<string, double>();
            Dictionary<string, double> Codes = new Dictionary<string, double>();
            Dictionary<string, double> Actions = new Dictionary<string, double>();
            Dictionary<string, double> BlockTypes = new Dictionary<string, double>();
            int Year = 2011;
            foreach (var log in CSVs)
            {
                foreach (var entry in log.entries)
                {
                    if (Year != entry.TimeStamp.Year)
                    {
                        Trace.WriteLine("");
                        Trace.WriteLine("");
                        Trace.WriteLine(Year);
                        Trace.WriteLine("--------------------------------------------------------------------------------------------------------------------");
                        var SortedAdvertisers = Advertisers.ToList();
                        SortedAdvertisers.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));
                        foreach (var adv in SortedAdvertisers)
                        {
                            Trace.WriteLine((int)adv.Value + " entries with " + adv.Key + " = " + Math.Round((adv.Value) * 100 / numAdvertisers, 2) + " % ");
                        }
                        Trace.WriteLine("Of " + (int)numAdvertisers + " advertisers = " + Math.Round(numAdvertisers * 100 / numEntries, 2) + " % ");
                        Trace.WriteLine("");
                        var SortedActions = Actions.ToList();
                        SortedActions.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));
                        foreach (var act in SortedActions)
                        {
                            Trace.WriteLine((int)act.Value + " entries with " + act.Key + " = " + Math.Round((act.Value) * 100 / numAdvertisers, 2) + " % ");
                        }
                        Trace.WriteLine("Of " + (int)numEntries + " actions");
                        Trace.WriteLine("");
                        var sortedCodes = Codes.ToList();
                        sortedCodes.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));
                        foreach (var code in sortedCodes)
                        {
                            Trace.WriteLine((int)code.Value + " entries with " + code.Key + " = " + Math.Round((code.Value) * 100 / numAdvertisers, 2) + " % ");
                        }
                        Trace.WriteLine("Of " + (int)numEntries + " codes");
                        Trace.WriteLine("");
                        var sortedBlockTypes = BlockTypes.ToList();
                        sortedBlockTypes.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));
                        foreach (var code in sortedBlockTypes)
                        {
                            Trace.WriteLine((int)code.Value + " entries with " + code.Key + " = " + Math.Round((code.Value) * 100 / numAdvertisers, 2) + " % ");
                        }
                        Trace.WriteLine("Of " + (int)numEntries + " codes");
                        Trace.WriteLine("");
                        Year = entry.TimeStamp.Year;
                        numAdvertisers = 0;
                        numEntries = 0;
                        Advertisers.Clear();
                        Codes.Clear();
                        Actions.Clear();
                        BlockTypes.Clear();
                    }
                    numEntries++;
                    if (Codes.ContainsKey(entry.code))
                        Codes[entry.code]++;
                    else
                        Codes.Add(entry.code, 1);

                    if (Advertisers.ContainsKey(entry.AdvertiserID))
                        Advertisers[entry.AdvertiserID]++;
                    else
                        Advertisers.Add(entry.AdvertiserID, 1);

                    if (Actions.ContainsKey(entry.action))
                        Actions[entry.action]++;
                    else
                        Actions.Add(entry.action, 1);

                    if (BlockTypes.ContainsKey(entry.BlokType))
                        BlockTypes[entry.BlokType]++;
                    else
                        BlockTypes.Add(entry.BlokType, 1);
                    numAdvertisers++;
                }
            }
            Trace.WriteLine("");
            Trace.WriteLine("");
            Trace.WriteLine(Year);
            Trace.WriteLine("");
            foreach (var adv in Advertisers)
            {
                Trace.WriteLine((int)adv.Value + " entries with " + adv.Key + " = " + Math.Round(((adv.Value) * 100 / numAdvertisers), 2) + " % ");
            }
            Trace.WriteLine("Of " + (int)numAdvertisers + " advertisers = " + Math.Round((numAdvertisers * 100 / numEntries), 2) + " % ");
            Trace.WriteLine("");
            foreach (var act in Actions)
            {
                Trace.WriteLine((int)act.Value + " entries with " + act.Key + " = " + Math.Round(((act.Value) * 100 / numAdvertisers), 2) + " % ");
            }
            Trace.WriteLine("Of " + (int)numEntries + " actions");
            Trace.WriteLine("");
            foreach (var code in Codes)
            {
                Trace.WriteLine((int)code.Value + " entries with " + code.Key + " = " + Math.Round(((code.Value) * 100 / numAdvertisers), 2) + " % ");
            }
            Trace.WriteLine("Of " + (int)numEntries + " codes");
            Trace.WriteLine("");
            Trace.WriteLine("");
            var nsortedBlockTypes = BlockTypes.ToList();
            nsortedBlockTypes.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));
            foreach (var code in nsortedBlockTypes)
            {
                Trace.WriteLine((int)code.Value + " entries with " + code.Key + " = " + Math.Round((code.Value) * 100 / numAdvertisers, 2) + " % ");
            }
            Trace.WriteLine("Of " + (int)numEntries + " codes");
            Trace.WriteLine("");
        }

        private static List<StudioLog> GetCSVs(string studioLogPath)
        {
            List<StudioLog> response = new List<StudioLog>();
            foreach(string filePath in System.IO.Directory.GetFiles(studioLogPath))
            {
                var studiolog = StudioLog.ParseCSV(filePath);
                response.Add(studiolog);
            }

            return response;
        }



    }

}
