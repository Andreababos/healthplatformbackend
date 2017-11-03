using RS.NetDiet.Therapist.DataModel;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace RS.NetDiet.Therapist.Api.Controllers
{
    [RoutePrefix("api/sqlexecutor")]
    public class DbController : BaseApiController
    {
        [Authorize(Roles = "DevAdmin")]
        [Route("nonquery")]
        [HttpPost]
        public IHttpActionResult ExecuteNonQuery(IEnumerable<string> commands)
        {
            NdLogger.Debug("Begin");
            if (commands.Any())
            {
                int rowsAffected = 0;
                using (var executor = new NdSqlExecutor())
                {
                    foreach (var command in commands)
                    {
                        NdLogger.Debug(string.Format("Executing command [{0}]", command));
                        var tmp = executor.NonQuery(command);
                        rowsAffected += tmp;
                        NdLogger.Debug(string.Format("Command executed successfully. Rows affected [{0}]", tmp));
                    }
                }

                NdLogger.Debug(string.Format("All commands executed successfully. Rows affected [{0}]", rowsAffected));
                return Ok(rowsAffected);
            }

            NdLogger.Warning("Command set is empty");
            return BadRequest();
        }

        [Authorize(Roles = "DevAdmin")]
        [Route("reader")]
        [HttpPost]
        public IHttpActionResult ExecuteReader([FromBody] string command)
        {
            NdLogger.Debug("Begin");
            List<List<Dictionary<string, object>>> data = new List<List<Dictionary<string, object>>>();
            using (var executor = new NdSqlExecutor())
            {
                NdLogger.Debug(string.Format("Executing reader with command [{0}]", command));
                data = executor.Reader(command);
                NdLogger.Debug("Reader executed successfully");
            }

            return Ok(data);
        }
    }
}