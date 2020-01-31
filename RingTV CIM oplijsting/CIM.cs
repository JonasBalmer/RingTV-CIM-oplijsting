using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;

namespace RingTV_CIM_oplijsting
{
    [DataContract]
    public class CIMItem
    {
        #region PRIVATES
        [DataMember]
        private string _channel;
        [DataMember]
        private DateTime _theoreticalBroadcastTime;
        [DataMember]
        private TimeSpan _theoreticalDuration;
        [DataMember]
        private TimeSpan _technicalDuration;
        [DataMember]
        private string _blockcode;
        [DataMember]
        private string _blockType;
        [DataMember]
        private string _advertiser;
        [DataMember]
        private string _filmID;
        [DataMember]
        private string _campaignID;
        [DataMember]
        private string _FilmDescription;
        [DataMember]
        private Uri _file;
        #endregion

        #region Properties
        public string Channel { get { return _channel; } }
        public string TheoreticalBroadcastDay { get { return _theoreticalBroadcastTime.ToString("yyyyMMdd"); } }
        public string TheoreticalBroadcastHour { get { return _theoreticalBroadcastTime.ToString("HHmmss"); } }
        public string TheoreticalDuration { get { return _theoreticalDuration.ToString("hhmmss"); } }
        public string TechnicalDuration { get { return _technicalDuration.ToString("hhmmss"); } }
        public string BlockCode { get { return _blockcode; } }
        public string BlockType { get { return _blockType.ToString(); } }
        public string AdvertiserID { get { return _advertiser.ToString(); } }
        public string FilmID { get { return _filmID; } }
        public string CampaignID { get { return _campaignID; } }
        public string FilmDescription
        {
            get { return _FilmDescription; }
            set { _FilmDescription = value; }
        }
        public string FileName { get { return Path.GetFileName(_file.AbsolutePath); } }
        public string FilePath { get { return _file.AbsolutePath; } }

        public DateTime BroadcastTime { get { return _theoreticalBroadcastTime; } }

        #endregion

        #region setters
        public void SetTheoreticalBroadcastTime(DateTime broadcastTime)
        {
            _theoreticalBroadcastTime = broadcastTime;
        }
        public void SetTheoreticalDuration(TimeSpan duration)
        {
            _theoreticalDuration = duration;
            _technicalDuration = duration;
        }
        public void SetBlockCode(string blockCode)
        {
            _blockcode = blockCode;
        }
        public void SetBlockType(string blockType)
        {
            _blockType = blockType;
            if (!CIMTypes.BlockTypes.Contains(blockType))
                CIMTypes.BlockTypes.Add(blockType);
        }
        public void SetAdvertiserID(string advertiser)
        {
            _advertiser = advertiser;
            if (!CIMTypes.AdvertiserIDs.Contains(advertiser))
                CIMTypes.AdvertiserIDs.Add(advertiser);
        }
        public void SetFilmID(string filmID)
        {
            _filmID = filmID;
        }
        public void SetCampaignID(string campaignID)
        {
            _campaignID = campaignID;
        }
        public void SetFile(string path)
        {
            _file = new Uri(path);
        }
        #endregion

        #region constructors
        public CIMItem(string Channel)
        {
            _channel = Channel;
        }
        public CIMItem()
        {
            _channel = "RingTV";
        }
        #endregion
    }

    [DataContract]
    public class CIMList
    {
        [DataMember]
        private List<CIMItem> _items = new List<CIMItem>();
        public void Save(Uri Path)
        {
            try
            {
                XmlDocument xml = new XmlDocument();
                DataContractSerializer serializer = new DataContractSerializer(this.GetType());
                using (FileStream writer = new FileStream(Path.AbsolutePath, FileMode.Create, FileAccess.Write))
                {
                    serializer.WriteObject(writer, this);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                throw;
            }
        }

        public List<CIMItem> Items { get { return _items; } }

        internal void Add(LogEntry entry)
        {
            CIMItem item = new CIMItem("RingTV");
            item.SetAdvertiserID(entry.AdvertiserID);
            item.SetBlockCode(entry.code);
            item.SetBlockType(entry.BlokType);
            item.SetCampaignID("");
            item.SetFilmID(entry.title);

            item.SetTheoreticalBroadcastTime(entry.TimeStamp.Add(entry.AirTime));
            item.SetTheoreticalDuration(entry.duration);
            try
            {
                if (entry.file.OriginalString != "")
                    item.SetFile(entry.file.AbsolutePath);
            }
            catch (Exception e)
            {
            }
            _items.Add(item);
        }
        internal void Add(CIMItem item)
        {
            _items.Add(item);
        }
    }

