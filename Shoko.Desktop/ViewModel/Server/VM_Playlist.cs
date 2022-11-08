using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Xml.Serialization;
using GongSolutions.Wpf.DragDrop;
using Newtonsoft.Json;
using Shoko.Commons.Notification;
using Shoko.Desktop.Utilities;
using Shoko.Desktop.ViewModel.Helpers;
using Shoko.Models.Client;
using Shoko.Models.Enums;
using Shoko.Models.Server;

// ReSharper disable InconsistentNaming

namespace Shoko.Desktop.ViewModel.Server
{
    public class VM_Playlist : Playlist, IDropTarget, INotifyPropertyChanged, INotifyPropertyChangedExt
    {


        private static readonly Random epRandom = new Random();

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }
        [JsonIgnore, XmlIgnore]
        public ObservableCollection<VM_PlaylistItem> PlaylistObjects { get; set; }

        private VM_AniDB_Anime aniDB_Anime;
        [JsonIgnore, XmlIgnore]
        public VM_AniDB_Anime AniDB_Anime
        {
            get { return aniDB_Anime; }
            set
            {
                this.SetField(()=>aniDB_Anime,value);
            }
        }

        private VM_AnimeSeries_User series;
        [JsonIgnore, XmlIgnore]
        public VM_AnimeSeries_User Series
        {
            get { return series; }
            set
            {
                this.SetField(()=>series,value);
            }
        }

        private VM_AnimeEpisode_User nextEpisode;
        [JsonIgnore, XmlIgnore]
        public VM_AnimeEpisode_User NextEpisode
        {
            get { return nextEpisode; }
            set
            {
                this.SetField(()=>nextEpisode,value);
            }
        }

        private Boolean isReadOnly = true;
        [JsonIgnore, XmlIgnore]
        public Boolean IsReadOnly
        {
            get { return isReadOnly; }
            set
            {
                this.SetField(()=>isReadOnly,value);
            }
        }

        private Boolean isBeingEdited;
        [JsonIgnore, XmlIgnore]
        public Boolean IsBeingEdited
        {
            get { return isBeingEdited; }
            set
            {
                this.SetField(()=>isBeingEdited,value);
            }
        }

        public new string PlaylistName
        {
            get { return base.PlaylistName; }
            set
            {
                this.SetField(()=>base.PlaylistName,(r)=> base.PlaylistName = r, value);
            }
        }

        [JsonIgnore, XmlIgnore]
        public bool PlayWatchedBool => PlayWatched==1;

        public new int PlayWatched
        {
            get { return base.PlayWatched; }
            set
            {
                this.SetField(()=>base.PlayWatched,(r)=> base.PlayWatched = r, value, ()=>PlayWatched, ()=>PlayUnwatchedBool);
            }
        }
        public new int PlayUnwatched
        {
            get { return base.PlayUnwatched; }
            set
            {
                this.SetField(()=>base.PlayUnwatched,(r)=> base.PlayUnwatched = r, value, ()=>PlayUnwatched, ()=>PlayUnwatchedBool);
            }
        }
        [JsonIgnore, XmlIgnore]
        public bool PlayUnwatchedBool => PlayUnwatched==1;

        public void SetDependendProperties()
        {
            AniDB_Anime = null;
            Series = null;

            SetNextEpisode();
        }

        [JsonIgnore, XmlIgnore]
        public PlaylistPlayOrder DefaultPlayOrderEnum => (PlaylistPlayOrder)DefaultPlayOrder;


        public VM_Playlist()
        {
            PlaylistObjects = new ObservableCollection<VM_PlaylistItem>();
        }

       
        public void Save()
        {

            CL_Response<Playlist> resp = VM_ShokoServer.Instance.ShokoServices.SavePlaylist(this);

            if (!string.IsNullOrEmpty(resp.ErrorMessage))
            {
                Utils.ShowErrorMessage(resp.ErrorMessage);
            }


        }

