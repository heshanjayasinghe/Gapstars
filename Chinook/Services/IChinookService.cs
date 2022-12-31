using Chinook.ClientModels;
using Chinook.Models;

namespace Chinook.Services
{
    public interface IChinookService
    {
        void AddToExistingPlayList(global::System.Int64 playListId, PlaylistTrack song);
        void AddToFavourties();
        Task CreateNewPlayList(global::System.String name, PlaylistTrack song);
        Task<List<UserPlaylist>> GetAllPlayLists(global::System.String userId);
        Task<ArtistDto> InitializArtisteData(global::System.Int64 ArtistId);
        Task Initialize(global::System.String CurrentUserId);
        Task<ClientModels.Playlist>  InitializePlayListData(global::System.Int64 PlaylistId, global::System.String CurrentUserId);
        void RemoveFromFavourties();
        void RemovePlayList(global::System.Int64 playListId);
        void RemoveTrackFromExistingPlayList(global::System.Int64 playListId, global::System.Int64 trackId);
        Task<List<Artist>> GetArtists();
    }
}