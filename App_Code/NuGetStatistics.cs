using System;
using System.Collections.Generic;
using System.Web.Caching;
using WebMatrix.Data;

public static class NuGetStatistics {
    private const string UpdatedStatsCacheKey = "updated_stats";
    public static void Update(Cache cache) {
        UpdateCore(cache);
    }

    private static void UpdateCore(Cache cache) {
        cache.GetOrCreate<object>(UpdatedStatsCacheKey, () => {
            using (var db = Database.Open("Stats")) {
                DateTime? lastLog = db.QueryValue("Select top 1 LogTime from Stats order by LogTime desc");
                if (!lastLog.HasValue || DateTime.UtcNow.Subtract(lastLog.Value).TotalMinutes > 30) {
                    Statistics stats = PackageRepository.GetCurrentStatistics(cache);

                    db.Execute("Insert into Stats (LogTime, Downloads, UniquePackages, TotalPackages) values (GETUTCDATE(), @0, @1, @2)",
                        stats.TotalDownloads,
                        stats.UniqueCount,
                        stats.TotalCount);
                }
            }

            return new object();

        }, TimeSpan.FromMinutes(5));
    }

    public static IEnumerable<dynamic> GetStatsHistory(int total = 10) {
        total = Math.Min(total, 10000);
        using (var db = Database.Open("Stats")) {
            return db.Query("Select top " + total + " * from Stats order by LogTime desc");
        }
    }

    public static MetaStatistics GetMetaStatistics(Cache cache) {
        return cache.GetOrCreate<MetaStatistics>("metastats", () => GetMetaStatisticsCore(cache), PackageRepository.CacheTime);
    }

    private static MetaStatistics GetMetaStatisticsCore(Cache cache) {
        Statistics stats = PackageRepository.GetCurrentStatistics(cache);
        return GetMetaStatistics(stats);
    }

    public static MetaStatistics GetMetaStatistics(Statistics stats) {
        var metaStats = new MetaStatistics();
        using (var db = Database.Open("Stats")) {
            int? hourDownloads = stats.TotalDownloads - (int?)db.QueryValue("Select top 1 Downloads from Stats where LogTime < DateAdd(hh, -1, GETUTCDATE())  order by LogTime desc");
            int? dayPackages = stats.TotalCount - (int?)db.QueryValue("Select top 1 TotalPackages from Stats where LogTime < DateAdd(day, -1, GETUTCDATE()) order by LogTime desc");

            if (dayPackages.HasValue) {
                metaStats.DayPackages = Math.Max(0, dayPackages.Value);
            }

            if (hourDownloads.HasValue) {
                metaStats.HourDownloads = Math.Max(0, hourDownloads.Value);
            }
        }
        return metaStats;
    }
}