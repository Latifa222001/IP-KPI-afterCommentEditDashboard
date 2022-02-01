using IP_KPI.Data;
using IP_KPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;


namespace IP_KPI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly db_a7baa5_ipkpiContext _db;
        public HomeController(ILogger<HomeController> logger, db_a7baa5_ipkpiContext db)
        {
            _logger = logger;
            _db = db;
        }


        public IActionResult Index()
        {
            ViewBag.Colleges = _db.Colleges.ToList();
            ViewBag.KPI = _db.Kpis.ToList();
            return View();
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        //Filter Code Start

        [HttpGet]
        public ActionResult GetDepartments(int collegeId)
        {
            try
            {
                var Departments = _db.Departments.Where(x => x.CollegeId == collegeId).Select(u => new { u.DepartmentName, u.DepartmentId }).ToList();
                return Json(Departments);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpGet]
        public ActionResult GetPrograms(int departmentId)
        {
            try
            {
                var Programs = _db.UniPrograms.Where(x => x.DepartmentId == departmentId).Select(u => new { u.ProgramName, u.ProgramId, u.Level }).ToList();
                return Json(Programs);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        // Filter code end










        // KPI-P-2,3 charts code start
        [HttpGet]
        public ActionResult GetBarGenderChartDictionary(String programId, String KPICode, String gender, String term, String year, String studentCase, String nationality)
        {

            float fScore = 0, mScore = 0, fCount = 0, mCount = 0;
            String fColor = "#D0DA32", mColor = "#1F3771", bColor = "#B0AEB3";
            List <StudentSurvey> data = new List<StudentSurvey>();
            var kpi = _db.Kpis.FirstOrDefault(x => x.Kpicode == KPICode);

            var program = _db.UniPrograms.FirstOrDefault(x => x.ProgramId == Convert.ToInt32(programId));
            String progName = program.Level + " " + program.ProgramName;

            var studentSurvey = _db.StudentSurveys.Where(x => x.KpiId == kpi.KpiId && x.ProgramId == Convert.ToInt32(programId) && x.Year == year).ToList();

            if (term != "0")
            {
                if (nationality != "0")
                {
                    if (studentCase != "0")
                        //if Term-> not both, country-> not both, Case-> not both
                        studentSurvey = studentSurvey.Where(x => x.StudentCase == studentCase && x.Nationality == nationality && x.Term == term).ToList();

                    else
                        //if Term-> not both, country-> not both, Case-> both
                        studentSurvey = studentSurvey.Where(x => x.Nationality == nationality && x.Term == term).ToList();
                }
                else
                {
                    if (studentCase != "0")
                        //if Term-> not both, country-> both, Case-> not both
                        studentSurvey = studentSurvey.Where(x => x.StudentCase == studentCase && x.Term == term).ToList();

                    else
                        //if Term-> not both, country-> both, Case-> both
                        studentSurvey = studentSurvey.Where(x => x.Term == term).ToList();
                }
            }
            else
            {
                if (nationality != "0")
                {
                    if (studentCase != "0")
                        //if Term-> both, country-> not both, Case-> not both
                        studentSurvey = studentSurvey.Where(x => x.StudentCase == studentCase && x.Nationality == nationality).ToList();

                    else
                        //if Term-> both, country-> not both, Case-> both
                        studentSurvey = studentSurvey.Where(x => x.Nationality == nationality).ToList();
                }
                else
                {
                    if (studentCase != "0")
                        //if Term-> both, country-> both, Case-> not both
                        studentSurvey = studentSurvey.Where(x => x.StudentCase == studentCase).ToList();

                    //if Term-> both, country-> both, Case-> both
                }
            }


            foreach (var item in studentSurvey)
            {
                if (item.Gender.Equals("انثى"))
                {
                    fScore += (float)item.SurveyScore;
                    fCount += (float)item.NumOfRespondent;
                }
                else if (item.Gender.Equals("ذكر"))
                {
                    mScore += (float)item.SurveyScore;
                    mCount += (float)item.NumOfRespondent;
                }
            }

            if (gender != "0")
            {
                if (gender.Equals("انثى"))
                {
                    fColor = "#D0DA32";//dark yellow
                    mColor = "#B0AEB3";//light grey
                    bColor = "#B0AEB3";//light grey
                }
                else
                {
                    mColor = "#1F3771";//dark blue
                    fColor = "#B0AEB3";//light grey
                    bColor = "#B0AEB3";//light grey
                }
            }

            data.Add(new StudentSurvey
            {
                Gender = "انثى",
                Nationality = fColor,
                Term = "" + (fScore / fCount),
                StudentCase = progName,
                ProgramId = (int) fCount
            });

            data.Add(new StudentSurvey
            {
                Gender = "ذكر",
                Nationality = mColor,
                Term = "" + (mScore / mCount),
                StudentCase = progName,
                ProgramId = (int)mCount
            });

            data.Add(new StudentSurvey
            {
                Gender = "الكل",
                Nationality = bColor,
                Term = "" + ((fScore + mScore) / (fCount + mCount)),
                StudentCase = progName,
                ProgramId = (int)(fCount + mCount)
            });

            return Json(data);

        }

        [HttpGet]
        public ActionResult GetBarNationalityChartDictionary(String programId, String KPICode, String gender, String term, String year, String studentCase, String nationality)
        {

            float inScore = 0, outScore = 0, inCount = 0, outCount = 0;
            String inColor = "#D0DA32", outColor = "#1F3771";
            List<StudentSurvey> data = new List<StudentSurvey>();
            var kpi = _db.Kpis.FirstOrDefault(x => x.Kpicode == KPICode);
            var program = _db.UniPrograms.FirstOrDefault(x => x.ProgramId == Convert.ToInt32(programId));
            String progName = program.Level + " " + program.ProgramName;
            var studentSurvey = _db.StudentSurveys.Where(x => x.KpiId == kpi.KpiId && x.ProgramId == Convert.ToInt32(programId) && x.Year == year).ToList();

            if (term != "0")
            {
                if (gender != "0")
                {
                    if (studentCase != "0")
                        //if Term-> not both, gender-> not both, Case-> not both
                        studentSurvey = studentSurvey.Where(x => x.StudentCase == studentCase && x.Gender == gender && x.Term == term).ToList();

                    else
                        //if Term-> not both, gender-> not both, Case-> both
                        studentSurvey = studentSurvey.Where(x => x.Gender == gender && x.Term == term).ToList();
                }
                else
                {
                    if (studentCase != "0")
                        //if Term-> not both, gender-> both, Case-> not both
                        studentSurvey = studentSurvey.Where(x => x.StudentCase == studentCase && x.Term == term).ToList();

                    else
                        //if Term-> not both, gender-> both, Case-> both
                        studentSurvey = studentSurvey.Where(x => x.Term == term).ToList();
                }
            }
            else
            {
                if (gender != "0")
                {
                    if (studentCase != "0")
                        //if Term-> both, gender-> not both, Case-> not both
                        studentSurvey = studentSurvey.Where(x => x.StudentCase == studentCase && x.Gender == gender).ToList();

                    else
                        //if Term-> both, gender-> not both, Case-> both
                        studentSurvey = studentSurvey.Where(x => x.Gender == gender).ToList();
                }
                else
                {
                    if (studentCase != "0")
                        //if Term-> both, gender-> both, Case-> not both
                        studentSurvey = studentSurvey.Where(x => x.StudentCase == studentCase).ToList();

                    //if Term-> both, gender-> both, Case-> both
                }
            }

            foreach (var item in studentSurvey)
            {
                if (item.Nationality.Equals("محلي"))
                {
                    inScore += (float)item.SurveyScore;
                    inCount += (float)item.NumOfRespondent;
                }
                else if (item.Nationality.Equals("دولي"))
                {
                    outScore += (float)item.SurveyScore;
                    outCount += (float)item.NumOfRespondent;
                }
            }

            if (nationality != "0")
            {
                if (nationality.Equals("سعودي"))
                {
                    inColor = "#D0DA32";//dark yellow
                    outColor = "#B0AEB3";//light grey
                }
                else
                {
                    outColor = "#1F3771";//dark blue
                    inColor = "#B0AEB3";//light grey
                }
            }


            data.Add(new StudentSurvey
            {
                Gender = "سعودي",
                Nationality = inColor,
                Term = "" + (inScore / inCount),
                StudentCase = progName,
                ProgramId = (int)inCount
            });

            data.Add(new StudentSurvey
            {
                Gender = "غير سعودي",
                Nationality = outColor,
                Term = "" + (outScore / outCount),
                StudentCase = progName,
                ProgramId = (int)outCount
            });

            return Json(data);

        }

        [HttpGet]
        public ActionResult GetBarStudentCaseChartDictionary(String programId, String KPICode, String gender, String term, String year, String studentCase, String nationality)
        {

            float normalScore = 0, specialScore = 0, normalCount = 0, specialCount = 0;
            String normalColor = "#D0DA32", specialColor = "#1F3771";
            List<StudentSurvey> data = new List<StudentSurvey>();
            var kpi = _db.Kpis.FirstOrDefault(x => x.Kpicode == KPICode);
            var program = _db.UniPrograms.FirstOrDefault(x => x.ProgramId == Convert.ToInt32(programId));
            String progName = program.Level + " " + program.ProgramName;
            var studentSurvey = _db.StudentSurveys.Where(x => x.KpiId == kpi.KpiId && x.ProgramId == Convert.ToInt32(programId) && x.Year == year).ToList();

            if (term != "0")
            {
                if (gender != "0")
                {
                    if (nationality != "0")
                        //if Term-> not both, gender-> not both, nationality-> not both
                        studentSurvey = studentSurvey.Where(x => x.Nationality == nationality && x.Gender == gender && x.Term == term).ToList();

                    else
                        //if Term-> not both, gender-> not both, nationality-> both
                        studentSurvey = studentSurvey.Where(x => x.Gender == gender && x.Term == term).ToList();
                }
                else
                {
                    if (nationality != "0")
                        //if Term-> not both, gender-> both, nationality-> not both
                        studentSurvey = studentSurvey.Where(x => x.Nationality == nationality && x.Term == term).ToList();
                    else
                    //if Term-> not both, gender-> both, nationality-> both
                    studentSurvey = studentSurvey.Where(x =>  x.Term == term).ToList();
                }
            }
            else
            {
                if (gender != "0")
                {
                    if (nationality != "0")
                        //if Term-> both, gender-> not both, nationality-> not both
                        studentSurvey = studentSurvey.Where(x => x.Nationality == nationality && x.Gender == gender).ToList();

                    else
                        //if Term-> both, gender-> not both, nationality-> both
                        studentSurvey = studentSurvey.Where(x => x.Gender == gender).ToList();
                }
                else
                {
                    if (nationality != "0")
                        //if Term-> both, gender-> both, nationality-> not both
                        studentSurvey = studentSurvey.Where(x => x.Nationality == nationality).ToList();

                    //if Term-> both, gender-> both, nationality-> both
                }
            }


            foreach (var item in studentSurvey)
            {
                if (item.StudentCase.Equals("طبيعي"))
                {
                    normalScore += (float)item.SurveyScore;
                    normalCount += (float)item.NumOfRespondent;
                }
                else if (item.StudentCase.Equals("احتياجات خاصة"))
                {
                    specialScore += (float)item.SurveyScore;
                    specialCount += (float)item.NumOfRespondent;
                }
            }

            if (studentCase != "0")
            {
                if (studentCase.Equals("طبيعي"))
                {
                    normalColor = "#D0DA32";//dark yellow
                    specialColor = "#B0AEB3";//light grey
                }
                else
                {
                    specialColor = "#1F3771";//dark blue
                    normalColor = "#B0AEB3";//light grey
                }
            }


            data.Add(new StudentSurvey
            {
                Gender = "طبيعي",
                Nationality = normalColor,
                Term = "" + (normalScore / normalCount),
                StudentCase = progName,
                ProgramId = (int)normalCount
            });

            data.Add(new StudentSurvey
            {
                Gender = "احتياجات خاصة",
                Nationality = specialColor,
                Term = "" + (specialScore / specialCount),
                StudentCase = progName,
                ProgramId = (int)specialCount
            });

            return Json(data);

        }

        [HttpGet]
        public ActionResult GetBarTermChartDictionary(String programId, String KPICode, String gender, String term, String year, String studentCase, String nationality)
        {

            float firstTermScore = 0, secondTermScore = 0, thirdTermScore = 0, summerTermScore = 0, firstTermCount = 0, secondTermCount = 0, thirdTermCount = 0, summerTermCount = 0;
            String firstTermColor = "#D0DA32", secondTermColor = "#71BA44", thirdTermColor = "#29B8BE", summerTermColor = "#1F3771", bothTermColor = "#B0AEB3";//dark green
            List<StudentSurvey> data = new List<StudentSurvey>();
            var kpi = _db.Kpis.FirstOrDefault(x => x.Kpicode == KPICode);
            var program = _db.UniPrograms.FirstOrDefault(x => x.ProgramId == Convert.ToInt32(programId));
            String progName = program.Level + " " + program.ProgramName;
            var studentSurvey = _db.StudentSurveys.Where(x => x.KpiId == kpi.KpiId && x.ProgramId == Convert.ToInt32(programId) && x.Year == year).ToList();

            if (nationality != "0")
            {
                if (gender != "0")
                {
                    if (studentCase != "0")
                        //if nationality-> not both, gender-> not both, Case-> not both
                        studentSurvey = studentSurvey.Where(x => x.StudentCase == studentCase && x.Gender == gender && x.Nationality == nationality).ToList();

                    else
                        //if nationality-> not both, gender-> not both, Case-> both
                        studentSurvey = studentSurvey.Where(x => x.Gender == gender && x.Nationality == nationality).ToList();
                }
                else
                {
                    if (studentCase != "0")
                        //if nationality-> not both, gender-> both, Case-> not both
                        studentSurvey = studentSurvey.Where(x => x.StudentCase == studentCase && x.Nationality == nationality).ToList();

                    else
                        //if nationality-> not both, gender-> both, Case-> both
                        studentSurvey = studentSurvey.Where(x => x.Nationality == nationality).ToList();
                }
            }
            else
            {
                if (gender != "0")
                {
                    if (studentCase != "0")
                        //if nationality-> both, gender-> not both, Case-> not both
                        studentSurvey = studentSurvey.Where(x => x.StudentCase == studentCase && x.Gender == gender).ToList();

                    else
                        //if nationality-> both, gender-> not both, Case-> both
                        studentSurvey = studentSurvey.Where(x => x.Gender == gender).ToList();
                }
                else
                {
                    if (studentCase != "0")
                        //if nationality-> both, gender-> both, Case-> not both
                        studentSurvey = studentSurvey.Where(x => x.StudentCase == studentCase).ToList();

                    //if nationality-> both, gender-> both, Case-> both
                }
            }


            foreach (var item in studentSurvey)
            {
                if (item.Term.Equals("الفصل الدراسي الاول"))
                {
                    firstTermScore += (float)item.SurveyScore;
                    firstTermCount += (float)item.NumOfRespondent;
                }
                else if (item.Term.Equals("الفصل الدراسي الثاني"))
                {
                    secondTermScore += (float)item.SurveyScore;
                    secondTermCount += (float)item.NumOfRespondent;
                }
                else if (item.Term.Equals("الفصل الدراسي الثالث"))
                {
                    thirdTermScore += (float)item.SurveyScore;
                    thirdTermCount += (float)item.NumOfRespondent;
                }
                else if (item.Term.Equals("الفصل الدراسي الصيفي"))
                {
                    summerTermScore += (float)item.SurveyScore;
                    summerTermCount += (float)item.NumOfRespondent;
                }
            }

            if (term != "0")
            {
                if (term.Equals("الفصل الدراسي الاول"))
                {
                    firstTermColor = "#D0DA32";//dark green
                    secondTermColor = "#B0AEB3";//light green
                    thirdTermColor = "#B0AEB3";//light green
                    summerTermColor = "#B0AEB3";//light green
                    bothTermColor = "#B0AEB3";//light green
                }
                else if (term.Equals("الفصل الدراسي الثاني"))
                {
                    secondTermColor = "#71BA44";//dark green
                    firstTermColor = "#B0AEB3";//light green
                    thirdTermColor = "#B0AEB3";//light green
                    summerTermColor = "#B0AEB3";//light green
                    bothTermColor = "#B0AEB3";//light green
                }
                else if (term.Equals("الفصل الدراسي الثالث"))
                {
                    thirdTermColor = "#29B8BE";//dark green
                    firstTermColor = "#B0AEB3";//light green
                    secondTermColor = "#B0AEB3";//light green
                    summerTermColor = "#B0AEB3";//light green
                    bothTermColor = "#B0AEB3";//light green
                }
                else
                {
                    summerTermColor = "#1F3771";//dark green
                    firstTermColor = "#B0AEB3";//light green
                    secondTermColor = "#B0AEB3";//light green
                    thirdTermColor = "#B0AEB3";//light green
                    bothTermColor = "#B0AEB3";//light green
                }
            }


            data.Add(new StudentSurvey
            {
                Gender = "الفصل الدراسي الاول",
                Nationality = firstTermColor,
                Term = "" + (firstTermScore / firstTermCount),
                StudentCase = progName,
                ProgramId = (int)firstTermCount
            });

            data.Add(new StudentSurvey
            {
                Gender = "الفصل الدراسي الثاني",
                Nationality = secondTermColor,
                Term = "" + (secondTermScore / secondTermCount),
                StudentCase = progName,
                ProgramId = (int)secondTermCount
            });

            data.Add(new StudentSurvey
            {
                Gender = "الفصل الدراسي الثالث",
                Nationality = thirdTermColor,
                Term = "" + (thirdTermScore / thirdTermCount),
                StudentCase = progName,
                ProgramId = (int)thirdTermCount
            });

            data.Add(new StudentSurvey
            {
                Gender = "الفصل الدراسي الصيفي",
                Nationality = summerTermColor,
                Term = "" + (summerTermScore / summerTermCount),
                StudentCase = progName,
                ProgramId = (int)summerTermCount
            });

            data.Add(new StudentSurvey
            {
                Gender = "الكل",
                Nationality = bothTermColor,
                Term = "" + ((firstTermScore + secondTermScore + thirdTermScore + summerTermScore) / (firstTermCount + secondTermCount + thirdTermCount + summerTermCount)),
                StudentCase = progName,
                ProgramId = (int)(firstTermCount + secondTermCount + thirdTermCount + summerTermCount)
            });

            return Json(data);

        }

        [HttpGet]
        public ActionResult GetSurveyTargetChartDictionary(String programId, String KPICode, String gender, String term, String year, String studentCase, String nationality)
        {

            float score = 0, count = 0, internalBenchmark = 0, externalBenchmark = 0, targetBenchmark =0;
            String KPIScoreColor;
            List<StudentSurvey> data = new List<StudentSurvey>();
            var kpi = _db.Kpis.FirstOrDefault(x => x.Kpicode == KPICode);
            var studentSurvey = _db.StudentSurveys.Where(x => x.KpiId == kpi.KpiId && x.ProgramId == Convert.ToInt32(programId) && x.Year == year).ToList();
            var KPIResult = _db.Kpiprograms.Where(x => x.KpiId == kpi.KpiId && x.ProgramId == Convert.ToInt32(programId) && x.Year == year).ToList();

            var program = _db.UniPrograms.FirstOrDefault(x => x.ProgramId == Convert.ToInt32(programId));
            String progName = program.Level + " " + program.ProgramName;

            if (term != "0")
            {
                if (nationality != "0")
                {
                    if (gender != "0")
                    {
                        if (studentCase != "0")
                            //if nationality-> not both, gender-> not both, Case-> not both, term-> not both
                            studentSurvey = studentSurvey.Where(x => x.StudentCase == studentCase && x.Gender == gender && x.Nationality == nationality && x.Term == term).ToList();
                        else
                            //if nationality-> not both, gender-> not both, Case-> both, term-> not both
                            studentSurvey = studentSurvey.Where(x => x.Gender == gender && x.Nationality == nationality && x.Term == term).ToList();
                    }
                    else
                    {
                        if (studentCase != "0")
                            //if nationality-> not both, gender-> both, Case-> not both, term-> not both
                            studentSurvey = studentSurvey.Where(x => x.StudentCase == studentCase && x.Nationality == nationality && x.Term == term).ToList();
                        else
                            //if nationality-> not both, gender-> both, Case-> both, term-> not both
                            studentSurvey = studentSurvey.Where(x => x.Nationality == nationality && x.Term == term).ToList();
                    }
                }
                else
                {
                    if (gender != "0")
                    {
                        if (studentCase != "0")
                            //if nationality-> both, gender-> not both, Case-> not both, term-> not both
                            studentSurvey = studentSurvey.Where(x => x.StudentCase == studentCase && x.Gender == gender && x.Term == term).ToList();
                        else
                            //if nationality-> both, gender-> not both, Case-> both, term-> not both
                            studentSurvey = studentSurvey.Where(x => x.Gender == gender && x.Term == term).ToList();
                    }
                    else
                    {
                        if (studentCase != "0")
                            //if nationality-> both, gender-> both, Case-> not both, term-> not both
                            studentSurvey = studentSurvey.Where(x => x.StudentCase == studentCase && x.Term == term).ToList();
                        else
                        //if nationality-> both, gender-> both, Case-> both, term-> not both
                        studentSurvey = studentSurvey.Where(x => x.Term == term).ToList();
                    }
                }
                KPIResult = KPIResult.Where(x => x.Term == term).ToList();
            }
            else
            {
                if (nationality != "0")
                {
                    if (gender != "0")
                    {
                        if (studentCase != "0")
                            //if nationality-> not both, gender-> not both, Case-> not both, term-> both
                            studentSurvey = studentSurvey.Where(x => x.StudentCase == studentCase && x.Gender == gender && x.Nationality == nationality).ToList();
                        else
                            //if nationality-> not both, gender-> not both, Case-> both, term-> both
                            studentSurvey = studentSurvey.Where(x => x.Gender == gender && x.Nationality == nationality).ToList();
                    }
                    else
                    {
                        if (studentCase != "0")
                            //if nationality-> not both, gender-> both, Case-> not both, term-> both
                            studentSurvey = studentSurvey.Where(x => x.StudentCase == studentCase && x.Nationality == nationality).ToList();
                        else
                            //if nationality-> not both, gender-> both, Case-> both, term-> both
                            studentSurvey = studentSurvey.Where(x => x.Nationality == nationality).ToList();
                    }
                }
                else
                {
                    if (gender != "0")
                    {
                        if (studentCase != "0")
                            //if nationality-> both, gender-> not both, Case-> not both, term->  both
                            studentSurvey = studentSurvey.Where(x => x.StudentCase == studentCase && x.Gender == gender).ToList();
                        else
                            //if nationality-> both, gender-> not both, Case-> both, term->  both
                            studentSurvey = studentSurvey.Where(x => x.Gender == gender).ToList();
                    }
                    else
                    {
                        if (studentCase != "0")
                            //if nationality-> both, gender-> both, Case-> not both, term->  both
                            studentSurvey = studentSurvey.Where(x => x.StudentCase == studentCase).ToList();

                        //if nationality-> both, gender-> both, Case-> both, term->  both
                    }
                }
            }
            

            foreach (var item in studentSurvey)
            {
                score += (float)item.SurveyScore;
                count += (float)item.NumOfRespondent;
            }
            foreach (var item in KPIResult)
            {
                internalBenchmark += (float)item.InternalBenchmark;
                externalBenchmark += (float)item.ExternalBenchmark;
                targetBenchmark += (float)item.ExternalBenchmark;
            }

            if (term == "0")
            {
                internalBenchmark= internalBenchmark / 5;
                externalBenchmark= externalBenchmark / 5;
                targetBenchmark= targetBenchmark / 5;
            }

            var KPIScore = (score / count);
            var tenPercentOfTarget = targetBenchmark * .10;

            if (KPIScore <= (targetBenchmark - tenPercentOfTarget))
                KPIScoreColor = "#C1272D";//red

            else if (KPIScore < targetBenchmark)
                KPIScoreColor = "#F7931E";//yellow

            else if (KPIScore >= (targetBenchmark + tenPercentOfTarget))
                KPIScoreColor = "#29B8BE";//blue

            else
                KPIScoreColor = "#71BA44";//green


            data.Add(new StudentSurvey
            {
                Gender = "القيمة الفعلية",
                Nationality = KPIScoreColor,
                Term = "" + KPIScore,
                StudentCase = progName
            });

            data.Add(new StudentSurvey
            {
                Gender = "المقارنة المرجعية الخارجية",
                Nationality = "#B3B3B3",
                Term = "" + externalBenchmark,
                StudentCase = progName
            });

            data.Add(new StudentSurvey
            {
                Gender = "المقارنة المرجعية الداخلية",
                Nationality = "#CCCCCC",
                Term = "" + internalBenchmark,
                StudentCase = progName
            });

            data.Add(new StudentSurvey
            {
                Gender = "الاداء المستهدف",
                Nationality = "#E6E6E6",
                Term = "" + targetBenchmark,
                StudentCase = progName
            });

            return Json(data);

        }

        //KPI-P-2,3 charts code END

        //KPI-P-14-16 charts code start

        [HttpGet]
        public ActionResult GetPieNumOfFacultyChartDictionary(String programId, String gender, String year)
        {

            String fColor = "#D0DA32", mColor = "#1F3771";
            List<StudentSurvey> data = new List<StudentSurvey>();

            var NumOfFemaleFaculty = _db.FacultyPublicationReports.FirstOrDefault(x => x.Year == year && x.ProgramId == Convert.ToInt64(programId) && x.Gender == "انثى");
            var NumOfMaleFaculty = _db.FacultyPublicationReports.FirstOrDefault(x => x.Year == year && x.ProgramId == Convert.ToInt64(programId) && x.Gender == "ذكر");

            var program = _db.UniPrograms.FirstOrDefault(x => x.ProgramId == Convert.ToInt32(programId));
            String progName = program.Level + " " + program.ProgramName;

            if (gender != "0")
            {
                if (gender.Equals("انثى"))
                {
                    fColor = "#D0DA32";//dark yellow
                    mColor = "#B0AEB3";//light grey
                }
                else
                {
                    mColor = "#1F3771";//dark blue
                    fColor = "#B0AEB3";//light grey
                }
            }
            int total = (int) NumOfFemaleFaculty.NumOfFacultyOneP + (int) NumOfMaleFaculty.NumOfFacultyOneP;

            data.Add(new StudentSurvey
            {
                Gender = "انثى",
                Nationality = fColor,
                Term = "" + ((NumOfFemaleFaculty.NumOfFacultyOneP*100)/total),
                StudentCase = progName, //program Name
                ProgramId = (int)NumOfFemaleFaculty.NumOfFaculty

            });
            data.Add(new StudentSurvey
            {
                Gender = "`ذكر",
                Nationality = mColor,
                Term = "" + ((NumOfMaleFaculty.NumOfFacultyOneP *100)/total),
                StudentCase = progName, //program Name
                ProgramId = (int)NumOfMaleFaculty.NumOfFaculty

            });
            return Json(data);
        }

        [HttpGet]
        public ActionResult GetPiePublicationsChartDictionary(String programId, String gender, String year)
        {
            String fColor = "#D0DA32", mColor = "#1F3771";
            List<StudentSurvey> data = new List<StudentSurvey>();

            var publicationsNumF = _db.FacultyPublicationReports.FirstOrDefault(x => x.Year == year && x.ProgramId == Convert.ToInt64(programId) && x.Gender == "انثى");
            var publicationsNumM = _db.FacultyPublicationReports.FirstOrDefault(x => x.Year == year && x.ProgramId == Convert.ToInt64(programId) && x.Gender == "ذكر");

            var program = _db.UniPrograms.FirstOrDefault(x => x.ProgramId == Convert.ToInt32(programId));
            String progName = program.Level + " " + program.ProgramName;


            if (gender != "0")
            {
                if (gender.Equals("انثى"))
                {
                    fColor = "#D0DA32";//dark yellow
                    mColor = "#B0AEB3";//light grey
                }
                else
                {
                    mColor = "#1F3771";//dark blue
                    fColor = "#B0AEB3";//light grey
                }
            }

            int total = (int)publicationsNumF.NumOfPublications + (int)publicationsNumM.NumOfPublications;

            data.Add(new StudentSurvey
            {
                Gender = "انثى",
                Nationality = fColor,
                Term = "" + ((publicationsNumF.NumOfPublications*100)/total),
                StudentCase = progName, //program Name
                ProgramId = (int)publicationsNumF.NumOfFaculty

            });
            data.Add(new StudentSurvey
            {
                Gender = "`ذكر",
                Nationality = mColor,
                Term = "" + ((publicationsNumM.NumOfPublications*100)/total),
                StudentCase = progName, //program Name
                ProgramId = (int)publicationsNumM.NumOfFaculty

            });

            return Json(data);

        }



        [HttpGet]
        public ActionResult GetPublicationTargetChartDictionary(String programId, String KPICode, String gender, String term, String year)
        {
            float numOfFaculty = 0, NumOfPublication = 0, numOfFacultyWithOnePublication = 0, numOfCitation = 0, internalBenchmark = 0, externalBenchmark = 0, targetBenchmark = 0;
            String KPIScoreColor;
            List<StudentSurvey> data = new List<StudentSurvey>();
            var kpi = _db.Kpis.FirstOrDefault(x => x.Kpicode == KPICode);
            var faculty = _db.FacultyPublicationReports.Where(x => x.ProgramId == Convert.ToInt32(programId) && x.Year == year).ToList();
            var KPIResult = _db.Kpiprograms.Where(x => x.KpiId == kpi.KpiId && x.ProgramId == Convert.ToInt32(programId) && x.Year == year).ToList();

            var program = _db.UniPrograms.FirstOrDefault(x => x.ProgramId == Convert.ToInt32(programId));
            String progName = program.Level + " " + program.ProgramName;

            if (gender != "0")
            {
                //if gender-> not both
                faculty = faculty.Where(x => x.Gender == gender).ToList();
            }
            if (term != "0")
            {
                //if term-> not both
                KPIResult = KPIResult.Where(x => x.Term == term).ToList();
            }


            foreach (var item in faculty)
            {
                numOfFaculty += (float)item.NumOfFaculty;
                NumOfPublication += (float)item.NumOfPublications;
                numOfFacultyWithOnePublication += (float)item.NumOfFacultyOneP;
                numOfCitation += (float)item.NumOfCitations;
            }
            foreach (var item in KPIResult)
            {
                internalBenchmark += (float)item.InternalBenchmark;
                externalBenchmark += (float)item.ExternalBenchmark;
                targetBenchmark += (float)item.ExternalBenchmark;
            }

            if (term == "0")
            {
                internalBenchmark = internalBenchmark / 5;
                externalBenchmark = externalBenchmark / 5;
                targetBenchmark = targetBenchmark / 5;
            }

            var KPIScore = 0.0;


            if (KPICode.Equals("KPI-P-14"))
                KPIScore = (float)((numOfFacultyWithOnePublication / numOfFaculty)*5);

            else if (KPICode.Equals("KPI-P-15"))
                KPIScore = (float)((NumOfPublication / numOfFaculty) * 5);

            else if (KPICode.Equals("KPI-P-16"))
                KPIScore = (float)((numOfCitation / NumOfPublication) * 5);


            //coloring the KPI result based on the target 
            var tenPercentOfTarget =targetBenchmark * .10;

            if (KPIScore <= (targetBenchmark - tenPercentOfTarget))
                KPIScoreColor = "#C1272D";//red

            else if (KPIScore < targetBenchmark)
                KPIScoreColor = "#F7931E";//yellow

            else if (KPIScore >= (targetBenchmark + tenPercentOfTarget))
                KPIScoreColor = "#29B8BE";//blue

            else
                KPIScoreColor = "#71BA44";//green


            data.Add(new StudentSurvey
            {
                Gender = "القيمة الفعلية",
                Nationality = KPIScoreColor,
                Term = "" + KPIScore,
                StudentCase = progName
            });

            data.Add(new StudentSurvey
            {
                Gender = "المقارنة المرجعية الخارجية",
                Nationality = "#B3B3B3",
                Term = "" + externalBenchmark,
                StudentCase = progName
            });

            data.Add(new StudentSurvey
            {
                Gender = "المقارنة المرجعية الداخلية",
                Nationality = "#CCCCCC",
                Term = "" + internalBenchmark,
                StudentCase = progName
            });

            data.Add(new StudentSurvey
            {
                Gender = "الاداء المستهدف",
                Nationality = "#E6E6E6",
                Term = "" + targetBenchmark,
                StudentCase = progName
            });

            return Json(data);

        }


        // KPI-P-14-16 charts code end

    }

}
