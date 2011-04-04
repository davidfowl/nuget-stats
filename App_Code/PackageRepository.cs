using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Caching;
using NuGet;

public class PackageRepository {
    private static readonly DataServicePackageRepository _repository = new DataServicePackageRepository(new Uri("http://packages.nuget.org/v1/FeedService.svc"));
    internal static readonly TimeSpan CacheTime = TimeSpan.FromMinutes(5);

    private static IList<DataServicePackage> GetPackages(Cache cache) {
        return cache.GetOrCreate<IList<DataServicePackage>>("packages", GetPackages, CacheTime);
    }

    private static IList<DataServicePackage> GetPackages() {
        return _repository.GetPackages().AsEnumerable().Cast<DataServicePackage>().ToList();
    }

    public static Statistics GetCurrentStatistics(Cache cache) {
        var packages = GetPackages(cache);

        return new Statistics {
            TotalCount = packages.Count,
            UniqueCount = packages.GroupBy(p => p.Id).Count(),
            TotalDownloads = packages.GroupBy(p => p.Id).Select(g => g.First().DownloadCount).Sum(),
            LatestPackages = (from p in packages
                              orderby p.LastUpdated descending
                              select new StatsPackage {
                                  Id = p.Id,
                                  Version = p.Version,
                                  Url = FixGalleryUrl(p)
                              }).Take(5),
            TopPackages = (from g in packages.GroupBy(p => p.Id)
                           let downloadCount = g.First().DownloadCount
                           let latest = (from p in g
                                         let version = Version.Parse(p.Version)
                                         orderby version descending
                                         select p).First()
                           orderby downloadCount descending
                           select new StatsPackage {
                               Id = latest.Id,
                               DownloadCount = downloadCount,
                               Url = FixGalleryUrl(latest)
                           }).Take(5)
        };
    }

    private static string FixGalleryUrl(DataServicePackage package) {
        // Workaround for the gallery url until it is fixed.
        return "http://nuget.org/List/Packages/" + package.Id + "/" + package.Version.ToString();
    }
}