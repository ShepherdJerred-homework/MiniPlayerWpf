using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPlayerWpf {
    class MusicLib {
        private DataSet musicDataSet;

        public EnumerableRowCollection<string> SongIds {
            get {
                EnumerableRowCollection<string> ids = from row in musicDataSet.Tables["song"].AsEnumerable()
                    orderby row["id"]
                    select row["id"].ToString();
                return ids;
            }
        }

        // The constructor reads the music.xsd and music.xml files into the DataSet
        public MusicLib() {
            musicDataSet = new DataSet();
            musicDataSet.ReadXmlSchema("music.xsd");
            musicDataSet.ReadXml("music.xml");

            Console.WriteLine("Total songs = " + musicDataSet.Tables["song"].Rows.Count);

            // Get a list of all song IDs
            var songs = musicDataSet.Tables["song"];
            var ids = from row in songs.AsEnumerable()
                orderby row["id"]
                select row["id"].ToString();
        }

        // Adds a song to the music library and returns the song's ID. The song  
        // parameter's ID is also set to the auto‐generated ID.
        public int AddSong(Song s) {
            // Add the selected file to the song table
            var table = musicDataSet.Tables["song"];
            var row = table.NewRow();
            table.Rows.Add(row);
            return Convert.ToInt32(row["id"]);
        }

        // Return a Song for the given song ID or null if no song was not found.
        public Song GetSong(int songId) {
            var table = musicDataSet.Tables["song"];

            var rows = table.Select("id=" + songId);

            if (rows.Length > 0) {
                // Only one row should be selected
                Song song = new Song();
                var row = rows.First();
                song.Title = row["title"].ToString();
                song.Artist = row["artist"].ToString();
                song.Album = row["album"].ToString();
                song.Genre = row["genre"].ToString();
                song.Length = row["length"].ToString();
                song.Filename = row["filename"].ToString();
                return song;
            }
            else {
                return null;
            }
        }

        // Update the given song with the given song ID.  Returns true if the song
        // was updated, false if it could not because the song ID was not found.
        public bool UpdateSong(int songId, Song song) {
            var table = musicDataSet.Tables["song"];

            var query = table.Select("id=" + songId);

            if (query.Length > 0) {
                foreach (var row in table.Select("id=" + songId)) {
                    row["title"] = song.Title;
                    row["artist"] = song.Artist;
                    row["album"] = song.Album;
                    row["genre"] = song.Genre;
                    row["length"] = song.Length;
                    row["filename"] = song.Filename;
                }
                return true;
            }
            else {
                return false;
            }
        }

        // Delete a song given the song's ID. Return true if the song was  
        // successfully deleted, false if the song ID was not found.
        public bool DeleteSong(int songId) {
            var table = musicDataSet.Tables["song"];

            var query = table.Select("id=" + songId);

            if (query.Length > 0) {
                // Search the primary key for the selected song and delete it from 
                // the song table
                table.Rows.Remove(table.Rows.Find(songId));
                // Remove from playlist_song every occurance of songId.
                // Add rows to a separate list before deleting because we'll get an exception
                // if we try to delete more than one row while looping through table.Rows
                var rows = new List<DataRow>();
                table = musicDataSet.Tables["playlist_song"];
                foreach (DataRow row in table.Rows) {
                    if (row["song_id"].ToString() == songId.ToString()) {
                        rows.Add(row);
                    }
                }

                foreach (var row in rows) {
                    row.Delete();
                }
                return true;
            }
            else {
                return false;
            }
        }

        // Save the song database to the music.xml file
        public void Save() {
            musicDataSet.WriteXml("music.xml");
        }

        public void PrintAllTables() {
            foreach (DataTable table in musicDataSet.Tables) {
                Console.WriteLine("Table name = " + table.TableName);
                foreach (DataRow row in table.Rows) {
                    Console.WriteLine("Row:");
                    var i = 0;
                    foreach (var item in row.ItemArray) {
                        Console.WriteLine(" " + table.Columns[i].Caption + "=" + item);
                        i++;
                    }
                }
                Console.WriteLine();
            }
        }
    }
}