using System.Collections.ObjectModel;
using NUnit.Framework;
using TIDALDL_UI.Else;
using TidalLib;

namespace TIDALDL_UI_TESTS
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void GetPaddedTrackNumTest()
        {
            var track = new Track { TrackNumber = 3 };
            var album = new Album { Tracks = new ObservableCollection<Track>() };
            album.Tracks.Add(track);

            string trackNum = Tools.GetPaddedTrackNum(track, album, null);

            // minimum padding is 2, so even if the album has only 1 track, it will still be prefixed with one zero
            Assert.AreEqual("03", trackNum);

            for(int i = 0; i < 10; i++)
                album.Tracks.Add(track);

            // now the album contains 11 tracks, we should still get "03"
            trackNum = Tools.GetPaddedTrackNum(track, album, null);
            Assert.AreEqual("03", trackNum);

            for (int i = 0; i < 100; i++)
                album.Tracks.Add(track);

            // now the album contains more than 100 tracks
            trackNum = Tools.GetPaddedTrackNum(track, album, null);
            Assert.AreEqual("003", trackNum);

            album.Tracks = null; // test case when we download a separate track, not from an album
            trackNum = Tools.GetPaddedTrackNum(track, album, null);
            Assert.AreEqual("03", trackNum); // default padding of 2

            Playlist playlist = new Playlist { Tracks = new ObservableCollection<Track>() };
            playlist.Tracks.Add(track);

            trackNum = Tools.GetPaddedTrackNum(track, null, playlist);

            // the track will be 1st track in the playlist, track.TrackNum is ignored and the index of the track in the playlist is used
            Assert.AreEqual("01", trackNum);

            for (int i = 0; i < 100; i++)
                playlist.Tracks.Add(track);

            // now the playlist contains more than 100 tracks
            trackNum = Tools.GetPaddedTrackNum(track, null, playlist);
            Assert.AreEqual("001", trackNum);
        }
    }
}