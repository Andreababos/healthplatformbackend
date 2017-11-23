using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace RS.NetDiet.Therapist.Api.Controllers
{
    [RoutePrefix("api/results")]
    public class ResultsController : BaseApiController
    {
        [Route("upload/{patientPk}")]
        [Authorize(Roles = "DevAdmin, Admin")]
        [HttpPost]
        public async Task<IHttpActionResult> UploadResult([FromUri] long patientPk)
        {
            var therapistId = _patientsRepository.GetTherapistIdOfPatient(patientPk);
            if (string.IsNullOrEmpty(therapistId))
            {
                return NotFound();
            }

            var provider = new MultipartFormDataStreamProvider(HttpContext.Current.Server.MapPath(Path.Combine("~/Results", therapistId)));

            await Request.Content.ReadAsMultipartAsync(provider);
            foreach (var file in provider.FileData)
            {
                try
                {
                    _resultsRepository.CreateResultEntry(patientPk, file.Headers.ContentDisposition.FileName.Trim('\"'), Path.GetFileName(file.LocalFileName));
                }
                catch (Exception ex)
                {
                    _logger.Error("Error uploading file", ex);
                }
            }

            return Ok();
        }

        [Route("download/{filePk}")]
        //[Authorize(Roles = "DevAdmin, Admin, Therapist")]
        [AllowAnonymous]
        [HttpGet]
        public HttpResponseMessage DownloadResult(long filePk)
        {
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            var fileInfo = _resultsRepository.GetFileInfo(filePk);
            if (fileInfo == null)
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            var path = HttpContext.Current.Server.MapPath(Path.Combine("~/Results", fileInfo.TherapistId, fileInfo.GeneratedFileName));
            var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            result.Content = new StreamContent(stream);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = fileInfo.OriginalFileName
            };

            return result;
        }
    }
}