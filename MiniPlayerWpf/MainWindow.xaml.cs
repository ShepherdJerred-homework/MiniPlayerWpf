using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MiniPlayerWpf {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private MusicLib musicLib;
        private MediaPlayer mediaPlayer;

        public MainWindow() {
            InitializeComponent();

            mediaPlayer = new MediaPlayer();

            try {
                musicLib = new MusicLib();
            } catch (Exception e) {
                MessageBox.Show("Error loading file: " + e.Message);
            }

            musicLib.PrintAllTables();

            // Put the ids to a ObservableCollection which has a Remove method for use later.
            // The UI will update itself automatically if any changes are made to this collection.
            var items = new ObservableCollection<string>(musicLib.SongIds);

            // Bind the song IDs to the combo box
            songIdComboBox.ItemsSource = items;

            // Select the first item
            if (songIdComboBox.Items.Count > 0) {
                songIdComboBox.SelectedItem = songIdComboBox.Items[0];
                deleteButton.IsEnabled = true;
            }
        }

        private void openButton_Click(object sender, RoutedEventArgs e) {
            // Configure open file dialog box
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.FileName = "";
            openFileDialog.DefaultExt = "*.wma;*.wav;*mp3";
            openFileDialog.Filter =
                "Media files|*.mp3;*.m4a;*.wma;*.wav|MP3 (*.mp3)|*.mp3|M4A (*.m4a)|*.m4a|Windows Media Audio (*.wma)|*.wma|Wave files (*.wav)|*.wav|All files|*.*";

            // Show open file dialog box
            var result = openFileDialog.ShowDialog();

            // Load the selected song
            if (result == true) {
                songIdComboBox.IsEnabled = false;
                var s = GetSongDetails(openFileDialog.FileName);
                if (s != null) {
                    titleTextBox.Text = s.Title;
                    artistTextBox.Text = s.Artist;
                    albumTextBox.Text = s.Album;
                    genreTextBox.Text = s.Genre;
                    lengthTextBox.Text = s.Length;
                    filenameTextBox.Text = s.Filename;
                    mediaPlayer.Open(new Uri(s.Filename));
                    addButton.IsEnabled = true;
                }
            }
        }

        private Song GetSongDetails(string filename) {
            Song song = null;

            try {
                // PM> Install-Package taglib
                // http://stackoverflow.com/questions/1750464/how-to-read-and-write-id3-tags-to-an-mp3-in-c
                var file = TagLib.File.Create(filename);

                song = new Song {
                    Title = file.Tag.Title,
                    Artist = file.Tag.AlbumArtists.Length > 0 ? file.Tag.AlbumArtists[0] : "",
                    Album = file.Tag.Album,
                    Genre = file.Tag.Genres.Length > 0 ? file.Tag.Genres[0] : "",
                    Length = file.Properties.Duration.Minutes + ":" + file.Properties.Duration.Seconds,
                    Filename = filename
                };

                return song;
            }
            catch (TagLib.UnsupportedFormatException) {
                DisplayError("You did not select a valid song file.");
            }
            catch (Exception ex) {
                DisplayError(ex.Message);
            }

            return song;
        }

        private void DisplayError(string errorMessage) {
            MessageBox.Show(errorMessage, "MiniPlayer", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void addButton_Click(object sender, RoutedEventArgs e) {
            Console.WriteLine("Adding song");

            Song song = new Song();

            song.Title = titleTextBox.Text;
            song.Artist = artistTextBox.Text;
            song.Album = albumTextBox.Text;
            song.Filename = filenameTextBox.Text;
            song.Length = lengthTextBox.Text;
            song.Genre = genreTextBox.Text;
            
            // Now that the id has been set, add it to the combo box
            songIdComboBox.IsEnabled = true;
            int id = musicLib.AddSong(song);
            (songIdComboBox.ItemsSource as ObservableCollection<string>).Add(id.ToString());
            songIdComboBox.SelectedIndex = songIdComboBox.Items.Count - 1;

            // There is at least one song that can be deleted
            deleteButton.IsEnabled = true;
        }

        private void updateButton_Click(object sender, RoutedEventArgs e) {
            var songId = songIdComboBox.SelectedItem.ToString();
            Console.WriteLine("Updating song " + songId);

            Song song = new Song();

            song.Title = titleTextBox.Text;
            song.Artist = artistTextBox.Text;
            song.Album = albumTextBox.Text;
            song.Genre = genreTextBox.Text;
            song.Length = lengthTextBox.Text;
            song.Filename = filenameTextBox.Text;

            musicLib.UpdateSong(Convert.ToInt32(songId), song);
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e) {
            if (MessageBox.Show("Are you sure you want to delete this song?", "MiniPlayer",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
                var songId = songIdComboBox.SelectedItem.ToString();
                Console.WriteLine("Deleting song " + songId);

                // Search the primary key for the selected song and delete it from 
                // the song table
                var table = musicDataSet.Tables["song"];
                table.Rows.Remove(table.Rows.Find(songId));

                // Remove from playlist_song every occurance of songId.
                // Add rows to a separate list before deleting because we'll get an exception
                // if we try to delete more than one row while looping through table.Rows

                var rows = new List<DataRow>();
                table = musicDataSet.Tables["playlist_song"];
                foreach (DataRow row in table.Rows)
                    if (row["song_id"].ToString() == songId.ToString())
                        rows.Add(row);

                foreach (var row in rows)
                    row.Delete();

                // Remove the song from the list box and select the next item
                (songIdComboBox.ItemsSource as ObservableCollection<string>).Remove(
                    songIdComboBox.SelectedItem.ToString());
                if (songIdComboBox.Items.Count > 0) {
                    songIdComboBox.SelectedItem = songIdComboBox.Items[0];
                }
                else {
                    // No more songs to display
                    deleteButton.IsEnabled = false;
                    titleTextBox.Text = "";
                    artistTextBox.Text = "";
                    albumTextBox.Text = "";
                    genreTextBox.Text = "";
                    lengthTextBox.Text = "";
                    filenameTextBox.Text = "";
                }
            }
        }

        private void playButton_Click(object sender, RoutedEventArgs e) {
            mediaPlayer.Open(new Uri(filenameTextBox.Text));
            mediaPlayer.Play();
        }

        private void stopButton_Click(object sender, RoutedEventArgs e) {
            mediaPlayer.Stop();
        }

        private void showDataButton_Click(object sender, RoutedEventArgs e) {
            PrintAllTables();
        }

        private void saveButton_Click(object sender, RoutedEventArgs e) {
            // Save music.xml in the same directory as the exe
            Console.WriteLine("Saving music.xml");
            musicLib.Save();
        }

        private void songIdComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            // Display the selected song
            if (songIdComboBox.SelectedItem != null) {
                Console.WriteLine("Load song " + songIdComboBox.SelectedItem);
                var songId = Convert.ToInt32(songIdComboBox.SelectedItem);
                var table = musicDataSet.Tables["song"];

                // Only one row should be selected
                foreach (var row in table.Select("id=" + songId)) {
                    titleTextBox.Text = row["title"].ToString();
                    artistTextBox.Text = row["artist"].ToString();
                    albumTextBox.Text = row["album"].ToString();
                    genreTextBox.Text = row["genre"].ToString();
                    lengthTextBox.Text = row["length"].ToString();
                    filenameTextBox.Text = row["filename"].ToString();
                }
            }
        }
    }
}