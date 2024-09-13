using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using Trackit.Models;

namespace Trackit.Data
{
    public class Database
    {
        private readonly SQLiteAsyncConnection _database;

        public Database(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<Tracker>().Wait();
            _database.CreateTableAsync<TrackerValues>().Wait();
            _database.CreateTableAsync<TrackerSettings>().Wait();

        }
        #region Tracker Logic
        public async Task<int> AddTrackerAsync(Tracker tracker)
        {
            return await _database.InsertAsync(tracker);
        }

        public async Task<int> DeleteTrackerAsync(int trackerId)
        {
            await _database.Table<TrackerValues>().DeleteAsync(v => v.tracker_id == trackerId);

            await _database.Table<TrackerSettings>().DeleteAsync(s => s.tracker_id == trackerId);

            return await _database.Table<Tracker>().DeleteAsync(t => t.tracker_id == trackerId);
        }


        public async Task<int> UpdateTrackerAsync(Tracker tracker)
        {
            return await _database.UpdateAsync(tracker);
        }

        public async Task<List<Tracker>> GetTrackersAsync()
        {
            return await _database.Table<Tracker>().ToListAsync();
        }

        public async Task<Tracker>GetTrackerAsync(int trackerId)
        {
            return await _database
                .Table<Tracker>()
                .Where(t => t.tracker_id == trackerId)
                .OrderByDescending(s =>s.created_at)
                .FirstOrDefaultAsync();
        }

        #endregion

        #region Value Logic
        public async Task<int> AddValueAsync(TrackerValues value)
        {
            return await _database.InsertAsync(value);
        }

        public async Task<int> DeleteValueAsync(TrackerValues value)
        {
            return await _database.DeleteAsync(value);
        }

        public async Task<int> UpdateValueAsync (TrackerValues value)
        {
            return await _database.UpdateAsync(value);
        }

        public async Task<List<TrackerValues>> GetValuesForTrackerAsync(int trackerId)
        {
            return await _database.Table<TrackerValues>()
                .Where(v => v.tracker_id == trackerId)
                .OrderBy(s => s.date)
                .ToListAsync();
        }

        #endregion

        #region Settings Logic
        public async Task<int> AddSettingsAsync(TrackerSettings setting)
        {
            return await _database.InsertAsync(setting);
        }

        public async Task<int> DeleteSettingsAsync(TrackerSettings setting)
        {
            return await _database.DeleteAsync(setting);
        }

        public async Task<int> UpdateSettingsAsync(TrackerSettings setting)
        {
            return await _database.UpdateAsync(setting);
        }

        public async Task<TrackerSettings> GetSettingsAsync(int trackerId)
        {
            //return await _database.Table<TrackerSettings>().Where(s => s.tracker_id == trackerId).FirstOrDefaultAsync();

            // Log the trackerId to ensure it's being passed correctly
            System.Diagnostics.Debug.WriteLine($"Getting settings for trackerId: {trackerId}");

            // Check if any records exist with the given trackerId
            var count = await _database.Table<TrackerSettings>().Where(s => s.tracker_id == trackerId).CountAsync();
            System.Diagnostics.Debug.WriteLine($"Number of settings found: {count}");

            // Fetch the actual settings
            var settings = await _database.Table<TrackerSettings>().Where(s => s.tracker_id == trackerId).FirstOrDefaultAsync();

            if (settings != null)
            {
                // Log the settings for debugging purposes
                System.Diagnostics.Debug.WriteLine($"Settings found: MinThreshold = {settings.min_threshhold}, MaxThreshold = {settings.max_threshold}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No settings found for the given trackerId.");
            }

            return settings;
        }

        #endregion

        #region TestData

        public async Task AddTestDataAsync()
        {
            var tracker1 = new Tracker { name = "Glucose Tracker", description = "Tracks glucose levels", created_at = DateTime.Now };
            var tracker2 = new Tracker { name = "Weight Tracker", description = "Tracks weight over time", created_at = DateTime.Now };

            // Insert dummy trackers into the database
            await AddTrackerAsync(tracker1);
            await AddTrackerAsync(tracker2);

            // Create some dummy values for tracker1 (Glucose Tracker)
            var value1 = new TrackerValues { tracker_id = tracker1.tracker_id, value = 4, date = DateTime.Now.AddDays(-2) };
            var value2 = new TrackerValues { tracker_id = tracker1.tracker_id, value = 8, date = DateTime.Now.AddDays(-1) };
            var value3 = new TrackerValues { tracker_id = tracker1.tracker_id, value = 6, date = DateTime.Now };

            // Insert dummy values into the database
            await AddValueAsync(value1);
            await AddValueAsync(value2);
            await AddValueAsync(value3);

            // Create some dummy values for tracker2 (Weight Tracker)
            var value4 = new TrackerValues { tracker_id = tracker2.tracker_id, value = 4, date = DateTime.Now.AddDays(-2) };
            var value5 = new TrackerValues { tracker_id = tracker2.tracker_id, value = 5, date = DateTime.Now.AddDays(-1) };
            var value6 = new TrackerValues { tracker_id = tracker2.tracker_id, value = 7, date = DateTime.Now };

            // Insert dummy values into the database
            await AddValueAsync(value4);
            await AddValueAsync(value5);
            await AddValueAsync(value6);

            // Create dummy settings for tracker1 (Glucose Tracker)
            var settings1 = new TrackerSettings { tracker_id = tracker1.tracker_id, min_threshhold = 4, max_threshold = 8 };

            // Insert dummy settings into the database
            await AddSettingsAsync(settings1);

            // Create dummy settings for tracker2 (Weight Tracker)
            var settings2 = new TrackerSettings { tracker_id = tracker2.tracker_id, min_threshhold = 4, max_threshold = 8 };

            // Insert dummy settings into the database
            await AddSettingsAsync(settings2);
        }

        #endregion
    }
}
