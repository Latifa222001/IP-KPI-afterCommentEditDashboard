using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace IP_KPI.Models
{
    [Table("StudentSurvey")]
    public partial class StudentSurvey
    {
        [Key]
        public int RecordNumber { get; set; }
        [StringLength(50)]
        public string Term { get; set; }
        public int? NumberOfStudent { get; set; }
        [StringLength(50)]
        public string Year { get; set; }
        [StringLength(50)]
        public string StudentCase { get; set; }
        [StringLength(50)]
        public string Nationality { get; set; }
        [StringLength(50)]
        public string Gender { get; set; }
        public int? NumOfRespondent { get; set; }
        public double? SurveyScore { get; set; }
        [Column("KPI_ID")]
        public int? KpiId { get; set; }
        public int? ProgramId { get; set; }

        [ForeignKey(nameof(KpiId))]
        [InverseProperty("StudentSurveys")]
        public virtual Kpi Kpi { get; set; }
        [ForeignKey(nameof(ProgramId))]
        [InverseProperty(nameof(UniProgram.StudentSurveys))]
        public virtual UniProgram Program { get; set; }
    }
}
