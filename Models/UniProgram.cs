using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace IP_KPI.Models
{
    [Table("UniProgram")]
    public partial class UniProgram
    {
        public UniProgram()
        {
            FacultyPublicationReports = new HashSet<FacultyPublicationReport>();
            Kpiprograms = new HashSet<Kpiprogram>();
            StudentSurveys = new HashSet<StudentSurvey>();
        }

        [Key]
        [Column("ProgramID")]
        public int ProgramId { get; set; }
        [StringLength(50)]
        public string ProgramName { get; set; }
        [Column("DepartmentID")]
        public int? DepartmentId { get; set; }
        [StringLength(50)]
        public string Level { get; set; }

        [ForeignKey(nameof(DepartmentId))]
        [InverseProperty("UniPrograms")]
        public virtual Department Department { get; set; }
        [InverseProperty(nameof(FacultyPublicationReport.Program))]
        public virtual ICollection<FacultyPublicationReport> FacultyPublicationReports { get; set; }
        [InverseProperty(nameof(Kpiprogram.Program))]
        public virtual ICollection<Kpiprogram> Kpiprograms { get; set; }
        [InverseProperty(nameof(StudentSurvey.Program))]
        public virtual ICollection<StudentSurvey> StudentSurveys { get; set; }
    }
}
