namespace Gestionnaire
{
    public class QueryResultRow
    {
        public Dictionary<string, string> Columns { get; } = [];
        public string this[string columnName] => Columns.TryGetValue(columnName, out var value) ? value : string.Empty;
    }

    public interface IContractorActivity
    {
        List<QueryResultRow> FetchData(string query, Dictionary<string, object> parameters);
        bool InsertData(string query, Dictionary<string, object> parameters);
    }


    public class ContractorActivity : IContractorActivity
    {
        public List<QueryResultRow> FetchData(string query, Dictionary<string, object> parameters)
        {
            /*
                Executes a query with given SQL and parameters.
                Returns a list of result rows or empty list on error.
                @param query - SQL query string
                @param parameters - query parameters
            */
            try
            {
                return Program.Controller.ReadData(query, parameters);
            }
            catch (Exception ex)
            {
                Methodes.PrintConsole(Config.sourceDataset, ex.ToString(), true);
                return new List<QueryResultRow>();
            }
        }

        public bool InsertData(string query, Dictionary<string, object> parameters)
        {
            /*
                Executes an insert query with parameters.
                Returns true if successful, false on error.
                @param query - SQL insert string
                @param parameters - query parameters
            */
            try
            {
                Program.Controller.InsertData(query, parameters);
                return true;
            }
            catch (Exception ex)
            {
                if (!Config.productionRun)
                    Methodes.PrintConsole(Config.sourceDataset, ex.ToString(), false);
                else
                    return false;
            }
            return false;
        }
    }

    public class Jobs : ContractorActivity
    {
        public int AuthorityLevel { get; private set; }
        public int JobId { get; private set; }
        public string Name { get; private set; } = "";
        public List<QueryResultRow> ListFonctions { get; private set; }
        public bool IsNull { get; private set; } = true;

        public Jobs(string jobName = "")
        {
            var parameters = new Dictionary<string, object>
            {
                { "@jobName", jobName },
            };
            string query = "SELECT authorityLevel, id, name FROM Jobs";
            if (!string.IsNullOrWhiteSpace(jobName)) query += " WHERE name LIKE @jobName LIMIT 1";
            else query += " ORDER BY authorityLevel ASC";

            ListFonctions = FetchData(query, parameters);

            if (ListFonctions.Count > 0)
            {
                _ = int.TryParse(ListFonctions[0]["authorityLevel"], out int prasedOutput);
                AuthorityLevel = prasedOutput;
                _ = int.TryParse(ListFonctions[0]["id"], out prasedOutput);
                JobId = prasedOutput;
                Name = ListFonctions[0]["name"];
                IsNull = false;
            }
        }
        public bool InsertNewJob(string jobName, int authorityLevel)
        {
            /*
                Inserts a new job into the database if it doesn't exist.
                @param jobName - name of the job
                @param authorityLevel - authority level of the job
                @return true if insertion is successful
            */
            string query = "";
            var parameters = new Dictionary<string, object>
            {
                { "@jobName", jobName },
                { "@authorityLevel", authorityLevel }
            };
            query = "INSERT IGNORE INTO Jobs (name, authorityLevel) SELECT * FROM (SELECT @jobName AS name, @authorityLevel AS authorityLevel) AS tmp WHERE NOT EXISTS (SELECT 1 FROM Jobs WHERE name = @jobName)";
            return InsertData(query, parameters);
        }
        
        public bool ModifyJob(string jobName, int authorityLevel)
        {
            /*
                Updates the job's name or authority level.
                @param jobName - new job name (use "0" to skip)
                @param authorityLevel - new authority level (use 1 or less to skip)
                @return true if update is performed
            */
            int paramsCount = 0;
            string query = "";
            var parameters = new Dictionary<string, object> {};
            query = "UPDATE Jobs SET ";
            if (jobName != "0")
            {
                query += "name = @jobName";
                parameters["@jobName"] = jobName;
                Name = jobName;
                paramsCount += 1;
            }
            if (authorityLevel > 1)
            {
                if (paramsCount > 0) query += ", authorityLevel = @authorityLevel ";
                else query += "authorityLevel = @authorityLevel ";
                parameters["@authorityLevel"] = authorityLevel;
                AuthorityLevel = authorityLevel;
                paramsCount += 1;
            }
            parameters["@id"] = JobId;
            query += "WHERE id = @id";
            Console.WriteLine(query);
            if (paramsCount > 0) return InsertData(query, parameters);
            else return false;
        }
        public bool DeleteJob()
        {
            /*
                Deletes the current job from the database.
                @return true if deletion is successful
            */
            string query = "";
            var parameters = new Dictionary<string, object>
            {
                { "@jobId", JobId }
            };
            query = "DELETE FROM Jobs WHERE id = @jobId";
            return InsertData(query, parameters);
        }
    }

    public class Absence : ContractorActivity
    {
        public bool IsAbsent { get; private set; }
        public List<QueryResultRow> ListAbsence { get; private set; }
        public string JustificativeDocument { get; private set; } = "";
        public long DateOfAbsence { get; private set; }
        public bool IsNull { get; private set; } = true;

