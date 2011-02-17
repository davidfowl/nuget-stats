using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Caching;
using NuGet;

public class PackageRepository {
    private static readonly object _lockObject = new object();
    private static readonly DataServicePackageRepository _repository = new DataServicePackageRepository(new Uri("http://packages.nuget.org/v1/FeedService.svc"));

    private static IList<DataServicePackage> GetPackages(Cache cache) {
        // Try to load if from the cache
        var packages = (IList<DataServicePackage>)cache.Get("packages");

        // Double check lock
        if (packages == null) {
            lock (_lockObject) {
                packages = (IList<DataServicePackage>)cache.Get("packages");

                if (packages == null) {
                    // If we still don't have anything cached then get the package list and store it.
                    packages = _repository.GetPackages().AsEnumerable().Cast<DataServicePackage>().ToList();

                    cache.Insert("packages",
                                  packages,
                                  null,
                                  DateTime.Now + TimeSpan.FromSeconds(20),
                                  Cache.NoSlidingExpiration);
                }
            }
        }

        return packages;
    }

    public static Statistics GetCurrentStatistics(Cache cache) {
        var packages = GetPackages(cache);

        return new Statistics {
            TotalCount = packages.Count,
            UniqueCount = packages.GroupBy(p => p.Id).Count(),
            TotalDownloads = packages.GroupBy(p => p.Id).Select(g => g.First().DownloadCount).Sum(),
            LatestPackages = (from p in packages
                              orderby p.LastUpdated descending
                              select new {
                                  Id = p.Id,
                                  Version = p.Version.ToString(),
                                  Url = FixDetailsUrl(p.GalleryDetailsUrl),
                                  Desc = p.Description
                              }).Take(5)
        };
    }

    private static string FixDetailsUrl(Uri uri) {
        // The official feed has a bug where the url contains a reference to localhost.
        // This is a temporary workaround.
        return uri.OriginalString.Replace("http://localhost:777/", "http://nuget.org/");
    }
}