        public void AddSeries(int animeSeriesID)
        {
            if (IsAlreadyInPlaylist(PlaylistItemType.AnimeSeries, animeSeriesID)) return;

            if (!string.IsNullOrEmpty(PlaylistItems))
                PlaylistItems += "|";

            PlaylistItems += $"{(int) PlaylistItemType.AnimeSeries};{animeSeriesID}";
        }

        public void RemoveSeries(int animeSeriesID)
        {
            if (string.IsNullOrEmpty(PlaylistItems)) return;

            string[] items = PlaylistItems.Split('|');
            PlaylistItems = "";

            // create a new list without the moved item
            foreach (string pitem in items)
            {
                string[] parms = pitem.Split(';');
                if (parms.Length != 2) continue;

                int objType;
                int objID;

                if (!int.TryParse(parms[0], out objType)) continue;
                if (!int.TryParse(parms[1], out objID)) continue;

                if (objType == (int)PlaylistItemType.AnimeSeries && objID == animeSeriesID)
                {
                    // remove this old item
                }
                else
                {
                    if (PlaylistItems.Length > 0) PlaylistItems += "|";
                    PlaylistItems += $"{objType};{objID}";
                }
            }
        }

        public void AddEpisode(int animeEpisodeID)
        {
            if (IsAlreadyInPlaylist(PlaylistItemType.Episode, animeEpisodeID)) return;

            if (!string.IsNullOrEmpty(PlaylistItems))
                PlaylistItems += "|";

            PlaylistItems += $"{(int) PlaylistItemType.Episode};{animeEpisodeID}";
        }

        public void RemoveEpisode(int animeEpisodeID)
        {
            if (string.IsNullOrEmpty(PlaylistItems)) return;

            string[] items = PlaylistItems.Split('|');
            PlaylistItems = "";

            // create a new list without the moved item
            foreach (string pitem in items)
            {
                string[] parms = pitem.Split(';');
                if (parms.Length != 2) continue;

                int objType;
                int objID;

                if (!int.TryParse(parms[0], out objType)) continue;
                if (!int.TryParse(parms[1], out objID)) continue;

                if (objType == (int)PlaylistItemType.Episode && objID == animeEpisodeID)
                {
                    // remove this old item
                }
                else
                {
                    if (PlaylistItems.Length > 0) PlaylistItems += "|";
                    PlaylistItems += $"{objType};{objID}";
                }
            }
        }

        public bool IsAlreadyInPlaylist(PlaylistItemType pType, int id)
        {
            string[] items = PlaylistItems.Split('|');

            foreach (string pitem in items)
            {
                string[] parms = pitem.Split(';');
                if (parms.Length != 2) continue;

                int objType;
                int objID;

                if (!int.TryParse(parms[0], out objType)) continue;
                if (!int.TryParse(parms[1], out objID)) continue;

                if (objType == (int)pType && objID == id)
                    return true;

            }

            return false;
        }

        private bool CanUseEpisode(VM_Playlist pl, VM_AnimeEpisode_User ep)
        {
            if (ep.Watched && pl.PlayWatchedBool && ep.HasFiles)
                return true;

            if (!ep.Watched && pl.PlayUnwatchedBool && ep.HasFiles)
                return true;

            return false;
        }

        public void SetNextEpisode()
        {
            SetNextEpisode(false);
        }

