namespace r2bw_alpha.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    public class Participant
    {

        public Participant()
        {
            this.Attendance = new HashSet<Attendance>();
        }

        [Required]
        public int Id { get; set; }

        [Required]
        [DisplayName("Participant")]
        public string Name { get; set; }

        [DisplayName("Waiver signed on")]
        [DataType(DataType.Date)]
        public DateTimeOffset? WaiverSignedOn { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string Sex { get; set; }

        public string Size { get; set; }

        [Required]
        public int GroupId { get; set; }

        public Group Group { get; set; }

        public ICollection<Attendance> Attendance { get; set; }
    }
}