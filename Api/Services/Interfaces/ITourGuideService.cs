﻿using GpsUtil.Location;
using System.Collections.Concurrent;
using TourGuide.Users;
using TourGuide.Utilities;
using TripPricer;

namespace TourGuide.Services.Interfaces
{
    public interface ITourGuideService
    {
        Tracker Tracker { get; }

        void AddUser(User user);
        List<User> GetAllUsers();
        List<Attraction> GetNearByAttractions(VisitedLocation visitedLocation);
        List<Provider> GetTripDeals(User user);
        User GetUser(string userName);
        VisitedLocation GetUserLocation(User user);
        ConcurrentBag<UserReward> GetUserRewards(User user);
        VisitedLocation TrackUserLocation(User user);
    }
}