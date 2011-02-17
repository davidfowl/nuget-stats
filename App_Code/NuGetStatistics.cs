using System;
using System.Collections.Generic;
using System.Web.Caching;
using WebMatrix.Data;

public static class NuGetStatistics {
    public static void Update(Cache Cache) {
        using (var db = Database.Open("Stats")) {
            DateTime? lastLog = db.QueryValue("Select top 1 LogTime from Stats order by LogTime desc");

            if (!lastLog.HasValue || DateTime.UtcNow.Subtract(lastLog.Value).TotalMinutes > 45) {
                var stats = PackageRepository.GetCurrentStatistics(Cache);

                db.Execute("Insert into Stats (LogTime, Downloads, UniquePackages, TotalPackages) values (@0, @1, @2, @3)",
                    DateTime.UtcNow,
                    stats.TotalDownloads,
                    stats.UniqueCount,
                    stats.TotalCount);
            }
        }
    }

    public static IEnumerable<dynamic> GetStatistics(int total = 10) {
        total = Math.Min(total, 10000);
        using (var db = Database.Open("Stats")) {
            return db.Query("Select top "+ total + " * from Stats order by LogTime desc");
        }
    }
}