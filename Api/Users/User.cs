using GpsUtil.Location;
using System.Collections.Concurrent;
using TripPricer;

namespace TourGuide.Users;

public class User
{
    public Guid UserId { get; }
    public string UserName { get; }
    public string PhoneNumber { get; set; }
    public string EmailAddress { get; set; }
    public DateTime LatestLocationTimestamp { get; set; }
    public ConcurrentBag<UserReward> UserRewards { get; set; } = new ConcurrentBag<UserReward>();
    public ConcurrentBag<VisitedLocation> VisitedLocations { get; set; } = new ConcurrentBag<VisitedLocation>();
    public UserPreferences UserPreferences { get; set; } = new UserPreferences();
    public List<Provider> TripDeals { get; set; } = new List<Provider>();


    public User(Guid userId, string userName, string phoneNumber, string emailAddress)
    {
        UserId = userId;
        UserName = userName;
        PhoneNumber = phoneNumber;
        EmailAddress = emailAddress;
    }

    public void AddToVisitedLocations(VisitedLocation visitedLocation)
    {
        VisitedLocations.Add(visitedLocation);
    }

    public void ClearVisitedLocations()
    {
        VisitedLocations.Clear();
    }

    public void AddUserReward(UserReward userReward)
    {
        //if (!UserRewards.Exists(r => r.Attraction.AttractionName == userReward.Attraction.AttractionName))
        //{
        //    UserRewards.Add(userReward);
        //}
        if (!UserRewards.Any(r => r.Attraction.AttractionName == userReward.Attraction.AttractionName))
        {
            UserRewards.Add(userReward);
        }
    }

    public VisitedLocation GetLastVisitedLocation()
    {
        //return VisitedLocations[^1];
        return VisitedLocations.OrderByDescending(v => v.TimeVisited).FirstOrDefault()!;
    }
}
