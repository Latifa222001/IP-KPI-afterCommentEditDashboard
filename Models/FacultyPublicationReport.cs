using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace IP_KPI.Models
{
    [Table("FacultyPublicationReport")]
    public partial class FacultyPublicationReport
    {
        public FacultyPublicationReport()
        {
            Kpis = new HashSet<Kpi>();
        }

        [Key]
        public int FacultyReportId { get; set; }
        [StringLength(50)]
        public string Gender { get; set; }
        public int? NumOfFaculty { get; set; }
        [Column("NumOfFaculty_oneP")]
        public int? NumOfFacultyOneP { get; set; }
        public int? NumOfPublications { get; set; }
        public int? NumOfCitations { get; set; }
        [StringLength(50)]
        public string Year { get; set; }
        public int? ProgramId { get; set; }

        [ForeignKey(nameof(ProgramId))]
        [InverseProperty(nameof(UniProgram.FacultyPublicationReports))]
        public virtual UniProgram Program { get; set; }
        [InverseProperty(nameof(Kpi.Faculty))]
        public virtual ICollection<Kpi> Kpis { get; set; }
    }
}
