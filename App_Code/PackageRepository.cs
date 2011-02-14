using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Caching;
using NuGet;

public class PackageRepository {
    private static readonly object _lockObject = new object();
    private static readonly DataServicePackageRepository _repository = new DataServicePackageRepository(new Uri("http://packages.nuget.org/v1/FeedService.svc"));

    public static IList<IPackage> GetPackages(Cache cache) {
        // Try to load if from the cache
        var packages = (IList<IPackage>)cache.Get("packages");

        // Double check lock
        if (packages == null) {
            lock (_lockObject) {
                packages = (IList<IPackage>)cache.Get("packages");

                if (packages == null) {
                    // If we still don't have anything cached then get the package list and store it.
                    packages = _repository.GetPackages().ToList();

                    cache.Insert("packages", 
                                  packages, 
                                  null, 
                                  DateTime.Now + TimeSpan.FromSeconds(30), 
                                  Cache.NoSlidingExpiration);
                }
            }
        }

        return packages;
    }
}