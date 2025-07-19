namespace Gestionnaire
{
    /// <summary>
    /// Représente une ligne de résultat d'une requête avec un dictionnaire colonnes/valeurs.
    /// </summary>
    public class QueryResultRow
    {
        public Dictionary<string, string> Columns { get; } = [];
        public string this[string columnName] => Columns.TryGetValue(columnName, out var value) ? value : string.Empty;
    }

    /// <summary>
    /// Interface définissant les méthodes d'activité d'un contractant.
    /// </summary>
    public interface IContractorActivity
    {
        /// <summary>
        /// Exécute une requête de lecture avec paramètres et retourne la liste des résultats.
        /// </summary>
        /// <param name="query">Requête SQL.</param>
        /// <param name="parameters">Paramètres de la requête.</param>
        /// <returns>Liste de résultats.</returns>
        List<QueryResultRow> FetchData(string query, Dictionary<string, object> parameters);

        /// <summary>
        /// Exécute une requête d'insertion avec paramètres.
        /// </summary>
        /// <param name="query">Requête SQL.</param>
        /// <param name="parameters">Paramètres de la requête.</param>
        /// <returns>True si succès, false sinon.</returns>
        bool InsertData(string query, Dictionary<string, object> parameters);
    }

    /// <summary>
    /// Implémentation de IContractorActivity utilisant Program.Controller.
    /// </summary>
    public class ContractorActivity : IContractorActivity
    {
        /// <inheritdoc/>
        public List<QueryResultRow> FetchData(string query, Dictionary<string, object> parameters)
        {
            return Program.Controller.ReadData(query, parameters);
        }
        /// <inheritdoc/>
        public bool InsertData(string query, Dictionary<string, object> parameters)
        {
            return Program.Controller.InsertData(query, parameters);
        }
    }
    
    /// <summary>
    /// Représente un poste (job) avec ses propriétés et méthodes associées.
    /// </summary>
    public class Jobs : ContractorActivity
    {
        /// <summary> Niveau d'autorité du poste. </summary>
        public int AuthorityLevel { get; private set; }
        /// <summary> Identifiant du poste. </summary>
        public int JobId { get; private set; }
        /// <summary> Nom du poste. </summary>
        public string Name { get; private set; } = "";
        /// <summary> Liste des fonctions liées au poste. </summary>
        public List<QueryResultRow> ListFonctions { get; private set; }
        /// <summary> Indique si l'objet Jobs est vide ou non. </summary>
        public bool IsNull { get; private set; } = true;

        /// <summary>
        /// Constructeur, charge un poste à partir du nom (optionnel).
        /// </summary>
        /// <param name="jobName">Nom du poste à charger (optionnel).</param>
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

        /// <summary>
        /// Insère un nouveau poste dans la base si inexistant.
        /// </summary>
        /// <param name="jobName">Nom du poste.</param>
        /// <param name="authorityLevel">Niveau d'autorité.</param>
        /// <returns>True si insertion réussie.</returns>
        public bool InsertNewJob(string jobName, int authorityLevel)
        {
            string query = "";
            var parameters = new Dictionary<string, object>
            {
                { "@jobName", jobName },
                { "@authorityLevel", authorityLevel }
            };
            query = "INSERT IGNORE INTO Jobs (name, authorityLevel) SELECT * FROM (SELECT @jobName AS name, @authorityLevel AS authorityLevel) AS tmp WHERE NOT EXISTS (SELECT 1 FROM Jobs WHERE name = @jobName)";
            return InsertData(query, parameters);
        }

        /// <summary>
        /// Modifie le nom ou le niveau d'autorité du poste.
        /// </summary>
        /// <param name="jobName">Nouveau nom (utiliser "0" pour ignorer).</param>
        /// <param name="authorityLevel">Nouveau niveau d'autorité (<=1 pour ignorer).</param>
        /// <returns>True si modification effectuée.</returns>
        public bool ModifyJob(string jobName, int authorityLevel)
        {
            int paramsCount = 0;
            string query = "";
            var parameters = new Dictionary<string, object> { };
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
            if (paramsCount > 0) return InsertData(query, parameters);
            else return false;
        }

        /// <summary>
        /// Supprime ce poste de la base.
        /// </summary>
        /// <returns>True si suppression réussie.</returns>
        public bool DeleteJob()
        {
            string query = "";
            var parameters = new Dictionary<string, object>
            {
                { "@jobId", JobId }
            };
            query = "DELETE FROM Jobs WHERE id = @jobId";
            return InsertData(query, parameters);
        }
    }
    
    /// <summary>
    /// Représente les absences d'un contractant.
    /// </summary>
    public class Absence : ContractorActivity
    {
        /// <summary> Indique si le contractant est absent. </summary>
        public bool IsAbsent { get; private set; }
        /// <summary> Liste des absences récupérées. </summary>
        public List<QueryResultRow> ListAbsence { get; private set; }
        /// <summary> Document justificatif associé à l'absence. </summary>
        public string JustificativeDocument { get; private set; } = "";
        /// <summary> Date de l'absence en timestamp Unix. </summary>
        public long DateOfAbsence { get; private set; }
        /// <summary> Indique si l'objet Absence est vide ou non. </summary>
        public bool IsNull { get; private set; } = true;

        /// <summary>
        /// Constructeur qui charge les informations d'absence pour un contractant et une date optionnelle.
        /// </summary>
        /// <param name="contractorId">ID du contractant.</param>
        /// <param name="date">Date optionnelle en timestamp Unix.</param>
        public Absence(int contractorId, long date = -1)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@contractorId", contractorId },
            };
            string query = "SELECT justificativeDocument, date FROM Absences WHERE contractorId = @contractorId";
            if (date > 0)
            {
                query += " AND date BETWEEN @startOfDay AND @endOfDay";
                parameters["@startOfDay"] = date;
                parameters["@endOfDay"] = date + 86399;
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

        /// <summary>
        /// Déclare une absence pour un contractant à une date donnée.
        /// </summary>
        /// <param name="contractorId">ID du contractant.</param>
        /// <param name="date">Date de l'absence en timestamp Unix.</param>
        /// <returns>True si l'insertion réussit.</returns>
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
            if (Config.productionRun) query = "INSERT IGNORE INTO Absences (contractorId, date) VALUES (@contractorId, @date)";
            else query = "INSERT IGNORE INTO Absences (contractorId, date, justificativeDocument) VALUES (@contractorId, @date, 'justificative_absence_Test.pdf')";
            return InsertData(query, parameters);
        }

        /// <summary>
        /// Ajoute un document justificatif à une absence.
        /// </summary>
        /// <param name="contractorId">ID du contractant.</param>
        /// <param name="justificative">Nom ou référence du document justificatif.</param>
        /// <returns>True si la mise à jour réussit.</returns>
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
    
    /// <summary>
    /// Représente les congés payés d'un contractant.
    /// </summary>
    public class PaidLeave : ContractorActivity
    {
        /// <summary> Indique si le contractant est en congé payé. </summary>
        public bool IsInPaidLeave { get; private set; }
        /// <summary> Liste des congés payés récupérés. </summary>
        public List<QueryResultRow> ListPaidLeave { get; private set; }
        /// <summary> Date de début du congé (format string). </summary>
        public string StartDate { get; private set; } = "";
        /// <summary> Date de fin du congé (format string). </summary>
        public string EndDate { get; private set; } = "";
        /// <summary> Date de début du congé (timestamp Unix). </summary>
        public long UnixStartDate { get; private set; }
        /// <summary> Date de fin du congé (timestamp Unix). </summary>
        public long UnixEndDate { get; private set; }
        /// <summary> Raison du congé. </summary>
        public string Reason { get; private set; } = "";
        /// <summary> Indique si l'objet PaidLeave est vide ou non. </summary>
        public bool IsNull { get; private set; } = true;

        /// <summary>
        /// Constructeur qui charge les informations de congé payé pour un contractant et une date optionnelle.
        /// </summary>
        /// <param name="contractorId">ID du contractant.</param>
        /// <param name="date">Date optionnelle pour filtrer le congé.</param>
        public PaidLeave(int contractorId, long date = -1)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@contractorId", contractorId }
            };
            string query = "SELECT startDate, endDate, reason FROM PaidLeave WHERE contractorId = @contractorId";

            if (date > 0)
            {
                query += " AND @leaveDate BETWEEN startDate AND endDate";
                parameters["@leaveDate"] = date;
            }
            query += " ORDER BY endDate DESC";

            ListPaidLeave = FetchData(query, parameters);
            if (ListPaidLeave.Count > 0)
            {
                IsInPaidLeave = true;
                _ = long.TryParse(ListPaidLeave[0]["startDate"], out long Date);
                DateTime datetime = DateTimeOffset.FromUnixTimeSeconds(Date).UtcDateTime.Date;
                StartDate = datetime.ToString("dd/MM/yyyy") ?? "";
                UnixStartDate = Date;

                _ = long.TryParse(ListPaidLeave[0]["endDate"], out Date);
                datetime = DateTimeOffset.FromUnixTimeSeconds(Date).UtcDateTime.Date;
                EndDate = datetime.ToString("dd/MM/yyyy") ?? "";
                UnixEndDate = Date;

                Reason = ListPaidLeave[0]["reason"];
                IsNull = false;
            }
        }

        /// <summary>
        /// Autorise un nouveau congé payé pour un contractant.
        /// </summary>
        /// <param name="parameters">Dictionnaire des paramètres de la requête (dates, raison, etc.).</param>
        /// <returns>True si l'insertion réussit.</returns>
        public bool AuthorizePaidLeave(Dictionary<string, object> parameters)
        {
            string query = "";
            query = "INSERT IGNORE INTO PaidLeave (contractorId, startDate, endDate, reason) VALUES (@contractorId, @startDate, @endDate, @reason)";
            return InsertData(query, parameters);
        }
    }

    /// <summary>
    /// Représente une session de formation d'un contractant.
    /// </summary>
    public class Training : ContractorActivity
    {
        /// <summary>Indique si le contractant est en formation.</summary>
        public bool IsInTraining { get; private set; }
        /// <summary>Liste des sessions de formation.</summary>
        public List<QueryResultRow> ListTraining { get; private set; }
        /// <summary>Type de formation.</summary>
        public string Type { get; private set; } = "";
        /// <summary>Adresse de la formation.</summary>
        public string Address { get; private set; } = "";
        /// <summary>Nom du formateur.</summary>
        public string Trainer { get; private set; } = "";
        /// <summary>Indique si l'objet est vide (aucune formation trouvée).</summary>
        public bool IsNull { get; private set; } = true;

        /// <summary>
        /// Charge les informations de formation pour un contractant, éventuellement filtrées par date.
        /// </summary>
        /// <param name="contractorId">ID du contractant.</param>
        /// <param name="date">Date optionnelle en timestamp Unix pour filtrer la formation.</param>
        public Training(int contractorId, long date = -1)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@contractorId", contractorId }
            };
            string query = "SELECT type, address, formateur, date FROM Training WHERE contractorId = @contractorId";

            if (date > 0)
            {
                query += " AND date BETWEEN @startOfDay AND @endOfDay";
                parameters["@startOfDay"] = date;
                parameters["@endOfDay"] = date + 86399;
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

        /// <summary>
        /// Autorise (ajoute) une nouvelle session de formation pour un contractant.
        /// </summary>
        /// <param name="parameters">Dictionnaire des paramètres de la formation (type, adresse, formateur, date).</param>
        /// <returns>True si l'insertion réussit.</returns>
        public bool AuthorizeTraining(Dictionary<string, object> parameters)
        {
            string query = "";
            query = "INSERT IGNORE INTO Training (contractorId, type, address, formateur, date) VALUES (@contractorId, @type, @address, @formateur, @date)";
            return InsertData(query, parameters);
        }
    }

    /// <summary>
    /// Représente une mission assignée à un contractant.
    /// </summary>
    public class Mission : ContractorActivity
    {
        /// <summary>Indique si le contractant est en mission.</summary>
        public bool IsInMission { get; private set; }
        /// <summary>Liste des missions.</summary>
        public List<QueryResultRow> ListMission { get; private set; }
        /// <summary>Date de la mission (timestamp Unix).</summary>
        public long DateMission { get; private set; }
        /// <summary>Description de la mission.</summary>
        public string Description { get; private set; } = "";
        /// <summary>Indique si l'objet est vide (aucune mission trouvée).</summary>
        public bool IsNull { get; private set; } = true;

        /// <summary>
        /// Charge les informations de mission pour un contractant, éventuellement filtrées par date.
        /// </summary>
        /// <param name="contractorId">ID du contractant.</param>
        /// <param name="date">Date optionnelle en timestamp Unix pour filtrer la mission.</param>
        public Mission(int contractorId, long date = -1)
        {
            var parameters = new Dictionary<string, object> { { "@contractorId", contractorId } };
            string query = "SELECT description, date FROM Mission WHERE contractorId = @contractorId";

            if (date > 0)
            {
                query += " AND date BETWEEN @startOfDay AND @endOfDay";
                parameters["@startOfDay"] = date;
                parameters["@endOfDay"] = date + 86399;
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

        /// <summary>
        /// Assigne une mission à un contractant.
        /// </summary>
        /// <param name="parameters">Dictionnaire contenant la description et la date.</param>
        /// <returns>True si l'insertion réussit.</returns>
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

    /// <summary>
    /// Représente un déplacement professionnel (work travel) d'un contractant.
    /// </summary>
    public class WorkTravel : ContractorActivity
    {
        /// <summary>Indique si le contractant est en déplacement professionnel.</summary>
        public bool IsInWorkTravel { get; private set; }
        /// <summary>Liste des déplacements professionnels.</summary>
        public List<QueryResultRow> ListWorkTravel { get; private set; }
        /// <summary>Date du fin du déplacement (format string).</summary>
        public string StartDate { get; private set; } = "";
        /// <summary>Date de début du déplacement (format string).</summary>
        public string EndDate { get; private set; } = "";
        /// <summary>Date de début du déplacement (timestamp Unix).</summary>
        public long UnixStartDate { get; private set; }
        /// <summary>Date de fin du déplacement (timestamp Unix).</summary>
        public long UnixEndDate { get; private set; }
        /// <summary>Adresse du déplacement.</summary>
        public string Address { get; private set; } = "";
        /// <summary>Description du déplacement.</summary>
        public string Description { get; private set; } = "";
        /// <summary>Indique si l'objet est vide (aucun déplacement trouvé).</summary>
        public bool IsNull { get; private set; } = true;

        /// <summary>
        /// Charge les informations de déplacement professionnel pour un contractant, éventuellement filtrées par date.
        /// </summary>
        /// <param name="contractorId">ID du contractant.</param>
        /// <param name="date">Date optionnelle en timestamp Unix pour vérifier le déplacement.</param>
        public WorkTravel(int contractorId, long date = -1)
        {
            var parameters = new Dictionary<string, object> { { "@contractorId", contractorId } };
            string query = "SELECT startDate, endDate, address, description FROM WorkTravel WHERE contractorId = @contractorId";

            if (date > 0)
            {
                query += " AND @leaveDate BETWEEN startDate AND endDate";
                parameters["@leaveDate"] = date;
            }
            query += " ORDER BY endDate DESC";

            ListWorkTravel = FetchData(query, parameters);
            if (ListWorkTravel.Count > 0)
            {
                IsInWorkTravel = true;
                _ = long.TryParse(ListWorkTravel[0]["startDate"], out long Date);
                DateTime datetime = DateTimeOffset.FromUnixTimeSeconds(Date).UtcDateTime.Date;
                StartDate = datetime.ToString("dd/MM/yyyy");
                UnixStartDate = Date;

                _ = long.TryParse(ListWorkTravel[0]["endDate"], out Date);
                datetime = DateTimeOffset.FromUnixTimeSeconds(Date).UtcDateTime.Date;
                EndDate = datetime.ToString("dd/MM/yyyy");
                UnixEndDate = Date;

                Address = ListWorkTravel[0]["address"];
                Description = ListWorkTravel[0]["description"];
                IsNull = false;
            }
        }
        /// <summary>
        /// Autorise (ajoute) un nouveau déplacement professionnel pour un contractant.
        /// </summary>
        /// <param name="parameters">Dictionnaire des paramètres (dates, adresse, description).</param>
        /// <returns>True si l'insertion réussit.</returns>
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

    /// <summary>
    /// Représente un contrat de travail pour un contractant.
    /// </summary>
    public class Contracts : ContractorActivity
    {
        /// <summary>ID du contractant.</summary>
        public int ContractorId { get; private set; }
        /// <summary>Nom complet du contractant.</summary>
        public string Fullname { get; private set; } = "";
        /// <summary>Numéro GSM du contractant.</summary>
        public string GSM { get; private set; } = "";
        /// <summary>Adresse email du contractant.</summary>
        public string Email { get; private set; } = "";
        /// <summary>Adresse physique du contractant.</summary>
        public string Address { get; private set; } = "";
        /// <summary>Date de début du contrat (timestamp).</summary>
        public int StartDate { get; private set; }
        /// <summary>Date de fin du contrat (timestamp).</summary>
        public int EndDate { get; set; }
        /// <summary>Nombre d'heures contractuelles.</summary>
        public int Hours { get; private set; }
        /// <summary>Salaire du contractant.</summary>
        public double Salary { get; private set; }
        /// <summary>Fonction ou poste du contractant.</summary>
        public string Job { get; private set; } = "";
        /// <summary>Indique si aucun contrat n'a été trouvé.</summary>
        public bool IsNull { get; private set; } = true;

        /// <summary>
        /// Charge les données du contrat d'un contractant via son nom complet.
        /// </summary>
        /// <param name="fullName">Nom complet optionnel du contractant.</param>
        public Contracts(string fullName = "")
        {
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
        
        /// <summary>
        /// Met à jour le salaire du contractant.
        /// </summary>
        /// <param name="salary">Nouveau salaire.</param>
        /// <returns>True si la mise à jour réussit, false sinon.</returns>
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

        /// <summary>
        /// Insère un nouveau contrat et crée un événement MySQL pour une augmentation récurrente.
        /// </summary>
        /// <param name="parameters">Dictionnaire des paramètres du contrat.</param>
        /// <returns>True si l'insertion réussit, false sinon.</returns>
        public bool InsertContract(Dictionary<string, object> parameters)
        {
            string query = "";
            query = "INSERT INTO Contracts (fullname, gsm, email, address, endDate, hours, salary, fonction) SELECT * FROM (SELECT @fullName AS fullname, @gsm AS gsm, @email AS email, @address AS address, @endDate AS endDate, @hours AS hours, @salary AS salary, @job AS fonction) AS tmp WHERE NOT EXISTS (SELECT 1 FROM contracts WHERE (gsm = @gsm AND (endDate = 0 OR endDate > UNIX_TIMESTAMP(UTC_TIMESTAMP()))));";
            return InsertData(query, parameters);
        }

        /// <summary>
        /// Termine un contrat actif en mettant à jour la date de fin et supprime l'événement d'augmentation salariale associé.
        /// </summary>
        /// <returns>True si la mise à jour et la suppression de l'événement réussissent, false sinon.</returns>
        public bool EndContract()
        {
            string query = "";
            var parameters = new Dictionary<string, object>
            {
                { "@contractorId", ContractorId }
            };
            query = "UPDATE Contracts SET endDate = UNIX_TIMESTAMP(UTC_TIMESTAMP()) WHERE contractorId = @contractorId;";
            query += $"DROP EVENT IF EXISTS contract_{GSM.Replace(" ", "_")};";
            return InsertData(query, parameters);
        }
    }
}