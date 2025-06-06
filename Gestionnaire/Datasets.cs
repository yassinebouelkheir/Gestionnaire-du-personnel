using Microsoft.VisualBasic;

namespace Gestionnaire
{
    public class QueryResultRow
    {
        public Dictionary<string, string> Columns { get; } = [];
        public string this[string columnName] => Columns.TryGetValue(columnName, out var value) ? value : string.Empty;
    }

    public abstract class ContractorActivity
    {
        protected static List<QueryResultRow> FetchData(string query, Dictionary<string, object> parameters)
        {
            try
            {
                return Program.Controller.ReadData(query, parameters);
            }
            catch (Exception ex)
            {
                Methodes.PrintConsole(Config.sourceDataset, ex.ToString(), true);
                return [];
            }
        }
    }

    public class Absence : ContractorActivity
    {
        public bool IsAbsent { get; private set; }
        public List<QueryResultRow> ListAbsence { get; private set; }
        public string Reason { get; private set; } = "";
        public string JustificativeDocument { get; private set; } = "";
        public bool IsNull { get; private set; } = true;

        public Absence(int contractorId, long date = -1)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@contractorId", contractorId },
            };
            string query = "SELECT reason, justificativeDocument, date FROM Absences WHERE contractorId = @contractorId";
            if (date > 0)
            {
                var dateTime = DateTimeOffset.FromUnixTimeSeconds(date).UtcDateTime.Date;
                var startOfDay = new DateTimeOffset(dateTime).ToUnixTimeSeconds();
                var endOfDay = new DateTimeOffset(dateTime.AddDays(1)).ToUnixTimeSeconds() - 1;

                query += " AND date BETWEEN @startOfDay AND @endOfDay";
                parameters["@startOfDay"] = startOfDay;
                parameters["@endOfDay"] = endOfDay;
            }
            query += " ORDER BY date DESC";

            ListAbsence = FetchData(query, parameters);

