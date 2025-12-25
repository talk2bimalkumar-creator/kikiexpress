using System;
using System.Collections.Generic;
using System.Linq;

namespace KikiCourierService
{
    // Offer definition
    public class Offer
    {
        public string Code { get; }
        public decimal DiscountPercent { get; }
        public int MinDistance { get; }
        public int MaxDistance { get; }
        public int MinWeight { get; }
        public int MaxWeight { get; }

        public Offer(string code, decimal discountPercent, int minDistance, int maxDistance, int minWeight, int maxWeight)
        {
            Code = code;
            DiscountPercent = discountPercent;
            MinDistance = minDistance;
            MaxDistance = maxDistance;
            MinWeight = minWeight;
            MaxWeight = maxWeight;
        }

        public bool IsApplicable(int distance, int weight)
        {
            return distance >= MinDistance && distance <= MaxDistance && weight >= MinWeight && weight <= MaxWeight;
        }
    }

    // Package definition
    public class Package
    {
        public string Id { get; }
        public int Weight { get; }
        public int Distance { get; }
        public string OfferCode { get; }
        public decimal Discount { get; set; }
        public decimal TotalCost { get; set; }
        public decimal DeliveryTime { get; set; }

        public Package(string id, int weight, int distance, string offerCode)
        {
            Id = id;
            Weight = weight;
            Distance = distance;
            OfferCode = offerCode;
        }
    }

    // Vehicle definition
    public class Vehicle
    {
        public int Id { get; }
        public decimal AvailableAt { get; set; }

        public Vehicle(int id)
        {
            Id = id;
            AvailableAt = 0;
        }
    }

    class Program
    {
        static List<Offer> Offers = new List<Offer>
        {
            new Offer("OFR001", 0.10m, 0, 200, 70, 200),
            new Offer("OFR002", 0.07m, 50, 150, 100, 250),
            new Offer("OFR003", 0.05m, 50, 250, 10, 150)
        };

        static void Main(string[] args)
        {
            // Read base_delivery_cost and no_of_packages
            var firstLine = Console.ReadLine()?.Trim().Split(' ');
            int baseDeliveryCost = int.Parse(firstLine[0]);
            int noOfPackages = int.Parse(firstLine[1]);

            var packages = new List<Package>();
            for (int i = 0; i < noOfPackages; i++)
            {
                var parts = Console.ReadLine()?.Trim().Split(' ');
                string id = parts[0];
                int weight = int.Parse(parts[1]);
                int distance = int.Parse(parts[2]);
                string offerCode = parts[3];
                packages.Add(new Package(id, weight, distance, offerCode));
            }

            // Try to read vehicle info (if present)
            string vehicleLine = Console.ReadLine();
            int noOfVehicles = 0, maxSpeed = 0, maxWeight = 0;
            bool deliveryTimeRequired = false;
            if (!string.IsNullOrWhiteSpace(vehicleLine))
            {
                var vehicleParts = vehicleLine.Trim().Split(' ');
                if (vehicleParts.Length == 3)
                {
                    noOfVehicles = int.Parse(vehicleParts[0]);
                    maxSpeed = int.Parse(vehicleParts[1]);
                    maxWeight = int.Parse(vehicleParts[2]);
                    deliveryTimeRequired = true;
                }
            }

            // Calculate cost and discount for each package
            foreach (var pkg in packages)
            {
                var offer = Offers.FirstOrDefault(o => o.Code == pkg.OfferCode && o.IsApplicable(pkg.Distance, pkg.Weight));
                decimal discount = 0;
                decimal totalCost = baseDeliveryCost + (pkg.Weight * 10) + (pkg.Distance * 5);
                if (offer != null)
                {
                    discount = Math.Floor(totalCost * offer.DiscountPercent);
                }
                pkg.Discount = discount;
                pkg.TotalCost = totalCost - discount;
            }

            if (!deliveryTimeRequired)
            {
                // Output for Problem 1
                foreach (var pkg in packages)
                {
                    Console.WriteLine($"{pkg.Id} {pkg.Discount:0} {pkg.TotalCost:0}");
                }
                return;
            }

            // Problem 2: Delivery time estimation
            // Sort packages by weight descending, then by distance descending
            var undelivered = packages.OrderByDescending(p => p.Weight).ThenByDescending(p => p.Distance).ToList();
            var vehicles = new List<Vehicle>();
            for (int i = 0; i < noOfVehicles; i++)
                vehicles.Add(new Vehicle(i));

            while (undelivered.Any())
            {
                // Find available vehicle
                var vehicle = vehicles.OrderBy(v => v.AvailableAt).First();

                // Find best shipment for this vehicle (maximize packages, prefer heavier)
                var shipment = FindBestShipment(undelivered, maxWeight);

                // Calculate max delivery time for this shipment
                decimal maxTime = 0;
                foreach (var pkg in shipment)
                {
                    decimal time = vehicle.AvailableAt + (decimal)pkg.Distance / maxSpeed;
                    pkg.DeliveryTime = time;
                    if (time > maxTime) maxTime = time;
                }

                // Update vehicle available time (go and return)
                int furthestDistance = shipment.Max(p => p.Distance);
                vehicle.AvailableAt = vehicle.AvailableAt + 2 * ((decimal)furthestDistance / maxSpeed);

                // Remove delivered packages
                foreach (var pkg in shipment)
                    undelivered.Remove(pkg);
            }

            // Output for Problem 2
            foreach (var pkg in packages)
            {
                Console.WriteLine($"{pkg.Id} {pkg.Discount:0} {pkg.TotalCost:0} {pkg.DeliveryTime:0.00}");
            }
        }

        // Greedy: maximize number of packages, then total weight
        static List<Package> FindBestShipment(List<Package> packages, int maxWeight)
        {
            // Try all combinations up to 10 packages (for performance)
            var best = new List<Package>();
            int n = packages.Count;
            int maxPackages = 0;
            int maxTotalWeight = 0;

            // Try all combinations (up to 2^n, but n is small)
            for (int mask = 1; mask < (1 << n); mask++)
            {
                var shipment = new List<Package>();
                int totalWeight = 0;
                for (int i = 0; i < n; i++)
                {
                    if ((mask & (1 << i)) != 0)
                    {
                        shipment.Add(packages[i]);
                        totalWeight += packages[i].Weight;
                    }
                }
                if (totalWeight <= maxWeight)
                {
                    if (shipment.Count > maxPackages ||
                        (shipment.Count == maxPackages && totalWeight > maxTotalWeight))
                    {
                        best = shipment;
                        maxPackages = shipment.Count;
                        maxTotalWeight = totalWeight;
                    }
                }
            }
            return best;
        }
    }
}