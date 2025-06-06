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
        public List<QueryResultRow> ListAbsence;
        public string Reason { get; private set; } = "";
        public string JustificativeDocument { get; private set; } = "";
        public bool isNull { get; private set; } = true;

        public Absence(int contractorId, int date = 0)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@contractorId", contractorId }
            };

            string query = "SELECT reason, justificativeDocument FROM Absences WHERE contractorId = @contractorId";
            if (date != 0)
            {
                query += " AND date = @date";
                parameters["@date"] = date;
            }
            query += " ORDER BY date DESC";

            ListAbsence = FetchData(query, parameters);

            if (ListAbsence.Count > 0)
            {
                IsAbsent = true;
                Reason = ListAbsence[0]["reason"];
                JustificativeDocument = ListAbsence[0]["justificativeDocument"];
                isNull = false;
            }
        }
    }

    public class PaidLeave : ContractorActivity
    {
        public bool IsInPaidLeave { get; private set; }
        public int StartDate { get; private set; }
        public int EndDate { get; private set; }
        public string Reason { get; private set; } = "";
        public bool isNull { get; private set; } = true;

        public PaidLeave(int contractorId, int date = 0)
        {
            var parameters = new Dictionary<string, object> { { "@contractorId", contractorId } };
            string query = "SELECT startDate, endDate, reason FROM PaidLeave WHERE contractorId = @contractorId";

            if (date != 0)
            {
                query += " AND (startDate >= @date && endDate <= @date)";
                parameters["@date"] = date;
            }
            query += " ORDER BY date DESC";

            var result = FetchData(query, parameters);
            if (result.Count > 0)
            {
                IsInPaidLeave = true;
                _ = int.TryParse(result[0]["startDate"], out int sDate);
                _ = int.TryParse(result[0]["endDate"], out int eDate);
                StartDate = sDate;
                EndDate = eDate;
                Reason = result[0]["reason"];
                isNull = false;
            }
        }
    }

    public class Training : ContractorActivity
    {
        public bool IsInTraining { get; private set; }
        public string Type { get; private set; } = "";
        public string Address { get; private set; } = "";
        public string Trainer { get; private set; } = "";
        public bool isNull { get; private set; } = true;

        public Training(int contractorId, int date = 0)
        {
            var parameters = new Dictionary<string, object> { { "@contractorId", contractorId } };
            string query = "SELECT type, address, formateur FROM Trainings WHERE contractorId = @contractorId";

            if (date != 0)
            {
                query += " AND date = @date";
                parameters["@date"] = date;
            }
            query += " ORDER BY date DESC";

            var result = FetchData(query, parameters);
            if (result.Count > 0)
            {
                IsInTraining = true;
                Type = result[0]["type"];
                Address = result[0]["address"];
                Trainer = result[0]["formateur"];
                isNull = false;
            }
        }
    }

    public class Mission : ContractorActivity
    {
        public bool IsInMission { get; private set; }
        public string Type { get; private set; } = "";
        public string Address { get; private set; } = "";
        public string Description { get; private set; } = "";
        public bool isNull { get; private set; } = true;

        public Mission(int contractorId, int date = 0)
        {
            var parameters = new Dictionary<string, object> { { "@contractorId", contractorId } };
            string query = "SELECT type, address, description FROM Mission WHERE contractorId = @contractorId";

            if (date != 0)
            {
                query += " AND date = @date";
                parameters["@date"] = date;
            }
            query += " ORDER BY date DESC";

            var result = FetchData(query, parameters);
            if (result.Count > 0)
            {
                IsInMission = true;
                Type = result[0]["type"];
                Address = result[0]["address"];
                Description = result[0]["description"];
                isNull = false;
            }
        }
    }

    public class WorkTravel : ContractorActivity
    {
        public bool IsInWorkTravel { get; private set; }
        public int StartDate { get; private set; }
        public int EndDate { get; private set; }
        public string Address { get; private set; } = "";
        public string Description { get; private set; } = "";
        public bool isNull { get; private set; } = true;
        public WorkTravel(int contractorId, int date = 0)
        {
            var parameters = new Dictionary<string, object> { { "@contractorId", contractorId } };
            string query = "SELECT startDate, endDate, address, description FROM WorkTravel WHERE contractorId = @contractorId";

            if (date != 0)
            {
                query += " AND (startDate >= @date && endDate <= @date)";
                parameters["@date"] = date;
            }
            query += " ORDER BY date DESC";

            var result = FetchData(query, parameters);
            if (result.Count > 0)
            {
                IsInWorkTravel = true;
                _ = int.TryParse(result[0]["startDate"], out int sDate);
                _ = int.TryParse(result[0]["endDate"], out int eDate);
                StartDate = sDate;
                EndDate = eDate;
                Address = result[0]["address"];
                Description = result[0]["description"];
                isNull = false;
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
            string query = "SELECT contractorId, fullname, gsm, email, address, startDate, endDate, hours, salary, type, locality, responsable, signedDocuments FROM Contracts WHERE fullName LIKE @name AND (endDate < CURRENT_TIMESTAMP) ORDER BY endDate DESC";

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