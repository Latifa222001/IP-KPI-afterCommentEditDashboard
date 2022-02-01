using IP_KPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Data.OleDb;
using System.Linq;
using System.Threading.Tasks;
using IP_KPI.Data;
using ExcelDataReader;
using System.Data;
using Microsoft.Data.SqlClient;

namespace IP_KPI.Controllers
{
    public class UploadController : Controller
    {
        private readonly db_a7baa5_ipkpiContext _db;

        public UploadController(db_a7baa5_ipkpiContext db)
        {

            _db = db;
        }
        public ActionResult Index()
        {
            return View();
        }

        public IActionResult UploadExcel(String kpi, IFormFile file)
        {

            switch (kpi)
            {
                case "survey":
                    UploadSurvey(file);
                    break;
                case "publication":
                    UploadPublication(file);
                    break;


            }

            return View("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken()]
        public async Task UploadSurvey(IFormFile file)
        {
            var SSlist = new List<StudentSurvey>();
            var KPlist = new List<Kpiprogram>();

            using (var stream = new MemoryStream())
            {
                try
                {
                    await file.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                        var rowcount = worksheet.Dimension.Rows;

                        //Go through all the rows in the excel sheet and store them in the list
                        for (int row = 2; row <= rowcount; row++)
                        {

                            var ProName = worksheet.Cells[row, 3].Value.ToString().Trim();
                            var ProLevel = worksheet.Cells[row, 4].Value.ToString().Trim();
                            var proID = _db.UniPrograms.Where(x => x.ProgramName == ProName && x.Level == ProLevel).Select(x => x.ProgramId).SingleOrDefault();
                            var KPI_code = worksheet.Cells[row, 13].Value.ToString().Trim();
                            var kpi_id = _db.Kpis.Where(x => x.Kpicode == KPI_code).Select(x => x.KpiId).SingleOrDefault();



                            SSlist.Add(new StudentSurvey
                            {

                                Year = worksheet.Cells[row, 5].Value.ToString().Trim(),
                                Term = worksheet.Cells[row, 6].Value.ToString().Trim(),
                                Gender = worksheet.Cells[row, 7].Value.ToString().Trim(),
                                Nationality = worksheet.Cells[row, 8].Value.ToString().Trim(),
                                StudentCase = worksheet.Cells[row, 9].Value.ToString().Trim(),
                                NumberOfStudent = Convert.ToInt32(worksheet.Cells[row, 10].Value.ToString().Trim()),
                                NumOfRespondent = Convert.ToInt32(worksheet.Cells[row, 11].Value.ToString().Trim()),
                                SurveyScore = Convert.ToDouble(worksheet.Cells[row, 12].Value.ToString().Trim()),
                                KpiId = kpi_id,
                                ProgramId = proID
                            }); ;
                            KPlist.Add(new Kpiprogram
                            {
                                Year = worksheet.Cells[row, 5].Value.ToString().Trim(),
                                Term = worksheet.Cells[row, 6].Value.ToString().Trim(),
                                TargetBenchmark = Convert.ToDouble(worksheet.Cells[row, 14].Value.ToString().Trim()),
                                InternalBenchmark = Convert.ToDouble(worksheet.Cells[row, 15].Value.ToString().Trim()),
                                ExternalBenchmark = Convert.ToDouble(worksheet.Cells[row, 16].Value.ToString().Trim()),
                                NewTargetBenchmark = Convert.ToDouble(worksheet.Cells[row, 17].Value.ToString().Trim()),
                                KpiId = kpi_id,
                                ProgramId = proID
                            });

                        }

                    }
                    //check if a recored already exist in the database , if yes delete from the list 
                    foreach (var n in SSlist)
                    {

                        var compare = _db.StudentSurveys.Where(x => x.ProgramId == n.ProgramId && x.Year == n.Year && x.Term == n.Term && x.KpiId == n.KpiId && x.Gender == n.Gender && x.Nationality == n.Nationality && x.StudentCase == n.StudentCase).Any();
                        if (compare)
                        {

                            SSlist.Remove(n);
                        }

                    }
                    foreach (var n in KPlist)
                    {

                        var compare = _db.Kpiprograms.Where(x => x.ProgramId == n.ProgramId && x.Year == n.Year && x.Term == n.Term && x.KpiId == n.KpiId).Any();
                        if (compare)
                            KPlist.Remove(n);

                    }

                    //add data to the database
                    foreach (var n in SSlist)
                    {
                        _db.StudentSurveys.Add(n);
                    }

                    _db.SaveChanges();

                    foreach (var n in KPlist)
                    {
                        _db.Kpiprograms.Add(n);
                    }

                    _db.SaveChanges();
                    ViewBag.suc = "تم حفظ البيانات بنجاح";
                    RedirectToPage("Index");
                }
                catch (NullReferenceException e)
                {
                    ViewBag.err = "حدث خطأ اثناء استرجاع البيانات من ملف الاكسل، الرجاء التحقق من صحة التعبئة";
                    RedirectToPage("Index");
                }
                catch (SqlException e)
                {
                    ViewBag.err = " لم يتم حفظ البيانات في قاعدة البيانات الرجاء اعادة المحاولة";
                    RedirectToPage("Index");

                }
                catch (Exception e)
                {
                    ViewBag.err = " حدث خطأ ما، الرجاء التحقق واعادة التحميل";
                    RedirectToPage("Index");

                }





            }
        }



        [HttpPost]
        [ValidateAntiForgeryToken()]
        public async Task UploadPublication(IFormFile file)
        {
            var facultyList = new List<FacultyPublicationReport>();
            var KPIList = new List<Kpiprogram>();


            using (var stream = new MemoryStream())
            {
                try
                {
                    await file.CopyToAsync(stream);

                    using (var package = new ExcelPackage(stream))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                        var rowcount = worksheet.Dimension.Rows;
                        //add  faculty

                        for (int row = 2; row <= rowcount; row++)
                        {
                            var pname = worksheet.Cells[row, 3].Value.ToString().Trim();
                            var programlevel = worksheet.Cells[row, 6].Value.ToString().Trim();
                            var pn = _db.UniPrograms.Where(x => x.ProgramName == pname && x.Level == programlevel).Select(x => x.ProgramId).SingleOrDefault();
                            facultyList.Add(new FacultyPublicationReport
                            {
                                Gender = worksheet.Cells[row, 7].Value.ToString().Trim(),
                                NumOfFaculty = Convert.ToInt32(worksheet.Cells[row, 8].Value.ToString().Trim()),
                                NumOfFacultyOneP = Convert.ToInt32(worksheet.Cells[row, 11].Value.ToString().Trim()),
                                NumOfPublications = Convert.ToInt32(worksheet.Cells[row, 9].Value.ToString().Trim()),
                                NumOfCitations = Convert.ToInt32(worksheet.Cells[row, 10].Value.ToString().Trim()),
                                Year = worksheet.Cells[row, 4].Value.ToString().Trim(),
                                ProgramId = pn

                            });


                            //add KPI-P-14 in KPIprogram table 
                            KPIList.Add(new Kpiprogram
                            {

                                KpiId = 23,
                                ProgramId = pn,
                                TargetBenchmark = Convert.ToDouble(worksheet.Cells[row, 12].Value.ToString().Trim()),
                                NewTargetBenchmark = Convert.ToDouble(worksheet.Cells[row, 15].Value.ToString().Trim()),
                                InternalBenchmark = Convert.ToDouble(worksheet.Cells[row, 13].Value.ToString().Trim()),
                                ExternalBenchmark = Convert.ToDouble(worksheet.Cells[row, 14].Value.ToString().Trim()),
                                Year = worksheet.Cells[row, 4].Value.ToString().Trim(),
                                Term = worksheet.Cells[row, 5].Value.ToString().Trim()

                            });

                            //add KPI-P-15 in KPIprogram table 

                            KPIList.Add(new Kpiprogram
                            {
                                KpiId = 24,
                                ProgramId = pn,
                                TargetBenchmark = Convert.ToDouble(worksheet.Cells[row, 16].Value.ToString().Trim()),
                                NewTargetBenchmark = Convert.ToDouble(worksheet.Cells[row, 19].Value.ToString().Trim()),
                                InternalBenchmark = Convert.ToDouble(worksheet.Cells[row, 17].Value.ToString().Trim()),
                                ExternalBenchmark = Convert.ToDouble(worksheet.Cells[row, 18].Value.ToString().Trim()),
                                Year = worksheet.Cells[row, 4].Value.ToString().Trim(),
                                Term = worksheet.Cells[row, 5].Value.ToString().Trim()

                            });

                            //add KPI-P-16 in KPIprogram table 
                            KPIList.Add(new Kpiprogram
                            {
                                KpiId = 25,
                                ProgramId = pn,
                                TargetBenchmark = Convert.ToDouble(worksheet.Cells[row, 20].Value.ToString().Trim()),
                                NewTargetBenchmark = Convert.ToDouble(worksheet.Cells[row, 23].Value.ToString().Trim()),
                                InternalBenchmark = Convert.ToDouble(worksheet.Cells[row, 21].Value.ToString().Trim()),
                                ExternalBenchmark = Convert.ToDouble(worksheet.Cells[row, 22].Value.ToString().Trim()),
                                Year = worksheet.Cells[row, 4].Value.ToString().Trim(),
                                Term = worksheet.Cells[row, 5].Value.ToString().Trim()

                            });

                        }
                    }
                    //remove the duplication from faculty 
                    foreach (var f in facultyList)
                    {

                        var compare = _db.FacultyPublicationReports.Where(x => x.ProgramId == f.ProgramId && x.Year == f.Year && x.Gender == f.Gender).Any();
                        if (compare)
                            facultyList.Remove(f);

                    }

                    //remove the duplication from KPIProgram 
                    foreach (var n in KPIList)
                    {

                        var compare = _db.Kpiprograms.Where(x => x.ProgramId == n.ProgramId && x.Year == n.Year && x.Term == n.Term && x.KpiId == n.KpiId).Any();
                        if (compare)
                            KPIList.Remove(n);

                    }
                    foreach (var n in facultyList)
                    {
                        _db.FacultyPublicationReports.Add(n);
                    }




                    _db.SaveChanges();
                    foreach (var n in KPIList)
                    {
                        _db.Kpiprograms.Add(n);
                    }


                    _db.SaveChanges();
                    ViewBag.suc = "تم حفظ البيانات بنجاح";
                    RedirectToPage("Index");
                }
                catch (NullReferenceException e)
                {
                    ViewBag.err = "حدث خطأ اثناء استرجاع البيانات من ملف الاكسل، الرجاء التحقق من صحة التعبئة";
                    RedirectToPage("Index");
                }
                catch (SqlException e)
                {
                    ViewBag.err = " لم يتم حفظ البيانات في قاعدة البيانات الرجاء اعادة المحاولة";
                    RedirectToPage("Index");

                }
                catch (Exception e)
                {
                    ViewBag.err = " حدث خطأ ما، الرجاء التحقق واعادة التحميل";
                    RedirectToPage("Index");

                }



            }
        }

    }
}