            if (ListAbsence.Count > 0)
            {
                IsAbsent = true;
                Reason = ListAbsence[0]["reason"];
                JustificativeDocument = ListAbsence[0]["justificativeDocument"];
                IsNull = false;
            }
        }
    }

    public class PaidLeave : ContractorActivity
    {
        public bool IsInPaidLeave { get; private set; }
        public List<QueryResultRow> ListPaidLeave { get; private set; }
        public string StartDate { get; private set; } = "";
        public string EndDate { get; private set; } = "";
        public long UnixStartDate { get; private set; }
        public long UnixEndDate { get; private set; }
        public string Reason { get; private set; } = "";
        public bool IsNull { get; private set; } = true;

        public PaidLeave(int contractorId, long date = -1)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@contractorId", contractorId }
            };
            string query = "SELECT startDate, endDate, reason FROM PaidLeave WHERE contractorId = @contractorId";

            if (date > 0)
            {
                var dateTime = DateTimeOffset.FromUnixTimeSeconds(date).UtcDateTime.Date;
                var startOfDay = new DateTimeOffset(dateTime).ToUnixTimeSeconds();

                query += " AND endDate >= @startOfDay AND startDate <= @startOfDay";
                parameters["@startOfDay"] = startOfDay;
            }
            query += " ORDER BY endDate DESC";

            ListPaidLeave = FetchData(query, parameters);
            if (ListPaidLeave.Count > 0)
            {
                IsInPaidLeave = true;
                _ = long.TryParse(ListPaidLeave[0]["startDate"], out long sDate);
                _ = long.TryParse(ListPaidLeave[0]["endDate"], out long eDate);

                DateTime sdate = DateTimeOffset.FromUnixTimeSeconds(sDate).DateTime.Date;
                DateTime edate = DateTimeOffset.FromUnixTimeSeconds(eDate).DateTime.Date;
                StartDate = sdate.ToString("dd/MM/yyyy") ?? "";
                EndDate = edate.ToString("dd/MM/yyyy") ?? "";

                UnixStartDate = sDate;
                UnixEndDate = eDate;

                Reason = ListPaidLeave[0]["reason"];
                IsNull = false;
            }
        }
    }

    public class Training : ContractorActivity
    {
        public bool IsInTraining { get; private set; }
        public List<QueryResultRow> ListTraining { get; private set; }
        public string Type { get; private set; } = "";
        public string Address { get; private set; } = "";
        public string Trainer { get; private set; } = "";
        public bool IsNull { get; private set; } = true;

        public Training(int contractorId, long date = -1)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@contractorId", contractorId }
            };
            string query = "SELECT type, address, formateur, date FROM Training WHERE contractorId = @contractorId";

            if (date > 0)
            {
                var dateTime = DateTimeOffset.FromUnixTimeSeconds(date).UtcDateTime.Date;
                var startOfDay = new DateTimeOffset(dateTime).ToUnixTimeSeconds();
                var endOfDay = new DateTimeOffset(dateTime.AddDays(1)).ToUnixTimeSeconds() - 1;

                query += " AND date BETWEEN @startOfDay AND @endOfDay";
                parameters["@startOfDay"] = startOfDay;
                parameters["@endOfDay"] = endOfDay;
            }
            query += " ORDER BY date DESC";

            ListTraining = FetchData(query, parameters);
            if (ListTraining.Count > 0)
            {
                IsInTraining = true;
                Type = ListTraining[0]["type"];
                Address = ListTraining[0]["address"];
                Trainer = ListTraining[0]["formateur"];
                IsNull = false;
            }
        }
    }

    public class Mission : ContractorActivity
    {
        public bool IsInMission { get; private set; }
        public List<QueryResultRow> ListMission { get; private set; }
        public string Type { get; private set; } = "";
        public string Address { get; private set; } = "";
        public string Description { get; private set; } = "";
        public bool IsNull { get; private set; } = true;

        public Mission(int contractorId, long date = -1)
        {
            var parameters = new Dictionary<string, object> { { "@contractorId", contractorId } };
            string query = "SELECT type, address, description, date FROM Mission WHERE contractorId = @contractorId";

            if (date > 0)
            {
                var dateTime = DateTimeOffset.FromUnixTimeSeconds(date).UtcDateTime.Date;
                var startOfDay = new DateTimeOffset(dateTime).ToUnixTimeSeconds();
                var endOfDay = new DateTimeOffset(dateTime.AddDays(1)).ToUnixTimeSeconds() - 1;

                query += " AND date BETWEEN @startOfDay AND @endOfDay";
                parameters["@startOfDay"] = startOfDay;
                parameters["@endOfDay"] = endOfDay;
            }
            query += " ORDER BY date DESC";

            ListMission = FetchData(query, parameters);
            if (ListMission.Count > 0)
            {
                IsInMission = true;
                Type = ListMission[0]["type"];
                Address = ListMission[0]["address"];
                Description = ListMission[0]["description"];
                IsNull = false;
            }
        }
    }

    public class WorkTravel : ContractorActivity
    {
        public bool IsInWorkTravel { get; private set; }
        public List<QueryResultRow> ListWorkTravel { get; private set; }
        public string StartDate { get; private set; } = "";
        public string EndDate { get; private set; } = "";
        public long UnixStartDate { get; private set; }
        public long UnixEndDate { get; private set; }
        public string Address { get; private set; } = "";
        public string Description { get; private set; } = "";
        public bool IsNull { get; private set; } = true;

        public WorkTravel(int contractorId, long date = -1)
        {
            var parameters = new Dictionary<string, object> { { "@contractorId", contractorId } };
            string query = "SELECT startDate, endDate, address, description FROM WorkTravel WHERE contractorId = @contractorId";

            if (date > 0)
            {
                var dateTime = DateTimeOffset.FromUnixTimeSeconds(date).UtcDateTime.Date;
                var startOfDay = new DateTimeOffset(dateTime).ToUnixTimeSeconds();

                query += " AND endDate >= @startOfDay AND startDate <= @startOfDay";
                parameters["@startOfDay"] = startOfDay;
            }
            query += " ORDER BY endDate DESC";

            ListWorkTravel = FetchData(query, parameters);
            if (ListWorkTravel.Count > 0)
            {
                IsInWorkTravel = true;
                _ = long.TryParse(ListWorkTravel[0]["startDate"], out long sDate);
                _ = long.TryParse(ListWorkTravel[0]["endDate"], out long eDate);

                DateTime sdate = DateTimeOffset.FromUnixTimeSeconds(sDate).DateTime.Date;
                DateTime edate = DateTimeOffset.FromUnixTimeSeconds(eDate).DateTime.Date;
                StartDate = sdate.ToString("dd/MM/yyyy");
                EndDate = edate.ToString("dd/MM/yyyy");

                UnixStartDate = sDate;
                UnixEndDate = eDate;

                Address = ListWorkTravel[0]["address"];
                Description = ListWorkTravel[0]["description"];
                IsNull = false;
            }
        }
    }

    public class Contracts : ContractorActivity
    {
        public int ContractorId { get; private set; }
        public string Fullname { get; private set; } = "";
        public string GSM { get; private set; } = "";
        public string Email { get; private set; } = "";
        public string Address { get; private set; } = "";
        public int StartDate { get; private set; }
        public int EndDate { get; set; }
        public int Hours { get; private set; }
        public double Salary { get; private set; }
        public string Type { get; private set; } = "";
        public string Locality { get; private set; } = "";
        public int ResponsableId { get; private set; }
        public string SignedDocument { get; private set; } = "";
        public bool isNull { get; private set; } = true;

        public Contracts(string fullName)
        {
            var parameters = new Dictionary<string, object> { { "@name", fullName } };
            string query = "SELECT contractorId, fullname, gsm, email, address, startDate, endDate, hours, salary, type, locality, responsableId, signedDocument FROM Contracts WHERE fullName LIKE @name ORDER BY endDate DESC";

            var result = FetchData(query, parameters);
            if (result.Count > 0)
            {
                var row = result[0];
                _ = int.TryParse(row["contractorId"], out int id);
                _ = int.TryParse(row["startDate"], out int sDate);
                _ = int.TryParse(row["endDate"], out int eDate);
                _ = int.TryParse(row["hours"], out int hrs);
                _ = double.TryParse(row["salary"], out double sal);
                _ = int.TryParse(row["responsable"], out int resp);

                ContractorId = id;
                Fullname = row["fullname"];
                GSM = row["gsm"];
                Email = row["email"];
                Address = row["address"];
                StartDate = sDate;
                EndDate = eDate;
                Hours = hrs;
                Salary = sal;
                ResponsableId = resp;
                Type = row["type"];
                Locality = row["locality"];
                SignedDocument = row["signedDocuments"];
                isNull = false;
            }
        }
    }
}