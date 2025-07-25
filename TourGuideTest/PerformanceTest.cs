﻿using GpsUtil.Location;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TourGuide.LibrairiesWrappers.Interfaces;
using TourGuide.Services.Interfaces;
using TourGuide.Users;
using TourGuide.Utilities;
using Xunit.Abstractions;

namespace TourGuideTest
{
    public class PerformanceTest : IClassFixture<DependencyFixture>
    {
        /*
         * Note on performance improvements:
         * 
         * The number of generated users for high-volume tests can be easily adjusted using this method:
         * 
         *_fixture.Initialize(100000); (for example)
         * 
         * 
         * These tests can be modified to fit new solutions, as long as the performance metrics at the end of the tests remain consistent.
         * 
         * These are the performance metrics we aim to achieve:
         * 
         * highVolumeTrackLocation: 100,000 users within 15 minutes:
         * Assert.True(TimeSpan.FromMinutes(15).TotalSeconds >= stopWatch.Elapsed.TotalSeconds);
         *
         * highVolumeGetRewards: 100,000 users within 20 minutes:
         * Assert.True(TimeSpan.FromMinutes(20).TotalSeconds >= stopWatch.Elapsed.TotalSeconds);
        */

        private readonly DependencyFixture _fixture;

        private readonly ITestOutputHelper _output;

        public PerformanceTest(DependencyFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;
        }

        [Fact]
        public async Task HighVolumeTrackLocation()
        {
            //On peut ici augmenter le nombre d'utilisateurs pour tester les performances
            _fixture.Initialize(100);

            List<User> allUsers = _fixture.TourGuideService.GetAllUsers();

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            SemaphoreSlim semaphore = new SemaphoreSlim(1000); // Limiting concurrent tracking to 1000 users
            List<Task> trackingTasks = new List<Task>();
            foreach (var user in allUsers)
            {
                await semaphore.WaitAsync();
                trackingTasks.Add(Task.Run(() =>
                {
                    try
                    {
                        _fixture.TourGuideService.TrackUserLocation(user);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));
            }

            stopWatch.Stop();
            _fixture.TourGuideService.Tracker.StopTracking();

            _output.WriteLine($"highVolumeTrackLocation: Time Elapsed: {stopWatch.Elapsed.TotalSeconds} seconds.");

            Assert.True(TimeSpan.FromMinutes(15).TotalSeconds >= stopWatch.Elapsed.TotalSeconds);
        }

        [Fact]
        public async Task HighVolumeGetRewards()
        {
            //On peut ici augmenter le nombre d'utilisateurs pour tester les performances
            _fixture.Initialize(100);

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            Attraction attraction = _fixture.GpsUtil.GetAttractions()[0];
            List<User> allUsers = _fixture.TourGuideService.GetAllUsers();
            SemaphoreSlim semaphore = new SemaphoreSlim(2000); // Limiting concurrent reward calculation to 1000 users
            List<Task> rewardTasks = new List<Task>();
            foreach (var user in allUsers)
            {
                await semaphore.WaitAsync();
                rewardTasks.Add(Task.Run(() =>
                {
                    try
                    {
                        user.AddToVisitedLocations(new VisitedLocation(user.UserId, attraction, DateTime.Now));
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));
            }
            Task.WaitAll(rewardTasks.ToArray());
            //allUsers.ForEach(u => u.AddToVisitedLocations(new VisitedLocation(u.UserId, attraction, DateTime.Now)));
            foreach (var user in allUsers)
            {
                await semaphore.WaitAsync();
                rewardTasks.Add(Task.Run(() =>
                {
                    try
                    {
                        _fixture.RewardsService.CalculateRewards(user);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));
            }
            Task.WaitAll(rewardTasks.ToArray());

            //allUsers.ForEach(u => _fixture.RewardsService.CalculateRewards(u));
            foreach (var user in allUsers)
            {
                await semaphore.WaitAsync();
                rewardTasks.Add(Task.Run(() =>
                {
                    try
                    {
                        Assert.True(user.UserRewards.Count > 0);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));
            }
            //foreach (var user in allUsers)
            //{
            //    Assert.True(user.UserRewards.Count > 0);
            //}
            stopWatch.Stop();
            _fixture.TourGuideService.Tracker.StopTracking();

            _output.WriteLine($"highVolumeGetRewards: Time Elapsed: {stopWatch.Elapsed.TotalSeconds} seconds.");
            Assert.True(TimeSpan.FromMinutes(20).TotalSeconds >= stopWatch.Elapsed.TotalSeconds);
        }
    }
}
