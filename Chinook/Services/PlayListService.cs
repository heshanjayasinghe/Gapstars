using System;
using System.Linq;
using System.Threading.Tasks;

using Chinook.Models;
using Chinook.ClientModels;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Chinook.Services
{
    public class PlayListService : IPlayListService
    {
        private readonly IDbContextFactory<ChinookContext> DbFactory;
        public Chinook.ClientModels.Playlist Playlist;
        ChinookContext DbContext;
        public string CurrentUser;
        public Artist Artist;
        public List<PlaylistTrack> Tracks;
        public PlayListService(IDbContextFactory<ChinookContext> contextFactory)
        {
            DbFactory = contextFactory;
        }

        public async Task Initialize(string CurrentUserId)
        {
            DbContext = await DbFactory.CreateDbContextAsync();
            CurrentUser = CurrentUserId;
        }
        public async Task<ClientModels.Playlist> InitializePlayListData(long PlaylistId, string CurrentUserId)
        {
            Playlist = DbContext.Playlists
                .Include(a => a.Tracks).ThenInclude(a => a.Album).ThenInclude(a => a.Artist)
                .Where(p => p.PlaylistId == PlaylistId)
                .Select(p => new ClientModels.Playlist()
                {
                    Name = p.Name,
                    Tracks = p.Tracks.Select(t => new ClientModels.PlaylistTrack()
                    {
                        AlbumTitle = t.Album.Title,
                        ArtistName = t.Album.Artist.Name,
                        TrackId = t.TrackId,
                        TrackName = t.Name,
                        IsFavorite = t.Playlists.Where(p => p.UserPlaylists.Any(up => up.UserId == CurrentUser && up.Playlist.Name == "Favorites")).Any()
                    }).ToList()
                })
                .FirstOrDefault();

                return Playlist;
        }

        public async Task<ArtistDto> InitializArtisteData(long ArtistId)
        {

            Artist = DbContext.Artists.SingleOrDefault(a => a.ArtistId == ArtistId);

            Tracks = DbContext.Tracks.Where(a => a.Album.ArtistId == ArtistId)
                .Include(a => a.Album)
                .Select(t => new PlaylistTrack()
                {
                    AlbumTitle = (t.Album == null ? "-" : t.Album.Title),
                    TrackId = t.TrackId,
                    TrackName = t.Name,
                    IsFavorite = t.Playlists.Where(p => p.UserPlaylists.Any(up => up.UserId == CurrentUser && up.Playlist.Name == "Favorites")).Any()
                })
                .ToList();
            return new ArtistDto(){
                Artist=Artist,
                Tracks=Tracks
            };
        }

        public void AddToFavourties() { }

        public void RemoveFromFavourties() { }

        public void AddToExistingPlayList(long playListId, PlaylistTrack song)
        {
            Chinook.Models.Playlist playList = DbContext.Playlists.SingleOrDefault(items => items.PlaylistId == playListId);
            if (playList != null)
            {
                var track = DbContext.Tracks.SingleOrDefault(items => items.TrackId == song.TrackId);
                try
                {
                    playList.Tracks.Add(track);
                    DbContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        public void RemoveTrackFromExistingPlayList(long playListId, long trackId)
        {
            Chinook.Models.Playlist playList = DbContext.Playlists.
                                                         Include(items => items.Tracks).
                                                         SingleOrDefault(items => items.PlaylistId == playListId);
            if (playList != null)
            {
                var track = DbContext.Tracks.SingleOrDefault(items => items.TrackId == trackId);
                try
                {
                    playList.Tracks.Remove(track);
                    DbContext.SaveChanges();
                }
                catch (Exception ex)
                {

                }
            }
        }

        public void RemovePlayList(long playListId)
        {
            Chinook.Models.Playlist playList = DbContext.Playlists.
                                                         Include(items => items.UserPlaylists).
                                                         Include(items => items.Tracks).
                                                         SingleOrDefault(items => items.PlaylistId == playListId);
            if (playList != null)
            {

                try
                {
                    playList.Tracks.Clear();
                    DbContext.Playlists.Remove(playList);
                    DbContext.SaveChanges();
                }
                catch (Exception ex)
                {

                }
            }
        }


        public async Task CreateNewPlayList(string name, PlaylistTrack song)
        {
            var track = DbContext.Tracks.SingleOrDefault(items => items.TrackId == song.TrackId);
            var currentcontexts = DbContext.Playlists.ToList();
            long playListId = DateTime.Now.Ticks;

            var newPlaylist = new Chinook.Models.Playlist()
            {
                Name = name,
                PlaylistId = playListId,
                Tracks = new List<Track>() { track }
            };

            try
            {
                var result = DbContext.Playlists.Add(newPlaylist);
                DbContext.UserPlaylists.Add(new UserPlaylist()
                {
                    PlaylistId = playListId,
                    UserId = CurrentUser,

                });
                DbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<Chinook.Models.UserPlaylist>> GetAllPlayLists(string userId)
        {

            try
            {
                var results = DbContext.UserPlaylists.Include(item => item.Playlist)
                            .Where(item => item.UserId == userId).ToList();
                return results;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<Artist>> GetArtists()
        {  
            return DbContext.Artists.Include(a=>a.Albums).ToList();
        }

    }
}