        public void SetNextEpisode(bool onlyRandom)
        {
            if (PlaylistObjects.Count == 0) return;

            // find the next episode to play
            NextEpisode = null;
            if (DefaultPlayOrderEnum == PlaylistPlayOrder.Sequential && !onlyRandom)
            {
                
                while (true)
                {
                    foreach (VM_PlaylistItem pli in PlaylistObjects)
                    {
                        if (pli.ItemType == PlaylistItemType.Episode)
                        {
                            VM_AnimeEpisode_User epTemp = pli.PlaylistItem as VM_AnimeEpisode_User;
                            if (CanUseEpisode(this, epTemp))
                            {
                                NextEpisode = epTemp;
                                break;
                            }
                        }

                        if (pli.ItemType == PlaylistItemType.AnimeSeries)
                        {
                            VM_AnimeSeries_User ser = (VM_AnimeSeries_User)pli.PlaylistItem;
                            VM_MainListHelper.Instance.UpdateAll();
                            ser.RefreshEpisodes();

                            List<VM_AnimeEpisode_User> eps = ser.AllEpisodes.OrderBy(a=>a.EpisodeType).ThenBy(a=>a.EpisodeNumber).ToList();

                            bool foundEp = false;
                            foreach (VM_AnimeEpisode_User epTemp in eps)
                            {
                                if (epTemp.EpisodeTypeEnum == EpisodeType.Episode || epTemp.EpisodeTypeEnum == EpisodeType.Special)
                                {
                                    if (CanUseEpisode(this, epTemp))
                                    {
                                        NextEpisode = epTemp;
                                        foundEp = true;
                                        break;
                                    }
                                }
                            }

                            if (foundEp) break;
                        }
                    }
                    break;
                }
            }
            else // random
            {
                // get all the candidate episodes
                List<VM_AnimeEpisode_User> canidateEps = new List<VM_AnimeEpisode_User>();

                foreach (VM_PlaylistItem pli in PlaylistObjects)
                {
                    if (pli.ItemType == PlaylistItemType.Episode)
                    {
                        VM_AnimeEpisode_User epTemp = pli.PlaylistItem as VM_AnimeEpisode_User;
                        if (CanUseEpisode(this, epTemp)) canidateEps.Add(epTemp);
                    }

                    if (pli.ItemType == PlaylistItemType.AnimeSeries)
                    {
                        VM_AnimeSeries_User ser = (VM_AnimeSeries_User) pli.PlaylistItem;
                        VM_MainListHelper.Instance.UpdateAll();
                        ser.RefreshEpisodes();

                        List<VM_AnimeEpisode_User> eps = ser.AllEpisodes;

                        foreach (VM_AnimeEpisode_User epTemp in eps)
                        {
                            if (epTemp.EpisodeTypeEnum == EpisodeType.Episode || epTemp.EpisodeTypeEnum == EpisodeType.Special)
                            {
                                if (CanUseEpisode(this, epTemp)) canidateEps.Add(epTemp);
                            }
                        }

                    }
                }

                // pick a random object from the play list
                if (canidateEps.Count > 0)
                {
                    NextEpisode = canidateEps[epRandom.Next(0, canidateEps.Count)];
                }

            }

            if (NextEpisode != null)
            {
                NextEpisode.SetTvDBInfo();
                NextEpisode.RefreshAnime();
                AniDB_Anime = NextEpisode.AniDBAnime;

                if (VM_MainListHelper.Instance.AllSeriesDictionary.ContainsKey(NextEpisode.AnimeSeriesID))
                    Series = VM_MainListHelper.Instance.AllSeriesDictionary[NextEpisode.AnimeSeriesID];
            }

        }

