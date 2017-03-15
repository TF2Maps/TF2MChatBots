using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamBotLite
{
    public class MapCollection
    {
        /// <summary>
        /// This class is a wrapper for an observerable collection, in order to ensure validity of inputs
        /// </summary>
        /// 
        ObservableCollection<Map> mapList = new ObservableCollection<Map>();
        string tablename = "Deleted Maps";

        HTMLFileFromArrayListiners listiner;
        public MapCollection(ObservableCollection<Map> inputlist , HTMLFileFromArrayListiners HtmlListiner)
        {
            this.mapList = inputlist;
            this.listiner = HtmlListiner;

            mapList.CollectionChanged += MapList_CollectionChanged;

            TableDataValue MapEntry = new TableDataValue();
            MapEntry.VisibleValue = "Map Name";

            TableDataValue ReasonEntry = new TableDataValue();
            ReasonEntry.VisibleValue = "Extra info";

            TableDataValue Owner = new TableDataValue();
            Owner.VisibleValue = "Owner";

            TableDataValue[] Data = new TableDataValue[] { MapEntry, ReasonEntry , Owner};
            HtmlListiner.SetTableHeader(tablename, Data);

            AllowOnlyUploadedMaps = false;
        }

        private void MapList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            CollectionChanged(this, e);
        }
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private ObservableCollection<Map> GetMaplist() { return mapList; }

        public Map GetMap(int position) { return mapList[position]; }

        public IReadOnlyList<Map> GetAllMaps() { return mapList; }

        public Map GetMapByFilename(string filename) { return mapList.FirstOrDefault(x => x.Filename == filename); }

        public void RemoveMap(Map map , string reason) {
            TableDataValue Owner = new TableDataValue();
            Owner.VisibleValue = map.SubmitterName;
            Owner.HoverText = map.Submitter.ToString();

            mapList.Remove(map);
            TableDataValue MapEntry = new TableDataValue();
            MapEntry.VisibleValue = map.Filename;

            TableDataValue ReasonEntry = new TableDataValue();
            ReasonEntry.VisibleValue = reason;

            

            TableDataValue[] Data = new TableDataValue[] { MapEntry, ReasonEntry , Owner };
            
            listiner.AddEntryWithLimit(tablename, Data, 3);
        }

        public void RemoveMap(int position) { mapList.RemoveAt(position); }

        public void InsertMap(int position, Map map) { mapList.Insert(position, map); }

        public int GetSize() { return mapList.Count; }

        public bool AllowOnlyUploadedMaps { get; private set; }
        public string ForceMapsToBeUploadedErrorResponse { get; private set; }

        public void RestrictMapsToBeUploaded (bool ForceMapsToBeUploaded, string ErrorMessage)
        {
            this.AllowOnlyUploadedMaps = ForceMapsToBeUploaded;
            ForceMapsToBeUploadedErrorResponse = ErrorMessage;
        }

        public IEnumerator<Map> GetEnumerator ()
        {
            return mapList.GetEnumerator();
        }

        public string AddMap(Map map)
        {
            MapValidityCheck MapCheck = CheckIfValid(map);

            if (MapCheck.IsValid)
            {
                mapList.Add(map);
                return string.Format("{0} Has been added to the list!", map.Filename);
            }
            else {
                return MapCheck.ReturnMessage;
            }
        }

        public string UpdateMap(string MapName, Map NewMapData, ChatroomEntity User)
        {
            MapValidityCheck MapCheck = CheckIfValid(NewMapData);
            UpdateMapValidityCheck Updatecheck = GetMapPositionInList(MapName);
            string ReturnMessage;

            if (MapCheck.IsValid && Updatecheck.MapExistsInList)
            {
                int entry = Updatecheck.MapEntry;

                if (mapList[entry].Submitter.ToString().Equals(User.identifier.ToString()) | User.Rank == ChatroomEntity.AdminStatus.True)
                {
                    mapList[entry].Filename = NewMapData.Filename ?? mapList[entry].Filename;
                    mapList[entry].DownloadURL = NewMapData.DownloadURL;
                    mapList[entry].Notes = NewMapData.Notes ?? mapList[entry].Notes;
                    mapList[entry].Uploaded = NewMapData.Uploaded;

                    ReturnMessage = "Successfully updated the map!";
                }
                else
                {
                    ReturnMessage = "You are either not an admin or this isn't your map!";
                }
            }
            else {
                ReturnMessage = MapCheck.ReturnMessage;
            }

            return ReturnMessage;
        }
        
        private UpdateMapValidityCheck GetMapPositionInList(string mapName)
        {
            UpdateMapValidityCheck ReturnData = new UpdateMapValidityCheck();

            int index = 0;
            foreach (Map Entry in mapList)
            {
                if (Entry.Filename.Equals(mapName))
                {
                    ReturnData.MapExistsInList = true;
                    ReturnData.MapEntry = index;
                }
                else
                {
                    index++;
                }
            }
            return ReturnData;
        }

        private MapValidityCheck CheckIfValid(Map map)
        {
            MapValidityCheck ValidityCheck = new MapValidityCheck();
            try
            {
                if (map.Uploaded == true) { }
                else {
                    if (AllowOnlyUploadedMaps) {
                        throw new ArgumentException(ForceMapsToBeUploadedErrorResponse);
                    }

                    if (map.DownloadURL != null & map.DownloadURL.StartsWith("http", StringComparison.OrdinalIgnoreCase)) { }
                    else {
                        throw new ArgumentException("Your map isn't uploaded, and doesn't include a URL!");
                    }
                }

                if (map.Filename.Any(c => char.IsUpper(c))) {
                    throw new ArgumentException("It includes an uppercase letter");
                }

                if (map.Filename.Length > 27) {
                    throw new ArgumentException("It includes too many characters: " + "27");
                }

                if (mapList.Any(m => m.Filename.Equals(map.Filename))) {
                    throw new ArgumentException("Your map has been rejected as it already exists in the map list!");
                }
            }

            catch (ArgumentException ex) { ValidityCheck.SetInvalid(ex.Message); }
            catch (NullReferenceException ex) { throw new ArgumentException("Map isn't uploaded"); }

            return ValidityCheck;
        }

        

    }

    public class Map
    {
        public Object Submitter { get; set; }
        public string SubmitterName { get; set; }

        public string DownloadURL { get; set; }
        public string Notes { get; set; }
        public bool Uploaded { get; set; }
        public string Filename { get; set; }

        public bool IsOwner(Object other)
        {
            return other.ToString().Equals(Submitter.ToString());
        }
    }

    public class MapValidityCheck
    {
        public bool IsValid = true;
        public string ReturnMessage;

        public void SetInvalid(string message)
        {
            IsValid = false;
            ReturnMessage = message;
        }
    }

    public class UpdateMapValidityCheck
    {
        public bool MapExistsInList = false;
        public int MapEntry;

    }
}
