using System;
using MiniPlayerWpf;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace MusicLibTests
{
    [TestClass]
    public class MusicLibUnitTest
    {
        private Song defaultSong = new Song
        {
            Id = 9,
            Artist = "Bob",
            Album = "Fire",
            Filename = "test.mp3",
            Genre = "cool",
            Length = "123",
            Title = "Best Song"
        };

        [TestMethod]
        public void TestSongIds()
        {
            MusicLib musicLib = new MusicLib();
            var songIds = new List<string>(musicLib.SongIds);

            // Make sure there are 7 IDs
            Assert.AreEqual(7, songIds.Count);

            // Make sure all IDs but 4 are present
            int idNum = 1;
            foreach (string id in songIds)
            {
                // There is no ID 4
                if (idNum == 4)
                    idNum++;

                Assert.AreEqual(idNum.ToString(), id);
                idNum++;
            }
        }

        [TestMethod]
        public void AddSongTest()
        {
            MusicLib musicLib = new MusicLib();

            // ID auto-increments
            int expectedId = 9;
            int actualId = musicLib.AddSong(defaultSong);
            Assert.AreEqual(expectedId, actualId, "ID of added song was unexpectedly " + actualId);

            // See if we can get back the song that was just added
            Song song = musicLib.GetSong(expectedId);
            Assert.AreEqual(defaultSong, song, "Got back unexpected song: " + song);
        }

        [TestMethod]
        public void DeleteSongTest()
        {
            MusicLib musicLib = new MusicLib();

            // Delete a song that already exists
            int songId = 8;
            bool songDeleted = musicLib.DeleteSong(songId);
            Assert.IsTrue(songDeleted, "Song should have been deleted");

            // Verify the song is not in the library anymore
            Song s = musicLib.GetSong(songId);
            Assert.IsNull(s, "Returned song should be null because it doesn't exist");
        }

        public void DeleteMissingSongTest()
        {
            MusicLib musicLib = new MusicLib();

            // Delete a song that does not exist
            int songId = 111;
            bool songDeleted = musicLib.DeleteSong(songId);
            Assert.IsFalse(songDeleted, "Non-existing song should not have been deleted");
        }

        [TestMethod]
        public void GetSongTest()
        {
            MusicLib musicLib = new MusicLib();

            // Add the default song and then retrieve it
            int songId = musicLib.AddSong(defaultSong);
            Song song = musicLib.GetSong(songId);
            Assert.AreEqual(defaultSong, song);            
        }

        [TestMethod]
        public void GetMissingSongTest()
        {
            MusicLib musicLib = new MusicLib();

            // Get a song that doesn't exist
            int songId = 111;
            Song song = musicLib.GetSong(songId);
            Assert.IsNull(song);
        }

        [TestMethod]
        public void UpdateSongTest()
        {
            MusicLib musicLib = new MusicLib();

            // Update a song's title
            int songId = musicLib.AddSong(defaultSong);
            defaultSong.Title = "Horrible Song";
            bool isUpdated = musicLib.UpdateSong(songId, defaultSong);
            Assert.IsTrue(isUpdated, "Update should have worked; got back " + isUpdated);

            // Make sure the song's title was really changed
            Song s = musicLib.GetSong(songId);
            Assert.AreEqual(defaultSong, s, "Title was not updated properly");
        }

        [TestMethod]
        public void UpdateMissingSongTest()
        {
            MusicLib musicLib = new MusicLib();

            // Try to update a song with a bad ID
            int songId = 111;
            bool isUpdated = musicLib.UpdateSong(songId, defaultSong);
            Assert.IsFalse(isUpdated, "Update should NOT have worked; got back " + isUpdated);
        }
    }
}