        public List<VM_AnimeEpisode_User> GetAllEpisodes(bool onlyRandom)
        {
            List<VM_AnimeEpisode_User> allEps = new List<VM_AnimeEpisode_User>();

            // find the next episode to play
            if (DefaultPlayOrderEnum == PlaylistPlayOrder.Sequential && !onlyRandom)
            {
                foreach (VM_PlaylistItem pli in PlaylistObjects)
                {
                    if (pli.ItemType == PlaylistItemType.Episode)
                    {
                        VM_AnimeEpisode_User epTemp = pli.PlaylistItem as VM_AnimeEpisode_User;
                        if (CanUseEpisode(this, epTemp))
                        {
                            allEps.Add(epTemp);
                        }
                    }

                    if (pli.ItemType == PlaylistItemType.AnimeSeries)
                    {
                        VM_AnimeSeries_User ser = (VM_AnimeSeries_User)pli.PlaylistItem;
                        VM_MainListHelper.Instance.UpdateAll();
                        ser.RefreshEpisodes();

                        List<VM_AnimeEpisode_User> eps = ser.AllEpisodes.OrderBy(a=>a.EpisodeType).ThenBy(a=>a.EpisodeNumber).ToList();

                        foreach (VM_AnimeEpisode_User epTemp in eps)
                        {
                            if (epTemp.EpisodeTypeEnum == EpisodeType.Episode || epTemp.EpisodeTypeEnum == EpisodeType.Special)
                            {
                                if (CanUseEpisode(this, epTemp))
                                {
                                    allEps.Add(epTemp);
                                }
                            }
                        }

                    }
                }
            }
            else // random
            {
                foreach (VM_PlaylistItem pli in PlaylistObjects)
                {
                    if (pli.ItemType == PlaylistItemType.Episode)
                    {
                        VM_AnimeEpisode_User epTemp = pli.PlaylistItem as VM_AnimeEpisode_User;
                        if (CanUseEpisode(this, epTemp)) allEps.Add(epTemp);
                    }

                    if (pli.ItemType == PlaylistItemType.AnimeSeries)
                    {
                        VM_AnimeSeries_User ser = (VM_AnimeSeries_User)pli.PlaylistItem;
                        VM_MainListHelper.Instance.UpdateAll();
                        ser.RefreshEpisodes();

                        List<VM_AnimeEpisode_User> eps = ser.AllEpisodes;

                        foreach (VM_AnimeEpisode_User epTemp in eps)
                        {
                            if (epTemp.EpisodeTypeEnum == EpisodeType.Episode || epTemp.EpisodeTypeEnum == EpisodeType.Special)
                            {
                                if (CanUseEpisode(this, epTemp)) allEps.Add(epTemp);
                            }
                        }

                    }
                }

                allEps.Shuffle();

            }

            return allEps;
        }

        public void PopulatePlaylistObjects()
        {
            PlaylistObjects.Clear();

            if (string.IsNullOrEmpty(PlaylistItems)) return;

            string[] items = PlaylistItems.Split('|');
            foreach (string pitem in items)
            {
                string[] parms = pitem.Split(';');
                if (parms.Length != 2) continue;

                int objType;
                int objID;

                if (!int.TryParse(parms[0], out objType)) continue;
                if (!int.TryParse(parms[1], out objID)) continue;

                if ((PlaylistItemType)objType == PlaylistItemType.AnimeSeries)
                {
                    VM_AnimeSeries_User ser;
                    if (VM_MainListHelper.Instance.AllSeriesDictionary.TryGetValue(objID, out ser) == false)
                    {
                        // get the series
                        ser = (VM_AnimeSeries_User)VM_ShokoServer.Instance.ShokoServices.GetSeries(objID, VM_ShokoServer.Instance.CurrentUser.JMMUserID);
                        if (ser != null)
                        {
                            VM_MainListHelper.Instance.AllSeriesDictionary[objID] = ser;
                        }
                    }

                    if (ser != null)
                        PlaylistObjects.Add(new VM_PlaylistItem(PlaylistID, PlaylistItemType.AnimeSeries, ser));
                }
                else
                {
                    // get the episode
                    VM_AnimeEpisode_User ep = (VM_AnimeEpisode_User)VM_ShokoServer.Instance.ShokoServices.GetEpisode(objID,
                        VM_ShokoServer.Instance.CurrentUser.JMMUserID);
                    if (ep != null)
                    {
                        //ep.SetTvDBInfo();
                        PlaylistObjects.Add(new VM_PlaylistItem(PlaylistID, PlaylistItemType.Episode, ep));
                    }
                }
            }

            SetDependendProperties();
        }

