using System;
using System.Collections.Generic;
using System.Text;

namespace Wellness.Infrastructure.Models
{
    public class Appointment
    {
        public long AppointmentId { get; set; }
        public DateTime DateOfAppointment { get; set; }
        public long PatientId { get; set; }
        public Patient Patient { get; set; }
        public long DoctorId { get; set; }
        public Doctor Doctor { get; set; }
    }
}