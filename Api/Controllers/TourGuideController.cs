using GpsUtil.Location;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TourGuide.Services;
using TourGuide.Services.Interfaces;
using TourGuide.Users;
using TripPricer;

namespace TourGuide.Controllers;

[ApiController]
[Route("[controller]")]
public class TourGuideController : ControllerBase
{
    private readonly ITourGuideService _tourGuideService;
    private readonly IRewardsService _rewardsService;

    public TourGuideController(ITourGuideService tourGuideService, IRewardsService rewardsService)
    {
        _tourGuideService = tourGuideService;
        _rewardsService = rewardsService;
    }

    [HttpGet("getLocation")]
    public ActionResult<VisitedLocation> GetLocation([FromQuery] string userName)
    {
        var location = _tourGuideService.GetUserLocation(GetUser(userName));
        return Ok(location);
    }

    // TODO: Change this method to no longer return a List of Attractions.
    // Instead: Get the closest five tourist attractions to the user - no matter how far away they are.
    // Return a new JSON object that contains:
    // Name of Tourist attraction, 
    // Tourist attractions lat/long, 
    // The user's location lat/long, 
    // The distance in miles between the user's location and each of the attractions.
    // The reward points for visiting each Attraction.
    //    Note: Attraction reward points can be gathered from RewardsCentral
    [HttpGet("getNearbyAttractions")]
    public ActionResult GetNearbyAttractions([FromQuery] string userName)
    {
        var visitedLocation = _tourGuideService.GetUserLocation(GetUser(userName));
        var attractions = _tourGuideService.GetNearByAttractions(visitedLocation);
        _rewardsService.CalculateRewards(GetUser(userName));
        var result = attractions.Select(attraction => new
        {
            Name = attraction.AttractionName,
            AttractionLatitude = attraction.Latitude,
            AttractionLongitude = attraction.Longitude,
            UserLatitude = visitedLocation.Location.Latitude,
            UserLongitude = visitedLocation.Location.Longitude,
            DistanceInMiles = _rewardsService.GetDistance(attraction, visitedLocation.Location),
            RewardPoints = GetUser(userName).UserRewards

        });

        string json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        return Ok(json);
    }

    [HttpGet("getRewards")]
    public ActionResult<List<UserReward>> GetRewards([FromQuery] string userName)
    {
        var rewards = _tourGuideService.GetUserRewards(GetUser(userName));
        return Ok(rewards);
    }

    [HttpGet("getTripDeals")]
    public ActionResult<List<Provider>> GetTripDeals([FromQuery] string userName)
    {
        var deals = _tourGuideService.GetTripDeals(GetUser(userName));
        return Ok(deals);
    }

    private User GetUser(string userName)
    {
        return _tourGuideService.GetUser(userName);
    }

}
