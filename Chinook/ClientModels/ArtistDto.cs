using Chinook.Models;
namespace Chinook.ClientModels;

public class ArtistDto
{
    public Artist Artist { get; set; }
    public List<PlaylistTrack> Tracks { get; set; }
}