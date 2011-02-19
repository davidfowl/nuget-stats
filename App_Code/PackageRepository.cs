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
                                Version = p.Version,
                                Url = FixGalleryUrl(p.GalleryDetailsUrl)
                            }).Take(5),
            TopPackages = (from g in packages.GroupBy(p => p.Id)
                          let downloadCount = g.Sum(c => c.DownloadCount)
                          let latest = (from p in g orderby Version.Parse(p.Version) descending select p).First()
                          orderby downloadCount descending
                          select new {
                              Id = latest.Id,
                              DownloadCount = downloadCount,
                              Url = FixGalleryUrl(latest.GalleryDetailsUrl)
                          }).Take(5)
        };
    }

    private static string FixGalleryUrl(Uri url) {
        // The official feed has a bug where the url contains a reference to localhost.
        // This is a temporary workaround.
        return url.OriginalString.Replace("http://localhost:777/", "http://nuget.org/");

        
    }
}