    internal class CIMFactory
    {
        internal static Dictionary<string, CIMList> GetMonthlyCIMS(List<StudioLog> LogList)
        {
            return GetCIMS(LogList, "yyyy-MM");
        }
        internal static Dictionary<string, CIMList> GetDailyCIMs(List<StudioLog> LogList)
        {
            return GetCIMS(LogList, "yyyy-MM-dd");
        }
        internal static Dictionary<string, CIMList> GetYearlyCIMS(List<StudioLog> LogList)
        {
            return GetCIMS(LogList, "yyyy");
        }

        private static Dictionary<string, CIMList> GetCIMS(List<StudioLog> logList, string stringFormat)
        {
            Dictionary<string, CIMList> response = new Dictionary<string, CIMList>();
            foreach (var log in logList)
            {
                foreach (var entry in log.entries)
                {
                    string key = entry.TimeStamp.ToString(stringFormat);
                    if (response.ContainsKey(key))
                        response[key].Add(entry);
                    else
                    {
                        response.Add(key, new CIMList());
                        response[key].Add(entry);
                    }
                }
            }
            return response;
        }
        private static Dictionary<string, CIMList> GetCIMS(CIMList list, string stringFormat)
        {
            Dictionary<string, CIMList> response = new Dictionary<string, CIMList>();
            foreach (var item in list.Items)
            {
                string key = item.BroadcastTime.ToString(stringFormat);
                if (response.ContainsKey(key))
                    response[key].Add(item);
                else
                {
                    response.Add(key, new CIMList());
                    response[key].Add(item);
                }
            }
            return response;
        }

        internal static IEnumerable<KeyValuePair<string, CIMList>> GetMonthlyCIMS(CIMList list)
        {
            return GetCIMS(list, "yyyy-MM");
        }

        internal static IEnumerable<KeyValuePair<string, CIMList>> GetDailyCIMs(CIMList list)
        {
            return GetCIMS(list, "yyyy-MM-dd");
        }
    }

    static class CIMTypes
    {
        public static List<string> BlockTypes = new List<string>();
        public static List<string> AdvertiserIDs = new List<string>();
        public static List<string> EntryActions = new List<string>();
    }

    static class CIMSpreadSheetFactory
    {
        public static   Application app = new Application();
        private static List<Workbook> workbooks = new List<Workbook>();
        private static List<Worksheet> worksheets = new List<Worksheet>();

        public static Workbook getSimpleWorkbook(CIMList Source)
        {
            Workbook response = app.Workbooks.Add();
            workbooks.Add(response);
            Worksheet worksheet = (Worksheet)response.Worksheets.Item[1];
            worksheets.Add(worksheet);
            addCIMHeader(worksheet);
            for (int i = 0; i < Source.Items.Count; i++)
                addCIMRow(worksheet, Source.Items[i], i + 2);
                
            return response;
        }

        private static void addCIMRow(Worksheet worksheet, CIMItem cIMItem, int row)
        {

            Range formatRange;
            formatRange = worksheet.get_Range("a"+row, "m"+row);
            formatRange.NumberFormat = "@";
            worksheet.Cells[row, 1] = cIMItem.Channel;
            worksheet.Cells[row, 2] = cIMItem.TheoreticalBroadcastDay;
            worksheet.Cells[row, 3] = cIMItem.TheoreticalBroadcastHour;
            worksheet.Cells[row, 4] = cIMItem.TheoreticalDuration;
            worksheet.Cells[row, 5] = cIMItem.TechnicalDuration;
            worksheet.Cells[row, 6] = cIMItem.BlockCode;
            worksheet.Cells[row, 7] = cIMItem.BlockType;
            worksheet.Cells[row, 8] = cIMItem.AdvertiserID;
            worksheet.Cells[row, 9] = cIMItem.FilmID;
            worksheet.Cells[row, 10] = cIMItem.CampaignID;
            worksheet.Cells[row, 11] = cIMItem.FilmDescription;
        }

        public static void releaseResources()
        {
            app.Quit();
            foreach (var item in worksheets)
                Marshal.ReleaseComObject(item);
            foreach (var item in workbooks)
                Marshal.ReleaseComObject(item);
            Marshal.ReleaseComObject(app);
        }

        private static void addCIMHeader(Worksheet worksheet)
        {
            worksheet.Cells[1, 1] = "Channel";
            worksheet.Cells[1, 2] = "Theoretical Day";
            worksheet.Cells[1, 3] = "Theoretical Hour";
            worksheet.Cells[1, 4] = "Theoretical Duration";
            worksheet.Cells[1, 5] = "Technical Duration";
            worksheet.Cells[1, 6] = "Blockcode";
            worksheet.Cells[1, 7] = "Blocktype";
            worksheet.Cells[1, 8] = "Advertiser ID";
            worksheet.Cells[1, 9] = "Film ID";
            worksheet.Cells[1, 10] = "Campain ID";
            worksheet.Cells[1, 11] = "Film Description";
            worksheet.Cells[1, 12] = "Bruto price";
            worksheet.Cells[1, 13] = "Price 30s";

        }
    }
}
