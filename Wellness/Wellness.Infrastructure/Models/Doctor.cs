using System;
using System.Collections.Generic;
using System.Text;

namespace Wellness.Infrastructure.Models
{
    public class Doctor
    {
        public long DoctorId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string IdDocument { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public List<Appointment> Appointments { get; set; }
    }
}
