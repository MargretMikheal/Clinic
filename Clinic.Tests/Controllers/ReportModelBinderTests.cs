using Clinic.Web.Controllers;
using Clinic.Web.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using Telerik.Reporting;
using Telerik.Reporting.Processing;

namespace Clinic.Tests.Controllers
{
    [TestClass]
    public class ReportModelBinderTests
    {
        [TestMethod]
        public void TestNormalizeNumericParameters_WithSingleElementArray_UnwrapsAndParsesInt()
        {
            var parms = new Dictionary<string, object>
            {
                { "DoctorId", new string[] { "5" } },
                { "PatientId", new object[] { "12" } }
            };

            ReportController.NormalizeNumericParameters(parms);

            Assert.AreEqual(5, parms["DoctorId"]);
            Assert.AreEqual(12, parms["PatientId"]);
        }

        [TestMethod]
        public void TestNormalizeNumericParameters_WithStringInt_ParsesInt()
        {
            var parms = new Dictionary<string, object>
            {
                { "PrescriptionId", "42" },
                { "EmptyVal", "" }
            };

            ReportController.NormalizeNumericParameters(parms);

            Assert.AreEqual(42, parms["PrescriptionId"]);
            Assert.IsNull(parms["EmptyVal"]);
        }

        [TestMethod]
        public void TestExtractId_HandlesDirectInt_String_AndArray()
        {
            var modelWithArray = new ReportsParameters
            {
                Parms = new Dictionary<string, object>
                {
                    { "PatientId", new string[] { "7" } }
                }
            };
            Assert.AreEqual(7, ReportController.ExtractId(modelWithArray, "PatientId"));

            var modelWithString = new ReportsParameters
            {
                Parms = new Dictionary<string, object>
                {
                    { "PrescriptionId", "15" }
                }
            };
            Assert.AreEqual(15, ReportController.ExtractId(modelWithString, "PrescriptionId"));

            var modelCaseInsensitive = new ReportsParameters
            {
                Parms = new Dictionary<string, object>
                {
                    { "prescriptionid", 20 }
                }
            };
            Assert.AreEqual(20, ReportController.ExtractId(modelCaseInsensitive, "PrescriptionId"));
        }

        [TestMethod]
        public void TestDoctorAppointmentsReportProcessing()
        {
            var reportPath = Path.GetFullPath(@"..\..\..\Clinic.web\Reports\DoctorAppointmentsReport.trdx");
            Assert.IsTrue(File.Exists(reportPath), "Report file should exist at " + reportPath);

            var uriReportSource = new UriReportSource { Uri = reportPath };
            uriReportSource.Parameters.Add("DoctorId", 0);
            uriReportSource.Parameters.Add("FromDate", null);
            uriReportSource.Parameters.Add("ToDate", null);
            uriReportSource.Parameters.Add("Status", null);

            var reportProcessor = new ReportProcessor();
            var result = reportProcessor.RenderReport("PDF", uriReportSource, null);

            Assert.IsFalse(result.HasErrors, result.HasErrors && result.Errors.Length > 0 ? result.Errors[0].ToString() : "");
        }


        [TestMethod]
        public void TestPatientMedicalHistoryReportProcessing()
        {
            var reportPath = Path.GetFullPath(@"..\..\..\Clinic.web\Reports\PatientMedicalHistoryReport.trdx");
            Assert.IsTrue(File.Exists(reportPath), "Report file should exist at " + reportPath);

            var uriReportSource = new UriReportSource { Uri = reportPath };
            uriReportSource.Parameters.Add("PatientId", 1);

            var reportProcessor = new ReportProcessor();
            var result = reportProcessor.RenderReport("PDF", uriReportSource, null);

            Assert.IsFalse(result.HasErrors, result.HasErrors && result.Errors.Length > 0 ? result.Errors[0].ToString() : "");
        }

        [TestMethod]
        public void TestPrescriptionReportProcessing()
        {
            var reportPath = Path.GetFullPath(@"..\..\..\Clinic.web\Reports\PrescriptionReport.trdx");
            Assert.IsTrue(File.Exists(reportPath), "Report file should exist at " + reportPath);

            var uriReportSource = new UriReportSource { Uri = reportPath };
            uriReportSource.Parameters.Add("PrescriptionId", 1);

            var reportProcessor = new ReportProcessor();
            var result = reportProcessor.RenderReport("PDF", uriReportSource, null);

            Assert.IsFalse(result.HasErrors, result.HasErrors && result.Errors.Length > 0 ? result.Errors[0].ToString() : "");
        }

        [TestMethod]
        public void TestDoctorPerformanceReportProcessing()
        {
            var reportPath = Path.GetFullPath(@"..\..\..\Clinic.web\Reports\DoctorPerformanceReport.trdx");
            Assert.IsTrue(File.Exists(reportPath), "Report file should exist at " + reportPath);

            var uriReportSource = new UriReportSource { Uri = reportPath };
            uriReportSource.Parameters.Add("FromDate", null);
            uriReportSource.Parameters.Add("ToDate", null);

            var reportProcessor = new ReportProcessor();
            var result = reportProcessor.RenderReport("PDF", uriReportSource, null);

            Assert.IsFalse(result.HasErrors, result.HasErrors && result.Errors.Length > 0 ? result.Errors[0].ToString() : "");
        }
    }
}
