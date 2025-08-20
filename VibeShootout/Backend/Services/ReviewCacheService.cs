using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace VibeShootout.Backend.Services
{
    public class ReviewCacheService
    {
        private readonly ConcurrentDictionary<string, DateTime> _recentChecksums;
        private readonly int _maxCacheSize;
        private readonly TimeSpan _cacheExpiry;

        public ReviewCacheService(int maxCacheSize = 100, TimeSpan? cacheExpiry = null)
        {
            _recentChecksums = new ConcurrentDictionary<string, DateTime>();
            _maxCacheSize = maxCacheSize;
            _cacheExpiry = cacheExpiry ?? TimeSpan.FromHours(1); // Default 1 hour expiry
        }

        public bool HasRecentReview(string checksum)
        {
            CleanupExpiredEntries();
            return _recentChecksums.ContainsKey(checksum);
        }

        public void AddReview(string checksum)
        {
            CleanupExpiredEntries();
            _recentChecksums.TryAdd(checksum, DateTime.UtcNow);
            
            // If cache is too large, remove oldest entries
            if (_recentChecksums.Count > _maxCacheSize)
            {
                var oldestEntries = _recentChecksums
                    .OrderBy(kvp => kvp.Value)
                    .Take(_recentChecksums.Count - _maxCacheSize)
                    .ToList();

                foreach (var entry in oldestEntries)
                {
                    _recentChecksums.TryRemove(entry.Key, out _);
                }
            }
        }

        private void CleanupExpiredEntries()
        {
            var cutoffTime = DateTime.UtcNow - _cacheExpiry;
            var expiredKeys = _recentChecksums
                .Where(kvp => kvp.Value < cutoffTime)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                _recentChecksums.TryRemove(key, out _);
            }
        }

        public int CacheSize => _recentChecksums.Count;
        
        public void ClearCache()
        {
            _recentChecksums.Clear();
        }
    }
}