        public void Populate(VM_Playlist contract)
        {
            PlaylistID = contract.PlaylistID;
            PlaylistName = contract.PlaylistName;
            PlaylistItems = contract.PlaylistItems;
            DefaultPlayOrder = contract.DefaultPlayOrder;
            PlayWatched = contract.PlayWatched;
            PlayUnwatched = contract.PlayUnwatched;
        }

        public void DragEnter(IDropInfo dropInfo)
        {
            
        }

        public void DragOver(IDropInfo dropInfo)
        {
            VM_PlaylistItem sourceItem = dropInfo.Data as VM_PlaylistItem;
            VM_PlaylistItem targetItem = dropInfo.TargetItem as VM_PlaylistItem;

            if (sourceItem != null && targetItem != null)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                dropInfo.Effects = DragDropEffects.Copy;
            }
        }

        public void DragLeave(IDropInfo dropInfo)
        {
            
        }

        public void Drop(IDropInfo dropInfo)
        {
            PlaylistItemType itemType = PlaylistItemType.AnimeSeries;
            int objIDOld = -1;

            VM_PlaylistItem pli = dropInfo.Data as VM_PlaylistItem;
            if (pli == null) return;

            if (pli.ItemType == PlaylistItemType.Episode)
            {
                VM_AnimeEpisode_User ep = (VM_AnimeEpisode_User)pli.PlaylistItem;
                itemType = PlaylistItemType.Episode;
                objIDOld = ep.AnimeEpisodeID;
            }
            if (pli.ItemType == PlaylistItemType.AnimeSeries)
            {
                VM_AnimeSeries_User ep = (VM_AnimeSeries_User)pli.PlaylistItem;
                itemType = PlaylistItemType.AnimeSeries;
                objIDOld = ep.AnimeSeriesID;
            }

            int iType = (int)itemType;

            // find where this item was previously

            if (string.IsNullOrEmpty(PlaylistItems)) return;

            string[] items = PlaylistItems.Split('|');

            // create a new list without the moved item
            string newItemList = "";
            foreach (string pitem in items)
            {
                string[] parms = pitem.Split(';');
                if (parms.Length != 2) continue;

                int objType;
                int objID;

                if (!int.TryParse(parms[0], out objType)) continue;
                if (!int.TryParse(parms[1], out objID)) continue;

                if (objType == iType && objID == objIDOld)
                {
                    // skip the old item
                }
                else
                {
                    if (newItemList.Length > 0) newItemList += "|";
                    newItemList += $"{objType};{objID}";
                }
            }

            // insert the moved item into it's new position
            items = newItemList.Split('|');

            PlaylistItems = "";
            int curPos = 0;

            if (string.IsNullOrEmpty(newItemList))
            {
                // means there was only one item in list to begin with
                PlaylistItems += $"{iType};{objIDOld}";
            }
            else
            {
                foreach (string pitem in items)
                {
                    string[] parms = pitem.Split(';');
                    if (parms.Length != 2) continue;

                    int objType;
                    int objID;

                    int.TryParse(parms[0], out objType);
                    int.TryParse(parms[1], out objID);

                    if (curPos == dropInfo.InsertIndex)
                    {
                        // insert moved item
                        if (PlaylistItems.Length > 0) PlaylistItems += "|";
                        PlaylistItems += $"{iType};{objIDOld}";
                    }


                    if (PlaylistItems.Length > 0) PlaylistItems += "|";
                    PlaylistItems += $"{objType};{objID}";

                    curPos++;
                }
            }

            // moved to the end of the list
            if (dropInfo.InsertIndex > items.Length)
            {
                if (PlaylistItems.Length > 0) PlaylistItems += "|";
                PlaylistItems += $"{iType};{objIDOld}";
            }

            Save();
            PopulatePlaylistObjects();

            VM_PlaylistHelper.Instance.OnPlaylistModified(new PlaylistModifiedEventArgs(PlaylistID));
        }


    }
}
