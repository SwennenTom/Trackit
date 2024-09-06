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

        public async Task<int> DeleteTrackerAsync(Tracker tracker)
        {
            await _database.Table<TrackerValues>().DeleteAsync(v => v.tracker_id == tracker.tracker_id);
            await _database.Table<TrackerSettings>().DeleteAsync(s => s.tracker_id == tracker.tracker_id);
            return await _database.DeleteAsync(tracker);
        }

        public async Task<int> UpdateTrackerAsync(Tracker tracker)
        {
            return await _database.UpdateAsync(tracker);
        }

        public async Task<List<Tracker>> GetTrackersAsync()
        {
            return await _database.Table<Tracker>().ToListAsync();
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
            return await _database.Table<TrackerValues>().Where(v => v.tracker_id == trackerId).ToListAsync();
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
            return await _database.Table<TrackerSettings>().Where(s => s.tracker_id == trackerId).FirstOrDefaultAsync();
        }

        #endregion
    }
}
