using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace SteamBotLite
{
    public class MapCollection
    {
        private IHTMLFileFromArrayPasser listiner;

        /// <summary>
        /// This class is a wrapper for an observerable collection, in order to ensure validity of inputs
        /// </summary>
        ///
        private ObservableCollection<Map> mapList = new ObservableCollection<Map>();

        private string tablename = "Deleted Maps";

        public MapCollection(ObservableCollection<Map> inputlist, IHTMLFileFromArrayPasser HtmlListiner)
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

            TableDataValue[] Data = new TableDataValue[] { MapEntry, ReasonEntry, Owner };



            SetTableHeader TableHandler = new SetTableHeader();
            TableHandler.TableIdentifier = tablename;
            TableHandler.Header = Data;

            HtmlListiner.HandleCommand(TableHandler);

            AllowOnlyUploadedMaps = false;
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public bool AllowOnlyUploadedMaps { get; private set; }

        public string ForceMapsToBeUploadedErrorResponse { get; private set; }

        public string AddMap(Map map)
        {
            MapValidityCheck MapCheck = CheckIfValid(map, true);

            if (MapCheck.IsValid)
            {
                mapList.Add(map);
                return string.Format("{0} Has been added to the list!", map.Filename);
            }
            else
            {
                return MapCheck.ReturnMessage;
            }
        }

        public IReadOnlyList<Map> GetAllMaps()
        {
            return mapList;
        }

        public IEnumerator<Map> GetEnumerator()
        {
            return mapList.GetEnumerator();
        }

        public Map GetMap(int position)
        {
            return mapList[position];
        }

        public Map GetMapByFilename(string filename)
        {
            return mapList.FirstOrDefault(x => x.Filename == filename);
        }

        public int GetSize()
        {
            return mapList.Count;
        }

        public void InsertMap(int position, Map map)
        {
            mapList.Insert(position, map);
        }

        public void RemoveMap(Map map, string reason)
        {
            TableDataValue Owner = new TableDataValue();
            Owner.VisibleValue = map.SubmitterName;
            Owner.HoverText = map.Submitter.ToString();

            mapList.Remove(map);
            TableDataValue MapEntry = new TableDataValue();
            MapEntry.VisibleValue = map.Filename;

            TableDataValue ReasonEntry = new TableDataValue();
            ReasonEntry.VisibleValue = reason;

            TableDataValue[] Data = new TableDataValue[] { MapEntry, ReasonEntry, Owner };

            AddWebsiteEntry AddEntryCommand = new AddWebsiteEntry();
            AddEntryCommand.Identifier = tablename;
            AddEntryCommand.Data = Data;
            AddEntryCommand.limit = 10;

            listiner.HandleCommand(AddEntryCommand);
        }

        public void RemoveMap(int position)
        {
            mapList.RemoveAt(position);
        }

        public void RestrictMapsToBeUploaded(bool ForceMapsToBeUploaded, string ErrorMessage)
        {
            this.AllowOnlyUploadedMaps = ForceMapsToBeUploaded;
            ForceMapsToBeUploadedErrorResponse = ErrorMessage;
        }

        public string UpdateMap(string MapName, Map NewMapData, ChatroomEntity User)
        {
            MapValidityCheck MapCheck = CheckIfValid(NewMapData, (NewMapData.Filename != MapName));
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
            else
            {
                ReturnMessage = MapCheck.ReturnMessage;
            }

            return ReturnMessage;
        }

        private bool CheckIfStringIsNumbers(string input)
        {
            int x;
            return int.TryParse(input, out x);
        }

        private MapValidityCheck CheckIfValid(Map map, bool checkDuplicates)
        {
            MapValidityCheck ValidityCheck = new MapValidityCheck();
            try
            {
                if (map.Uploaded == true) { }
                else
                {
                    if (AllowOnlyUploadedMaps)
                    {
                        throw new ArgumentException(ForceMapsToBeUploadedErrorResponse);
                    }

                    if (map.DownloadURL != null & map.DownloadURL.StartsWith("http", StringComparison.OrdinalIgnoreCase)) { }
                    else
                    {
                        throw new ArgumentException("Your map isn't uploaded, and doesn't include a URL!");
                    }
                }

                if (string.IsNullOrWhiteSpace(map.Filename))
                {
                    throw new ArgumentException("You must include a filename!");
                }

                if (CheckIfStringIsNumbers(map.Filename))
                {
                    throw new ArgumentException("You must include more than numbers!");
                }
                if (map.Filename.Any(c => char.IsUpper(c)))
                {
                    throw new ArgumentException("It includes an uppercase letter");
                }

                if (map.Filename.Length > 27)
                {
                    throw new ArgumentException("It includes too many characters: " + "27");
                }

                if (checkDuplicates && mapList.Any(m => m.Filename.Equals(map.Filename)))
                {
                    throw new ArgumentException("Your map has been rejected as it already exists in the map list!");
                }
            }
            catch (ArgumentException ex) { ValidityCheck.SetInvalid(ex.Message); }
            catch (NullReferenceException ex) { throw new ArgumentException("Map isn't uploaded"); }

            return ValidityCheck;
        }

        private ObservableCollection<Map> GetMaplist()
        {
            return mapList;
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

        private void MapList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged(this, e);
        }
    }
}