        public Absence(int contractorId, long date = -1)
        {
            /*
                Loads absence information for a contractor.
                If a date is provided, fetches only absences for that day.
                @param contractorId - ID of the contractor
                @param date - (optional) specific date in Unix timestamp
            */
            var parameters = new Dictionary<string, object>
            {
                { "@contractorId", contractorId },
            };
            string query = "SELECT justificativeDocument, date FROM Absences WHERE contractorId = @contractorId";
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
                JustificativeDocument = ListAbsence[0]["justificativeDocument"];
                _ = long.TryParse(ListAbsence[0]["date"], out long unixDate);
                DateOfAbsence = unixDate;
                IsNull = false;
            }
        }
        public bool DeclareAbsence(int contractorId, long date)
        {
            /*
                Declares an absence for a contractor on a specific date.
                @param contractorId - ID of the contractor
                @param date - absence date in Unix timestamp
                @return true if the insert succeeds, false otherwise
            */
            string query = "";
            var parameters = new Dictionary<string, object>
            {
                { "@contractorId", contractorId },
                { "@date", date }
            };
            query = "INSERT IGNORE INTO Absences (contractorId, date) VALUES (@contractorId, @date)";
            return InsertData(query, parameters);
        }
        public bool DeclareJustificative(int contractorId, string justificative)
        {
            /*
                Attaches a justificative document to the contractor's absence record.
                @param contractorId - ID of the contractor
                @param justificative - document filename or reference
                @return true if update is successful, false otherwise
            */
            string query = "";
            var parameters = new Dictionary<string, object>
            {
                { "@contractorId", contractorId },
                { "@justificativeDocument", justificative },
                { "@date", DateOfAbsence }
            };
            query = "UPDATE Absences SET justificativeDocument = @justificativeDocument WHERE contractorId = @contractorId AND date = @date";
            return InsertData(query, parameters);
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
            /*
                Loads paid leave information for a contractor.
                If a date is provided, filters results to relevant range.
                @param contractorId - ID of the contractor
                @param date - (optional) date to check paid leave status
            */
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
                _ = long.TryParse(ListPaidLeave[0]["startDate"], out long Date);
                DateTime datetime = DateTimeOffset.FromUnixTimeSeconds(Date).DateTime.Date;
                StartDate = datetime.ToString("dd/MM/yyyy") ?? "";
                UnixStartDate = Date;

                _ = long.TryParse(ListPaidLeave[0]["endDate"], out Date);
                datetime = DateTimeOffset.FromUnixTimeSeconds(Date).DateTime.Date;
                EndDate = datetime.ToString("dd/MM/yyyy") ?? "";
                UnixEndDate = Date;

                Reason = ListPaidLeave[0]["reason"];
                IsNull = false;
            }
        }
        public bool AuthorizePaidLeave(Dictionary<string, object> parameters)
        {
            /*
                Inserts a new paid leave entry for a contractor.
                @param parameters - dictionary of query parameters including dates and reason
                @return true if insertion is successful, false otherwise
            */
            string query = "";
            query = "INSERT IGNORE INTO PaidLeave (contractorId, startDate, endDate, reason) VALUES (@contractorId, @startDate, @endDate, @reason)";
            return InsertData(query, parameters);
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
            /*
                Loads training session information for a contractor.
                If a date is provided, checks if training exists on that date.
                @param contractorId - ID of the contractor
                @param date - (optional) date to filter training records
            */
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
        public bool AuthorizeTraining(Dictionary<string, object> parameters)
        {
            /*
                Adds a new training session record for a contractor.
                @param parameters - dictionary including type, address, trainer, and date
                @return true if insertion is successful, false otherwise
            */
            string query = "";
            query = "INSERT IGNORE INTO Training (contractorId, type, address, formateur, date) VALUES (@contractorId, @type, @address, @formateur, @date)";
            return InsertData(query, parameters);
        }
    }

    public class Mission : ContractorActivity
    {
        public bool IsInMission { get; private set; }
        public List<QueryResultRow> ListMission { get; private set; }
        public long DateMission { get; private set; }
        public string Description { get; private set; } = "";
        public bool IsNull { get; private set; } = true;

        public Mission(int contractorId, long date = -1)
        {
            /*
                Loads mission data for a contractor.
                If a date is provided, checks if a mission exists on that date.
                @param contractorId - ID of the contractor
                @param date - (optional) date in Unix timestamp
            */
            var parameters = new Dictionary<string, object> { { "@contractorId", contractorId } };
            string query = "SELECT description, date FROM Mission WHERE contractorId = @contractorId";

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
                Description = ListMission[0]["description"];
                _ = long.TryParse(ListMission[0]["date"], out long DateMiss);
                DateMission = DateMiss;
                IsNull = false;
            }
        }
        public bool AssignMission(Dictionary<string, object> parameters)
        {
            /*
                Assigns a mission to a contractor.
                @param parameters - dictionary containing description and date
                @return true if insertion is successful, false otherwise
            */
            string query = "";
            query = "INSERT IGNORE INTO Mission (contractorId, description, date) VALUES (@contractorId, @description, @date)";
            return InsertData(query, parameters);
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
            /*
                Loads work travel information for a contractor.
                If a date is provided, checks if travel overlaps that date.
                @param contractorId - ID of the contractor
                @param date - (optional) date to check travel status
            */
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
                _ = long.TryParse(ListWorkTravel[0]["startDate"], out long Date);
                DateTime datetime = DateTimeOffset.FromUnixTimeSeconds(Date).DateTime.Date;
                StartDate = datetime.ToString("dd/MM/yyyy");
                UnixStartDate = Date;

                _ = long.TryParse(ListWorkTravel[0]["endDate"], out Date);
                datetime = DateTimeOffset.FromUnixTimeSeconds(Date).DateTime.Date;
                EndDate = datetime.ToString("dd/MM/yyyy");
                UnixEndDate = Date;

                Address = ListWorkTravel[0]["address"];
                Description = ListWorkTravel[0]["description"];
                IsNull = false;
            }
        }
        public bool AuthorizeWorkTravel(Dictionary<string, object> parameters)
        {
            /*
                Inserts a new work travel entry for a contractor.
                @param parameters - dictionary including dates, address, and description
                @return true if insertion is successful, false otherwise
            */
            string query = "";
            query = "INSERT IGNORE INTO WorkTravel (contractorId, startDate, endDate, address, description) VALUES (@contractorId, @startDate, @endDate, @address, @description)";
            return InsertData(query, parameters);
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
        public string Job { get; private set; } = "";
        public bool IsNull { get; private set; } = true;

        public Contracts(string fullName = "")
        {
            /*
                Loads contract data for a contractor based on full name.
                @param fullName - (optional) full name of the contractor
            */
            if (!string.IsNullOrWhiteSpace(fullName))
            {
                var parameters = new Dictionary<string, object> { { "@name", fullName } };
                string query = "SELECT contractorId, fullname, gsm, email, address, startDate, endDate, hours, salary, fonction FROM Contracts WHERE fullName LIKE @name ORDER BY endDate DESC";

                var result = FetchData(query, parameters);
                if (result.Count > 0)
                {
                    var row = result[0];
                    _ = int.TryParse(row["contractorId"], out int id);
                    _ = int.TryParse(row["startDate"], out int sDate);
                    _ = int.TryParse(row["endDate"], out int eDate);
                    _ = int.TryParse(row["hours"], out int hrs);
                    _ = double.TryParse(row["salary"], out double sal);

                    ContractorId = id;
                    Fullname = row["fullname"];
                    GSM = row["gsm"];
                    Email = row["email"];
                    Address = row["address"];
                    StartDate = sDate;
                    EndDate = eDate;
                    Hours = hrs;
                    Salary = sal;
                    Job = row["fonction"];
                    IsNull = false;
                }
            }
        }
        public bool UpdateSalary(double salary)
        {
            /*
                Updates the salary of a contractor.
                @param salary - new salary value
                @return true if update is successful, false otherwise
            */
            var parameters = new Dictionary<string, object>
            {
                { "@contractorId", ContractorId },
                { "@salary", salary }
            };

            string query = "";
            query = "UPDATE Contracts SET salary = @salary WHERE contractorId = @contractorId";
            return InsertData(query, parameters);
        }

        public bool InsertContract(Dictionary<string, object> parameters)
        {
            /*
                Inserts a new contract and creates a recurring raise mysql event.
                @param parameters - dictionary containing contract details
                @return true if insertion is successful, false otherwise
            */
            string query = "";
            query = "INSERT INTO Contracts (fullname, gsm, email, address, endDate, hours, salary, fonction) SELECT * FROM (SELECT @fullName AS fullname, @gsm AS gsm, @email AS email, @address AS address, @endDate AS endDate, @hours AS hours, @salary AS salary, @job AS fonction) AS tmp WHERE NOT EXISTS (SELECT 1 FROM contractor WHERE (gsm = @gsm AND (endDate = 0 OU endDate > UNIX_TIMESTAMP())));";
            query += $"CREATE EVENT contract_{GSM.Replace(" ", "_")} ON SCHEDULE EVERY 1 MONTH DO UPDATE Contract SET salary = salary + (salary * 0.02), lastRaise = UNIX_TIMESTAMP() WHERE startDate <= UNIX_TIMESTAMP(DATE_SUB(NOW(), INTERVAL 2 YEAR)) AND (lastRaise = 0 OR lastRaise <= UNIX_TIMESTAMP(DATE_SUB(NOW(), INTERVAL 2 YEAR)));";
            return InsertData(query, parameters);
        }

        public bool EndContract()
        {
            /*
                Ends an active contract by setting the end date.
                Also removes the associated salary raise event.
                @return true if update and event removal are successful, false otherwise
            */
            string query = "";
            var parameters = new Dictionary<string, object>
            {
                { "@contractorId", ContractorId }
            };
            query = "UPDATE Contracts SET endDate = UNIX_TIMESTAMP() WHERE contractorId = @contractorId;";
            query += $"DROP EVENT IF EXISTS contract_{GSM.Replace(" ", "_")};";
            return InsertData(query, parameters);
        }
    }
}