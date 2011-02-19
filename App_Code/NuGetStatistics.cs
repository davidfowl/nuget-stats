using System;
using System.Collections.Generic;
using System.Web.Caching;
using WebMatrix.Data;

public static class NuGetStatistics {
    public static void Update(Cache cache) {
        using (var db = Database.Open("Stats")) {
            DateTime? lastLog = db.QueryValue("Select top 1 LogTime from Stats order by LogTime desc");

            if (!lastLog.HasValue || DateTime.UtcNow.Subtract(lastLog.Value).TotalMinutes > 45) {
                var stats = PackageRepository.GetCurrentStatistics(cache);

                db.Execute("Insert into Stats (LogTime, Downloads, UniquePackages, TotalPackages) values (@0, @1, @2, @3)",
                    DateTime.UtcNow,
                    stats.TotalDownloads,
                    stats.UniqueCount,
                    stats.TotalCount);
            }
        }
    }

    public static IEnumerable<dynamic> GetStatsHistory(int total = 10) {
        total = Math.Min(total, 10000);
        using (var db = Database.Open("Stats")) {
            return db.Query("Select top " + total + " * from Stats order by LogTime desc");
        }
    }

    public static MetaStatistics GetMetaStatistics(Cache cache) {
        MetaStatistics metaStats = (MetaStatistics)cache.Get("metastats");
        if (metaStats == null) {
            metaStats = GetMetaStatistics(PackageRepository.GetCurrentStatistics(cache));

            cache.Insert("metastats",
                        metaStats,
                        null,
                        DateTime.Now + TimeSpan.FromSeconds(60),
                        Cache.NoSlidingExpiration);
        }

        return metaStats;
    }

    public static MetaStatistics GetMetaStatistics(Statistics stats) {
        var metaStats = new MetaStatistics();
        using (var db = Database.Open("Stats")) {
            metaStats.HourDownloads = stats.TotalDownloads - (int)db.QueryValue("Select top 1 Downloads from Stats where LogTime < DateAdd(hh, -1, GetDate()) order by LogTime desc");
            metaStats.DayPackages = stats.TotalCount - (int)db.QueryValue("Select top 1 TotalPackages from Stats where LogTime < DateAdd(day, -1, GetDate()) order by LogTime desc");
        }
        return metaStats;
    }
}