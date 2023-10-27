using System;
using System.Collections.Generic;
using System.Linq;

namespace UserProfileService.Common.Tests.Utilities.TestModels
{
    public class Employee
    {
        public DateTime? BirthDate { get; set; }
        public string EMail { get; set; }
        public string FirstName { get; set; }
        public string Gender { get; set; }
        public int Id { get; set; }
        public bool IsManager { get; set; }
        public string LastIp { get; set; }
        public string LastName { get; set; }
        public IDictionary<string, string> LastUsedAppInfo { get; set; }
        public bool? LikesJob { get; set; }
        public Dictionary<string, string> PhotoMetadata { get; set; }
        public IList<Cars> PreferredCars { get; set; }
        public double Priority { get; set; }
        public double? Quality { get; set; }
        public IList<string> Skills { get; set; }
        public DateTime StartedAt { get; set; }
        public string[] Wallets { get; set; }
        public DateTime? WeddingDate { get; set; }

        public Employee()
        {
        }

        public Employee(Employee source)
        {
            Id = source.Id;
            FirstName = source.FirstName;
            LastName = source.LastName;
            EMail = source.EMail;
            Gender = source.Gender;
            LastIp = source.LastIp;
            IsManager = source.IsManager;
            StartedAt = source.StartedAt;
            BirthDate = source.BirthDate;
            Quality = source.Quality;
            Priority = source.Priority;
            LikesJob = source.LikesJob;
            WeddingDate = source.WeddingDate;
            Skills = source.Skills.ToList();
            Wallets = source.Wallets.ToArray();

            PreferredCars = source.PreferredCars
                .Select(c => new Cars(c))
                .ToList();

            LastUsedAppInfo = new Dictionary<string, string>(source.LastUsedAppInfo);
            PhotoMetadata = new Dictionary<string, string>(source.PhotoMetadata);
        }

        public class Cars
        {
            public string Make { get; set; }
            public string Model { get; set; }
            public short Year { get; set; }

            public Cars()
            {
            }

            public Cars(Cars source)
            {
                Make = source.Make;
                Model = source.Model;
                Year = source.Year;
            }
        }
    }
}
