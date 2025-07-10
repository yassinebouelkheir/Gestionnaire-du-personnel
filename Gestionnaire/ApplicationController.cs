using System.Text.RegularExpressions;

namespace Gestionnaire
{
    partial class ApplicationController
    {
        private readonly string ErrorMessage = "";
        private readonly Methodes UserConsole = new();
        public ApplicationController()
        {
            while (true)
            {
                if (Config.productionRun)
                {
                    Console.Clear();
                    Methodes.PrintConsole(Config.sourceApplicationController, "\n--- Menu Principal --- Gestionnaire du personnel v 1.0");
                    Methodes.PrintConsole(Config.sourceApplicationController, "S'il vous plaît, Entrer le numéro du service que vous voulez accèder:\n");
                }
                Methodes.PrintConsole(Config.sourceApplicationController, "1. L'absence/présence d'un membre");
                Methodes.PrintConsole(Config.sourceApplicationController, "2. Les membres en congé");
                Methodes.PrintConsole(Config.sourceApplicationController, "3. Les membres formation");
                Methodes.PrintConsole(Config.sourceApplicationController, "4. Les membres en mission");
                Methodes.PrintConsole(Config.sourceApplicationController, "5. Les membres en déplacements");
                Methodes.PrintConsole(Config.sourceApplicationController, "6. Paramètres global du société");
                Methodes.PrintConsole(Config.sourceApplicationController, "7. Fermer l'application");
                if (!Config.productionRun) Methodes.PrintConsole(Config.sourceApplicationController, "8. Beta testing options");
                if (ErrorMessage != "") Methodes.PrintConsole(Config.sourceApplicationController, ErrorMessage); ErrorMessage = "";
                string serviceText = Methodes.ReadUserInput("Votre choix (1-7): ") ?? string.Empty;
                bool prasedInput = int.TryParse(serviceText, out int serviceNumber);

                if (prasedInput) ErrorMessage = RunService(serviceNumber);
                else ErrorMessage = "Erreur, Votre choix doit être entre 1 et 7, Veuillez réssayer s'il vous plaît...";
            }
        }
        private string RunService(int service)
        {
            Console.WriteLine("\n");
            switch (service)
            {
                case 1:
                    {
                        GetData(out string fullname, out int contractorId, out long unixdate);
                        if (contractorId < 0) return "Erreur, Le nom du membre que vous avez entré est incorrect, Veuillez réssayer s'il vous plaît...";

                        Absence absence = new(contractorId, unixdate);
                        if (absence.IsNull)
                        {
                            Methodes.PrintConsole(Config.sourceApplicationController, fullname + " n'a aucune absence enregistré" + ((unixdate > 0) ? " ce jour là." : ".") + "\n");
                        }
                        else
                        {
                            if (unixdate < 0)
                            {
                                goto printPrompt;
                                retryprompt: Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, Votre choix doit être entre 1 et 30, Veuillez réssayer s'il vous plaît...");
                                printPrompt: string response = Methodes.ReadUserInput("Enter le total des dernières absence que vous voulez voir (1-30): ") ?? string.Empty;
                                bool prasedInput = int.TryParse(response, out int absenceCount);
                                if (absenceCount > 30 || absenceCount < 1) goto retryprompt;

                                Methodes.PrintConsole(Config.sourceApplicationController, $"Liste des dernière {absenceCount} absence pour {fullname}:");

                                int maxToShow = Math.Min(absenceCount, Math.Min(30, absence.ListAbsence.Count));
                                for (int i = 0; i < maxToShow; i++)
                                {
                                    List<QueryResultRow> row = absence.ListAbsence;
                                    string doc = !string.IsNullOrWhiteSpace(row[i]["justificativeDocument"]) ? "Déposé" : "Aucun document";
                                    string formattedDate = !string.IsNullOrWhiteSpace(row[i]["date"]) ? row[i]["date"] : "-1";

                                    bool parsedDate = long.TryParse(formattedDate, out long unixTimestamp);
                                    if (parsedDate)
                                    {
                                        DateTime date = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).DateTime.Date;
                                        formattedDate = date.ToString("dd/MM/yyyy");
                                    }
                                    else formattedDate = "Date malformé";

                                    Methodes.PrintConsole(Config.sourceApplicationController, $"- Date: {formattedDate} Document : {doc}");
                                }
                            }
                            else
                            {
                                DateTime date = DateTimeOffset.FromUnixTimeSeconds(unixdate).Date;
                                Methodes.PrintConsole(Config.sourceApplicationController, fullname + " a été absent le " + date.ToString("dd/MM/yyyy") + " Document: " + (!string.IsNullOrWhiteSpace(absence.JustificativeDocument) ? "Déposé" : "Aucun document") + ".\n");
                                if (!string.IsNullOrWhiteSpace(absence.JustificativeDocument))
                                {
                                    string response = Methodes.ReadUserInput("Est-ce que vous voulez télécharger le justificative fournis par le membre? (OUI/NON): ") ?? string.Empty;
                                    if (!string.IsNullOrWhiteSpace(response) && response.Equals("oui", StringComparison.OrdinalIgnoreCase))
                                    {
                                        Methodes.DownloadJustificative(absence.JustificativeDocument);
                                    }
                                }
                            }
                        }
                        ShowContinuePrompt();
                        break;
                    }
                case 2:
                    {
                        GetData(out string fullname, out int contractorId, out long unixdate);
                        if (contractorId < 0) return "Erreur, Le nom du membre que vous avez entré est incorrect, Veuillez réssayer s'il vous plaît...";

                        PaidLeave paidLeave = new(contractorId, unixdate);
                        if (paidLeave.IsNull)
                        {
                            Methodes.PrintConsole(Config.sourceApplicationController, fullname + " n'a aucun congé payés enregistré" + ((unixdate > 0) ? " ce jour là." : ".") + "\n");
                        }
                        else
                        {
                            if (unixdate < 0)
                            {
                                goto printPrompt;
                                retryprompt: Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, Votre choix doit être entre 1 et 30, Veuillez réssayer s'il vous plaît...");
                                printPrompt: string response = Methodes.ReadUserInput("Entrer le total des congés payés que vous voulez voir (1-30): ") ?? string.Empty;
                                bool prasedInput = int.TryParse(response, out int paidLeaveCount);
                                if (paidLeaveCount > 30 || paidLeaveCount < 1) goto retryprompt;

                                Methodes.PrintConsole(Config.sourceApplicationController, $"Liste des dernière {paidLeaveCount} congés payés pour {fullname}:");

                                int maxToShow = Math.Min(paidLeaveCount, Math.Min(30, paidLeave.ListPaidLeave.Count));
                                for (int i = 0; i < maxToShow; i++)
                                {
                                    List<QueryResultRow> row = paidLeave.ListPaidLeave;
                                    string reason = !string.IsNullOrWhiteSpace(row[i]["reason"]) ? row[i]["reason"] : "Non spécifiée";
                                    string sformattedDate = !string.IsNullOrWhiteSpace(row[i]["startDate"]) ? row[i]["startDate"] : "-1";
                                    string eformattedDate = !string.IsNullOrWhiteSpace(row[i]["endDate"]) ? row[i]["endDate"] : "-1";

                                    bool sparsedDate = long.TryParse(sformattedDate, out long sunixTimestamp);
                                    bool eparsedDate = long.TryParse(eformattedDate, out long eunixTimestamp);
                                    if (sparsedDate && eparsedDate)
                                    {
                                        DateTime sdate = DateTimeOffset.FromUnixTimeSeconds(sunixTimestamp).DateTime.Date;
                                        DateTime edate = DateTimeOffset.FromUnixTimeSeconds(eunixTimestamp).DateTime.Date;
                                        sformattedDate = sdate.ToString("dd/MM/yyyy");
                                        eformattedDate = edate.ToString("dd/MM/yyyy");
                                    }
                                    else
                                    {
                                        eformattedDate = "Date malformé";
                                        sformattedDate = "Date malformé";
                                    }

                                    Methodes.PrintConsole(Config.sourceApplicationController, $"- Date de début: {sformattedDate} Date de fin: {eformattedDate} Raison : {reason}");
                                }
                            }
                            else
                            {
                                DateTime date = DateTimeOffset.FromUnixTimeSeconds(unixdate).Date;
                                Methodes.PrintConsole(Config.sourceApplicationController, fullname + " bénéfice d'un congé payé le " + date.ToString("dd/MM/yyyy"));
                                Methodes.PrintConsole(Config.sourceApplicationController, "Date de début: " + (!string.IsNullOrWhiteSpace(paidLeave.StartDate) ? paidLeave.StartDate : "Non spécifiée") + ".\n");
                                Methodes.PrintConsole(Config.sourceApplicationController, "Date de fin: " + (!string.IsNullOrWhiteSpace(paidLeave.EndDate) ? paidLeave.EndDate : "Non spécifiée") + ".\n");
                                Methodes.PrintConsole(Config.sourceApplicationController, "Raison: " + (!string.IsNullOrWhiteSpace(paidLeave.Reason) ? paidLeave.Reason : "Non spécifiée") + ".\n");
                            }
                        }
                        ShowContinuePrompt();
                        break;
                    }
                case 3:
                    {
                        GetData(out string fullname, out int contractorId, out long unixdate);
                        if (contractorId < 0) return "Erreur, Le nom du membre que vous avez entré est incorrect, Veuillez réssayer s'il vous plaît...";

                        Training training = new(contractorId, unixdate);
                        if (training.IsNull)
                        {
                            Methodes.PrintConsole(Config.sourceApplicationController, fullname + " n'a aucune formation enregistré" + ((unixdate > 0) ? " ce jour là." : ".") + "\n");
                        }
                        else
                        {
                            if (unixdate < 0)
                            {
                                goto printPrompt;
                                retryprompt: Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, Votre choix doit être entre 1 et 30, Veuillez réssayer s'il vous plaît...");
                                printPrompt: string response = Methodes.ReadUserInput("Enter le total des dernières formations que vous voulez voir (1-30): ") ?? string.Empty;
                                bool prasedInput = int.TryParse(response, out int trainingCount);
                                if (trainingCount > 30 || trainingCount < 1) goto retryprompt;

                                Methodes.PrintConsole(Config.sourceApplicationController, $"Liste des dernière {trainingCount} formations pour {fullname}:");

                                int maxToShow = Math.Min(trainingCount, Math.Min(30, training.ListTraining.Count));
                                for (int i = 0; i < maxToShow; i++)
                                {
                                    List<QueryResultRow> row = training.ListTraining;
                                    string type = !string.IsNullOrWhiteSpace(row[i]["type"]) ? row[i]["type"] : "Non spécifiée";
                                    string formateur = !string.IsNullOrWhiteSpace(row[i]["formateur"]) ? row[i]["formateur"] : "Non spécifiée";
                                    string formattedDate = !string.IsNullOrWhiteSpace(row[i]["date"]) ? row[i]["date"] : "-1";

                                    bool parsedDate = long.TryParse(formattedDate, out long unixTimestamp);
                                    if (parsedDate)
                                    {
                                        DateTime date = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).DateTime.Date;
                                        formattedDate = date.ToString("dd/MM/yyyy");
                                    }
                                    else formattedDate = "Date malformé";

                                    Methodes.PrintConsole(Config.sourceApplicationController, $"- Date: {formattedDate} Type: {type} Formateur: {formateur}");
                                }
                            }
                            else
                            {
                                DateTime date = DateTimeOffset.FromUnixTimeSeconds(unixdate).Date;
                                Methodes.PrintConsole(Config.sourceApplicationController, fullname + " bénéfice d'une formation le " + date.ToString("dd/MM/yyyy"));
                                Methodes.PrintConsole(Config.sourceApplicationController, "Type: " + (!string.IsNullOrWhiteSpace(training.Type) ? training.Type : "Non spécifiée"));
                                Methodes.PrintConsole(Config.sourceApplicationController, "Formateur: " + (!string.IsNullOrWhiteSpace(training.Trainer) ? training.Trainer : "Non spécifiée"));
                                Methodes.PrintConsole(Config.sourceApplicationController, "Adresse: " + training.Address + ".\n");
                            }
                        }
                        ShowContinuePrompt();
                        break;
                    }
                case 4:
                    {
                        GetData(out string fullname, out int contractorId, out long unixdate);
                        if (contractorId < 0) return "Erreur, Le nom du membre que vous avez entré est incorrect, Veuillez réssayer s'il vous plaît...";

                        Mission mission = new(contractorId, unixdate);
                        if (mission.IsNull)
                        {
                            Methodes.PrintConsole(Config.sourceApplicationController, fullname + " n'a aucune mission enregistré" + ((unixdate > 0) ? " ce jour là." : ".") + "\n");
                        }
                        else
                        {
                            if (unixdate < 0)
                            {
                                goto printPrompt;
                                retryprompt: Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, Votre choix doit être entre 1 et 30, Veuillez réssayer s'il vous plaît...");
                                printPrompt: string response = Methodes.ReadUserInput("Enter le total des dernières missions que vous voulez voir (1-30): ") ?? string.Empty;
                                bool prasedInput = int.TryParse(response, out int missionCount);
                                if (missionCount > 30 || missionCount < 1) goto retryprompt;

                                Methodes.PrintConsole(Config.sourceApplicationController, $"Liste des dernière {missionCount} missions pour {fullname}:");

                                int maxToShow = Math.Min(missionCount, Math.Min(30, mission.ListMission.Count));
                                for (int i = 0; i < maxToShow; i++)
                                {
                                    List<QueryResultRow> row = mission.ListMission;
                                    string description = !string.IsNullOrWhiteSpace(row[i]["description"]) ? row[i]["description"] : "Non spécifiée";
                                    string formattedDate = !string.IsNullOrWhiteSpace(row[i]["date"]) ? row[i]["date"] : "-1";

                                    bool parsedDate = long.TryParse(formattedDate, out long unixTimestamp);
                                    if (parsedDate)
                                    {
                                        DateTime date = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).DateTime.Date;
                                        formattedDate = date.ToString("dd/MM/yyyy");
                                    }
                                    else formattedDate = "Date malformé";

                                    Methodes.PrintConsole(Config.sourceApplicationController, $"- Date: {formattedDate} Description: {description}");
                                }
                            }
                            else
                            {
                                DateTime date = DateTimeOffset.FromUnixTimeSeconds(unixdate).Date;
                                Methodes.PrintConsole(Config.sourceApplicationController, "La mission de " + fullname + " le " + date.ToString("dd/MM/yyyy"));
                                Methodes.PrintConsole(Config.sourceApplicationController, "Description: " + mission.Description + ".\n");
                            }
                        }
                        ShowContinuePrompt();
                        break;
                    }
                case 5:
                    {
                        GetData(out string fullname, out int contractorId, out long unixdate);
                        if (contractorId < 0) return "Erreur, Le nom du membre que vous avez entré est incorrect, Veuillez réssayer s'il vous plaît...";

                        WorkTravel workTravel = new(contractorId, unixdate);
                        if (workTravel.IsNull)
                        {
                            Methodes.PrintConsole(Config.sourceApplicationController, fullname + " n'a aucun déplacement enregistré" + ((unixdate > 0) ? " ce jour là." : ".") + "\n");
                        }
                        else
                        {
                            if (unixdate < 0)
                            {
                                goto printPrompt;
                            retryprompt: Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, Votre choix doit être entre 1 et 30, Veuillez réssayer s'il vous plaît...");
                            printPrompt: string response = Methodes.ReadUserInput("Entrer le total des déplacements que vous voulez voir (1-30): ") ?? string.Empty;
                                bool prasedInput = int.TryParse(response, out int workTravelCount);
                                if (workTravelCount > 30 || workTravelCount < 1) goto retryprompt;

                                Methodes.PrintConsole(Config.sourceApplicationController, $"Liste des dernière {workTravelCount} déplacements pour {fullname}:");

                                int maxToShow = Math.Min(workTravelCount, Math.Min(30, workTravel.ListWorkTravel.Count));
                                for (int i = 0; i < maxToShow; i++)
                                {
                                    List<QueryResultRow> row = workTravel.ListWorkTravel;
                                    string address = !string.IsNullOrWhiteSpace(row[i]["address"]) ? row[i]["address"] : "Non spécifiée";
                                    string sformattedDate = !string.IsNullOrWhiteSpace(row[i]["startDate"]) ? row[i]["startDate"] : "-1";
                                    string eformattedDate = !string.IsNullOrWhiteSpace(row[i]["endDate"]) ? row[i]["endDate"] : "-1";

                                    bool sparsedDate = long.TryParse(sformattedDate, out long sunixTimestamp);
                                    bool eparsedDate = long.TryParse(eformattedDate, out long eunixTimestamp);
                                    if (sparsedDate && eparsedDate)
                                    {
                                        DateTime sdate = DateTimeOffset.FromUnixTimeSeconds(sunixTimestamp).DateTime.Date;
                                        DateTime edate = DateTimeOffset.FromUnixTimeSeconds(eunixTimestamp).DateTime.Date;
                                        sformattedDate = sdate.ToString("dd/MM/yyyy");
                                        eformattedDate = edate.ToString("dd/MM/yyyy");
                                    }
                                    else
                                    {
                                        eformattedDate = "Date malformé";
                                        sformattedDate = "Date malformé";
                                    }

                                    Methodes.PrintConsole(Config.sourceApplicationController, $"- Date de début: {sformattedDate} Date de fin: {eformattedDate} Address : {address}");
                                }
                            }
                            else
                            {
                                DateTime date = DateTimeOffset.FromUnixTimeSeconds(unixdate).Date;
                                Methodes.PrintConsole(Config.sourceApplicationController, fullname + " était en déplacement le " + date.ToString("dd/MM/yyyy"));
                                Methodes.PrintConsole(Config.sourceApplicationController, "Date de début: " + (!string.IsNullOrWhiteSpace(workTravel.StartDate) ? workTravel.StartDate : "Non spécifiée") + ".\n");
                                Methodes.PrintConsole(Config.sourceApplicationController, "Date de fin: " + (!string.IsNullOrWhiteSpace(workTravel.EndDate) ? workTravel.EndDate : "Non spécifiée") + ".\n");
                                Methodes.PrintConsole(Config.sourceApplicationController, "Adresse: " + (!string.IsNullOrWhiteSpace(workTravel.Address) ? workTravel.Address : "Non spécifiée") + ".\n");
                                Methodes.PrintConsole(Config.sourceApplicationController, "Description: " + (!string.IsNullOrWhiteSpace(workTravel.Description) ? workTravel.Description : "Non spécifiée") + ".\n");
                            }
                        }
                        ShowContinuePrompt();
                        break;
                    }
                case 6:
                    {
                        int codePINAttempts = 0;
                        retryCodePIN: string adminPanelPIN = Methodes.ReadUserInput("Enter le code PIN requis pour accéder à cette option: ", true) ?? string.Empty;
                        codePINAttempts += 1;
                        _ = int.TryParse(adminPanelPIN, out int ParsedPIN);
                        if (Config.adminSettingsPIN == ParsedPIN) {
                            AdministrationPanel();
                            break;
                        }
                        if (codePINAttempts > 2) {
                            ShowContinuePrompt();
                            break;
                        }
                        Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, le code pin est incorrect, Veuillez réssayer s'il vous plaît...");
                        goto retryCodePIN;
                        break;
                    }
                    case 7:
                    {
                        string response = Methodes.ReadUserInput("Est-ce que vous voulez êtes sûr que vous voulez fermer l'application? (OUI/NON): ") ?? string.Empty;
                        if (!string.IsNullOrWhiteSpace(response) && response.Equals("oui", StringComparison.OrdinalIgnoreCase)) Environment.Exit(0);
                        break;
                    }
                    case 8:
                    {
                        if (Config.productionRun) return "Erreur, Votre choix doit être entre 1 et 7, Veuillez réssayer s'il vous plaît...";
                        if (Config.productionRun)
                        {
                            Console.Clear();
                            Methodes.PrintConsole(Config.sourceApplicationController, "\n--- Debug mode - Gestionnaire du personnel v1.0");
                            Methodes.PrintConsole(Config.sourceApplicationController, $"- MySQL Server IP: {Config.mysqlServer}:{Config.mysqlPort}");
                        }
                        Methodes.PrintConsole(Config.sourceApplicationController, "1. Remplir la base de donnée avec un script de test");
                        Methodes.PrintConsole(Config.sourceApplicationController, "2. Vider les tables de la base de donnée (sauf jobs et users)");
                        Methodes.PrintConsole(Config.sourceApplicationController, "3. Générer les salaires dans la table 'payments'");
                        Methodes.PrintConsole(Config.sourceApplicationController, "4. Télécharger tout les fiche de paie généré ce mois là (de la table 'payments')");
                        Methodes.PrintConsole(Config.sourceApplicationController, "X. Revenir au menu principal");
                        string response = Methodes.ReadUserInput("Votre choix (1-X): ") ?? string.Empty;
                        bool prasedInput = int.TryParse(response, out int serviceNumber);
                        if (prasedInput)
                        {
                            if (serviceNumber == 1) Program.Controller.InsertData(Config.debugScript);
                            else if (serviceNumber == 2)
                            {
                                string query = @"
                                    SET FOREIGN_KEY_CHECKS = 0;
                                    TRUNCATE TABLE Absences;
                                    TRUNCATE TABLE Mission;
                                    TRUNCATE TABLE PaidLeave;
                                    TRUNCATE TABLE Training;
                                    TRUNCATE TABLE WorkTravel;
                                    TRUNCATE TABLE Contracts;
                                    TRUNCATE TABLE Payments;
                                    TRUNCATE TABLE PDF_files;
                                    SET FOREIGN_KEY_CHECKS = 1;";
                                Program.Controller.InsertData(query);
                            }
                            else if (serviceNumber == 3) Program.Controller.InsertData(Config.generatePayslips);
                            else if (serviceNumber == 4) Methodes.GenerateAndZipPayslips();
                            if (serviceNumber > 0 && serviceNumber < 5)
                            {
                                Methodes.PrintConsole(Config.sourceApplicationController, "Fonction exécuté avec success");
                                ShowContinuePrompt();
                                RunService(8);
                            }
                        }
                        break;
                    }
            }
            return "";
        }
        private void GetData(out string contractorName, out int contractorId, out long unixdate)
        {
            contractorName = Methodes.ReadUserInput("Enter le nom et prénom du membre ciblé(e): ") ?? string.Empty;
            string date = Methodes.ReadUserInput("Enter une date précise (facultative) (dd/mm/aaaa): ") ?? string.Empty;

            Methodes.PrintConsole(Config.sourceApplicationController, "Votre demande est en cours de traitement, Veuillez patientez s'il vous plaît...");
            string cleanedName = MyRegex().Replace(contractorName.Trim(), " ");         
            Contracts contractor = new(cleanedName);

            if (contractor.IsNull)
            {
                contractorId = -1;
                contractorName = "";
            }
            else
            {
                contractorId = contractor.ContractorId;
                contractorName = contractor.Fullname;
            }

            if (DateTime.TryParse(date, out DateTime dateTime))
            {
                DateTimeOffset dto = new(dateTime.ToUniversalTime());
                unixdate = dto.AddDays(1).ToUnixTimeSeconds();
            }
            else
            {
                unixdate = -1;
            }
        }
        private void AdministrationPanel()
        {
            string ErrorMessage = "";
            while (true)
            {
                if (Config.productionRun)
                {
                    Console.Clear();
                    Methodes.PrintConsole(Config.sourceApplicationController, "\n--- Administration Général");
                    Methodes.PrintConsole(Config.sourceApplicationController, "S'il vous plaît, Entrer le numéro du service que vous voulez:\n");
                }
                Methodes.PrintConsole(Config.sourceApplicationController, "1. Gérer les fonctions et leur barèmes");
                Methodes.PrintConsole(Config.sourceApplicationController, "2. Gérer les membres de personnel");
                Methodes.PrintConsole(Config.sourceApplicationController, "3. Revenir au menu principal");
                if (ErrorMessage != "") Methodes.PrintConsole(Config.sourceApplicationController, ErrorMessage); ErrorMessage = "";
                string serviceText = Methodes.ReadUserInput("Votre choix (1-3): ") ?? string.Empty;
                bool prasedInput = int.TryParse(serviceText, out int serviceNumber);

                if (prasedInput)
                {
                    if (serviceNumber < 3 && serviceNumber > 0) RunAdminService(serviceNumber);
                    else if (serviceNumber == 3) break;
                    else ErrorMessage = "Erreur, Votre choix doit être entre 1 et 3, Veuillez réssayer s'il vous plaît...";
                }
            }
        }
        private string RunAdminService(int service)
        {
            Console.WriteLine("\n");
            switch (service)
            {
                case 1:
                    {
                        retryJobsManagement:
                        if (Config.productionRun)
                        {
                            Console.Clear();
                            Methodes.PrintConsole(Config.sourceApplicationController, "\n--- Gestion des fonctions");
                            Methodes.PrintConsole(Config.sourceApplicationController, "S'il vous plaît, Entrer le numéro du service que vous voulez:\n");
                        }
                        Methodes.PrintConsole(Config.sourceApplicationController, "1. Voir les fonctions actuelles et leur barrèmes");
                        Methodes.PrintConsole(Config.sourceApplicationController, "2. Ajouter une fonction");
                        Methodes.PrintConsole(Config.sourceApplicationController, "3. Gérer une fonction");
                        Methodes.PrintConsole(Config.sourceApplicationController, "4. Revenir au menu précédent");
                        string serviceText = Methodes.ReadUserInput("Votre choix (1-4): ") ?? string.Empty;
                        bool prasedInput = int.TryParse(serviceText, out int serviceNumber);
                        if (prasedInput)
                        {
                            if (serviceNumber < 4 && serviceNumber > 0) RunAdminService(serviceNumber + 2);
                            else if (serviceNumber == 4) break;
                            else goto retryJobsManagement;
                        }
                        break;
                    }
                case 2:
                    {
                        retryStaffManagement:
                        if (Config.productionRun)
                        {
                            Console.Clear();
                            Methodes.PrintConsole(Config.sourceApplicationController, "\n--- Gestion des membres");
                            Methodes.PrintConsole(Config.sourceApplicationController, "S'il vous plaît, Entrer le numéro du service que vous voulez:\n");
                        }
                        Methodes.PrintConsole(Config.sourceApplicationController, "1. Ajouter un nouveau membre du personnel");
                        Methodes.PrintConsole(Config.sourceApplicationController, "2. Gérer le contrat d'un membre existant");
                        Methodes.PrintConsole(Config.sourceApplicationController, "3. Revenir au menu précédent");
                        string serviceText = Methodes.ReadUserInput("Votre choix (1-3): ") ?? string.Empty;
                        bool prasedInput = int.TryParse(serviceText, out int serviceNumber);
                        if (prasedInput)
                        {
                            if (serviceNumber < 3 && serviceNumber > 0) RunAdminService(serviceNumber + 5);
                            else if (serviceNumber == 3) break;
                            else goto retryStaffManagement;
                        }
                        break;
                    }
                case 3:
                    {
                        Jobs job = new();
                        Methodes.PrintConsole(Config.sourceApplicationController, $"\nListe des dernière fonctions et leur barrèmes:");

                        for (int i = 0; i < job.ListFonctions.Count; i++)
                        {
                            List<QueryResultRow> row = job.ListFonctions;
                            string name = !string.IsNullOrWhiteSpace(row[i]["name"]) ? row[i]["name"] : "Non spécifiée";
                            bool parsedAuthorityLevel = int.TryParse(row[i]["authorityLevel"], out int authorityLevel);
                            if (parsedAuthorityLevel)
                            {
                                Methodes.PrintConsole(Config.sourceApplicationController, $"- Titre: {name} Barrème: {authorityLevel}");
                            }
                            else Methodes.PrintConsole(Config.sourceApplicationController, $"- Titre: {name} Barrème: Non spécifiée");
                        }
                        ShowContinuePrompt();
                        RunAdminService(1);
                        break;
                    }
                case 4:
                    {
                        Jobs job = new();
                    retryJobName: string jobName = Methodes.ReadUserInput("Entrer un titre pour la nouvelle fonction: ") ?? string.Empty;

                        if (string.IsNullOrWhiteSpace(jobName) || jobName.Length < 5)
                        {
                            Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, Le titre doit avoir au moins 5 caractères et ne doit pas être vide.");
                            goto retryJobName;
                        }

                    retryAuthorityLevel: string authorityLevelString = Methodes.ReadUserInput("Enter le barrème de cette fonction (1-inf): ") ?? string.Empty;
                        _ = int.TryParse(authorityLevelString, out int authorityLevel);

                        if (authorityLevel < 1)
                        {
                            Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, Le barrème d'une fonction doit être supérieure à 1.");
                            goto retryAuthorityLevel;
                        }

                        bool dataInserted = job.InsertNewJob(jobName, authorityLevel);
                        if (dataInserted) Methodes.PrintConsole(Config.sourceApplicationController, $"Votre demande a été enregistré avec success.");
                        else Methodes.PrintConsole(Config.sourceApplicationController, $"Un erreur s'est produite, Veuillez réessayer s'il vous plaît...");

                        ShowContinuePrompt();
                        RunAdminService(1);
                        break;
                    }
                case 5:
                    {
                    retryJobName: string jobName = Methodes.ReadUserInput("Entrer le titre de la fonction que vous souhaiter gérer: ") ?? string.Empty;

                        if (string.IsNullOrWhiteSpace(jobName) || jobName.Length < 5)
                        {
                            Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, Le titre doit avoir au moins 5 caractères et ne doit pas être vide.");
                            goto retryJobName;
                        }

                        Jobs job = new(jobName);

                        if (job.IsNull)
                        {
                            Methodes.PrintConsole(Config.sourceApplicationController, "Il ne existe aucune fonction avec le titre que vous avez fournis.");
                            ShowContinuePrompt();
                            RunAdminService(1);
                            break;
                        }
                        else
                        {
                        retrySelectOption:
                            if (Config.productionRun)
                            {
                                Console.Clear();
                                Methodes.PrintConsole(Config.sourceApplicationController, "\n--- Gestion des fonctions");
                                Methodes.PrintConsole(Config.sourceApplicationController, "S'il vous plaît, Entrer le numéro du service que vous voulez:\n");
                            }
                            Methodes.PrintConsole(Config.sourceApplicationController, $"1. Modifier la fonction {job.Name}");
                            Methodes.PrintConsole(Config.sourceApplicationController, $"2. Supprimer la fonction {job.Name}");
                            Methodes.PrintConsole(Config.sourceApplicationController, "3. Revenir au menu précédent");
                            string selectOption = Methodes.ReadUserInput("Votre choix (1-3):") ?? string.Empty;
                            _ = int.TryParse(selectOption, out int selectedOption);

                            if (selectedOption == 1)
                            {
                                Methodes.PrintConsole(Config.sourceApplicationController, $"Modification de la fonction {job.Name}.");
                                Methodes.PrintConsole(Config.sourceApplicationController, "Veuillez entrer uniquement les informations des chemins que vous voulez modifier.");
                                string nameJob = Methodes.ReadUserInput($"Enter un nouveau titre pour cette fonction (actuel: {job.Name}): ") ?? string.Empty;
                                if (string.IsNullOrWhiteSpace(nameJob)) nameJob = "0";

                                string authoritylevelString = Methodes.ReadUserInput($"Enter le nouveau barrème pour cette fonction (actuel: {job.AuthorityLevel}): ") ?? string.Empty;
                                if (string.IsNullOrWhiteSpace(authoritylevelString)) authoritylevelString = "0";

                                bool isAuthorityLevelValid = int.TryParse(authoritylevelString, out int authorityLevel);
                                if (!isAuthorityLevelValid || authorityLevel < 1) authorityLevel = 0;

                                bool dataInserted = job.ModifyJob(nameJob, authorityLevel);
                                Methodes.PrintConsole(Config.sourceApplicationController, $"La fonction {job.Name} a été modifier avec success");
                            }
                            else if (selectedOption == 2)
                            {
                                string response = Methodes.ReadUserInput($"Est-ce que vous êtes sûr que vous voulez supprimer la fonction ({job.Name}) ? (OUI/NON): ") ?? string.Empty;
                                if (!string.IsNullOrWhiteSpace(response) && response.Equals("oui", StringComparison.OrdinalIgnoreCase))
                                {
                                    bool dataInserted = job.DeleteJob();
                                    if (dataInserted) Methodes.PrintConsole(Config.sourceApplicationController, $"La fonction {job.Name} a été supprimé avec success.");
                                    else Methodes.PrintConsole(Config.sourceApplicationController, $"Un erreur s'est produite, Veuillez réessayer s'il vous plaît...");
                                }
                            }
                            else if (selectedOption == 3) break;
                            else
                            {
                                Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, Votre choix doit être entre 1 et 3, Veuillez réssayer s'il vous plaît...");
                                goto retrySelectOption;
                            }

                            ShowContinuePrompt();
                            RunAdminService(1);
                            break;
                        }
                    }
                case 6:
                    {
                        Contracts contract = new();

                    retryCrewMemberName: string crewMemberName = Methodes.ReadUserInput("Entrer le nom et prénom du nouveau membre: ") ?? string.Empty;

                        if (string.IsNullOrWhiteSpace(crewMemberName) || crewMemberName.Length < 8)
                        {
                            Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, Le nom complète doit avoir au moins 8 caractères et ne doit pas être vide.");
                            goto retryCrewMemberName;
                        }

                    retryCrewMemberGSM: string crewMemberGSM = Methodes.ReadUserInput( "Entrer le numéro GSM du nouveau membre: ") ?? string.Empty;

                        if (string.IsNullOrWhiteSpace(crewMemberGSM) || crewMemberGSM.Length < 12 || !Methodes.IsNumeric(crewMemberGSM))
                        {
                            Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, Le numéro GSM doit être composée d'au moins 12 chiffres et ne doit pas être vide.");
                            goto retryCrewMemberGSM;
                        }

                    retryCrewMemberEmail: string crewMemberEmail = Methodes.ReadUserInput("Entrer l'adresse mail du nouveau membre: ") ?? string.Empty;

                        if (string.IsNullOrWhiteSpace(crewMemberEmail) || crewMemberEmail.Length < 12 || !Methodes.IsEmail(crewMemberEmail))
                        {
                            Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, L'adresse doit être composée d'au moins 12 caractères et ne doit pas être vide.");
                            goto retryCrewMemberEmail;
                        }

                    retryCrewMemberAddress: string crewMemberAddress = Methodes.ReadUserInput("Entrer l'adresse mail du nouveau membre: ") ?? string.Empty;

                        if (string.IsNullOrWhiteSpace(crewMemberAddress) || crewMemberAddress.Length < 15 || !Methodes.IsEmail(crewMemberAddress))
                        {
                            Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, L'adresse doit être composée d'au moins 15 caractères et ne doit pas être vide.");
                            goto retryCrewMemberAddress;
                        }

                    retryEndContractDate: string crewMemberEndDate = Methodes.ReadUserInput("Enter la date du fin de contrat (Laisser vide si durée indéterminée): ") ?? string.Empty;
                        long crewMemberUnixEndDate = 0;
                        if (!string.IsNullOrWhiteSpace(crewMemberEndDate))
                        {
                            if (DateTime.TryParse(crewMemberEndDate, out var d)) crewMemberUnixEndDate = ((DateTimeOffset)d).ToUnixTimeSeconds();
                            else
                            {
                                Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, La date doit être valide et ne doit pas être vide.");
                                goto retryEndContractDate;
                            }
                        }


                    retryCrewMemberHours: string crewMemberHours = Methodes.ReadUserInput("Entrer le nombre des heures du travail par semaine du nouveau membre: ") ?? string.Empty;
                        if (string.IsNullOrWhiteSpace(crewMemberHours) || !Methodes.IsNumeric(crewMemberHours))
                        {
                            Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, Le nombre des heures du travail doit être supérieure à 0 et ne doit pas être vide.");
                            goto retryCrewMemberHours;
                        }

                    retryCrewMemberSalary: string crewMemberSalary = Methodes.ReadUserInput("Entrer le salaire du nouveau membre: ") ?? string.Empty;
                        if (string.IsNullOrWhiteSpace(crewMemberSalary) || !Methodes.IsNumeric(crewMemberSalary))
                        {
                            Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, Le salaire doit être ne doit pas être vide.");
                            goto retryCrewMemberSalary;
                        }

                    retryCrewMemberJob: string crewMemberJob = Methodes.ReadUserInput("Entrer la position du nouveau membre: ") ?? string.Empty;
                        if (string.IsNullOrWhiteSpace(crewMemberJob))
                        {
                            Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, La position ne doit pas être vide.");
                            goto retryCrewMemberJob;
                        }

                        Jobs job = new(crewMemberJob);
                        if (job.IsNull)
                            if (string.IsNullOrWhiteSpace(crewMemberJob))
                            {
                                Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, Cette position n'existe pas dans le système.");
                                goto retryCrewMemberJob;
                            }

                        var parameters = new Dictionary<string, object> { };
                        parameters["@fullName"] = MyRegex().Replace(crewMemberName.Trim(), " ");
                        parameters["@gsm"] = crewMemberGSM;
                        parameters["@email"] = crewMemberEmail;
                        parameters["@address"] = crewMemberAddress;
                        parameters["@endDate"] = crewMemberUnixEndDate;
                        parameters["@hours"] = crewMemberHours;
                        parameters["@salary"] = crewMemberSalary;
                        parameters["@job"] = job.Name;

                        bool dataInserted = contract.InsertContract(parameters);
                        if (dataInserted) Methodes.PrintConsole(Config.sourceApplicationController, $"Votre demande a été enregistré avec success.");
                        else Methodes.PrintConsole(Config.sourceApplicationController, $"Un erreur s'est produite, Veuillez réessayer s'il vous plaît...");

                        ShowContinuePrompt();
                        RunAdminService(2);
                        break;
                    }
                case 7:
                    {
                        retryCrewMemberName: string crewMemberName = Methodes.ReadUserInput("Entrer le nom complète du membre que vous souhaiter gérer: ") ?? string.Empty;
                        if (string.IsNullOrWhiteSpace(crewMemberName) || crewMemberName.Length < 8)
                        {
                            Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, Le nom complète doit avoir au moins 8 caractères et ne doit pas être vide.");
                            goto retryCrewMemberName;
                        }
                        string cleanedName = MyRegex().Replace(crewMemberName.Trim(), " "); 
                        Contracts contract = new(cleanedName);

                        if (contract.IsNull)
                        {
                            Methodes.PrintConsole(Config.sourceApplicationController, $"Il ne existe aucune contract actif pour {crewMemberName}.");
                            ShowContinuePrompt();
                            RunAdminService(2);
                            break;
                        }
                        else
                        {
                        retrySelectOption:
                        if (Config.productionRun)
                        {
                            Console.Clear();
                            Methodes.PrintConsole(Config.sourceApplicationController, $"\n--- Gestion du contrat de {contract.Fullname}");
                            Methodes.PrintConsole(Config.sourceApplicationController, "S'il vous plaît, Entrer le numéro du service que vous voulez:\n");
                        }
                            Methodes.PrintConsole(Config.sourceApplicationController, "1. Mettre en fin le contrat.");
                            Methodes.PrintConsole(Config.sourceApplicationController, "2. Déclarer un absence");
                            Methodes.PrintConsole(Config.sourceApplicationController, "3. Déposer un justificative d'absence");
                            Methodes.PrintConsole(Config.sourceApplicationController, "4. Autoriser une formation");
                            Methodes.PrintConsole(Config.sourceApplicationController, "5. Autoriser un congé payé");
                            Methodes.PrintConsole(Config.sourceApplicationController, "6. Autoriser un déplacement");
                            Methodes.PrintConsole(Config.sourceApplicationController, "7. Assigner une mission");
                            Methodes.PrintConsole(Config.sourceApplicationController, "8. Télécharger son fiche de paie");
                            Methodes.PrintConsole(Config.sourceApplicationController, "9. Revenir au menu précédent");
                            string selectOption = Methodes.ReadUserInput("Votre choix (1-9): ") ?? string.Empty;
                            _ = int.TryParse(selectOption, out int selectedOption);

                            if (selectedOption == 1)
                            {
                                string response = Methodes.ReadUserInput($"Est-ce que vous êtes sûr que vous voulez mettre en fin le contrat de ({contract.Fullname}) ? (OUI/NON): ") ?? string.Empty;
                                if (!string.IsNullOrWhiteSpace(response) && response.Equals("oui", StringComparison.OrdinalIgnoreCase))
                                {
                                    bool dataInserted = contract.EndContract();
                                    if (dataInserted) Methodes.PrintConsole(Config.sourceApplicationController, $"Le contrat de {contract.Fullname} a été mise en fin avec success");
                                    else Methodes.PrintConsole(Config.sourceApplicationController, $"Un erreur s'est produite, Veuillez réessayer s'il vous plaît...");
                                }
                                ShowContinuePrompt();
                                RunAdminService(2);
                                break;
                            }
                            else if (selectedOption == 2)
                            {
                            retryAbsenceDate: string crewMemberAbsenceDate = Methodes.ReadUserInput("Enter la date d'absence: ") ?? string.Empty;
                                long crewMemberUnixDate = 0;

                                if (!string.IsNullOrWhiteSpace(crewMemberAbsenceDate))
                                {
                                    if (DateTime.TryParse(crewMemberAbsenceDate, out var d))
                                    {
                                        if (d > DateTime.Now)
                                        {
                                            Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, la date ne doit pas être dans le futur.");
                                            goto retryAbsenceDate;
                                        }
                                        crewMemberUnixDate = ((DateTimeOffset)d).ToUnixTimeSeconds();
                                    }
                                    else
                                    {
                                        Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, La date doit être valide et ne doit pas être vide.");
                                        goto retryAbsenceDate;
                                    }
                                }

                                Absence absent = new(contract.ContractorId);
                                bool dataInserted = absent.DeclareAbsence(contract.ContractorId, crewMemberUnixDate);
                                if (dataInserted) Methodes.PrintConsole(Config.sourceApplicationController, $"Le membre {contract.Fullname} a été déclaré absent le {crewMemberAbsenceDate}.");
                                else Methodes.PrintConsole(Config.sourceApplicationController, $"Un erreur s'est produite, Veuillez réessayer s'il vous plaît...");
                                ShowContinuePrompt();
                                RunAdminService(2);
                                break;
                            }
                            else if (selectedOption == 3)
                            {
                            retryAbsenceDate: string crewMemberAbsenceDate = Methodes.ReadUserInput("Enter la date d'absence: ") ?? string.Empty;
                                long crewMemberUnixDate = 0;

                                if (!string.IsNullOrWhiteSpace(crewMemberAbsenceDate))
                                {
                                    if (DateTime.TryParse(crewMemberAbsenceDate, out DateTime dateTime))
                                    {
                                        DateTimeOffset dto = new(dateTime.ToUniversalTime());
                                        crewMemberUnixDate = dto.AddDays(1).ToUnixTimeSeconds();
                                        if (dateTime > DateTime.Now)
                                        {
                                            Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, la date ne doit pas être dans le futur.");
                                            goto retryAbsenceDate;
                                        }
                                    }
                                    else
                                    {
                                        Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, La date doit être valide et ne doit pas être vide.");
                                        goto retryAbsenceDate;
                                    }
                                }

                                Absence absent = new(contract.ContractorId, crewMemberUnixDate);
                                if (!absent.IsNull)
                                {
                                    Methodes.PrintConsole(Config.sourceApplicationController, "Faites glisser et déposez un fichier PDF dans cette fenêtre et appuyez sur Entrée: ");
                                    string inputPath = Console.ReadLine()?.Trim('\'', '"') ?? "";
                                    if (string.IsNullOrWhiteSpace(inputPath) || !File.Exists(inputPath) || !inputPath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                                    {
                                        Methodes.PrintConsole(Config.sourceApplicationController, $"Un erreur s'est produite, Veuillez réessayer s'il vous plaît...");
                                        ShowContinuePrompt();
                                        RunAdminService(2);
                                        break;
                                    }
                                    string fileName = $"justificative_absence_" + contract.ContractorId + "_" + absent.DateOfAbsence + ".pdf";
                                    byte[] fileData = File.ReadAllBytes(inputPath);
                                    string base64String = Convert.ToBase64String(fileData);
                                    try
                                    {
                                        Methodes.UploadJustificative(fileName, base64String);
                                        absent.DeclareJustificative(contract.ContractorId, fileName);
                                        Methodes.PrintConsole(Config.sourceApplicationController, "Le justificative a été bien téléchargé et encodé dans le système.");
                                    }
                                    catch (Exception ex)
                                    {
                                        Methodes.PrintConsole(Config.sourceApplicationController, ex.ToString(), true);
                                    }
                                }
                                else
                                {
                                    Methodes.PrintConsole(Config.sourceApplicationController, "Il ne existe aucun absence enregistrée pour le date que vous avez fournis.");
                                }
                                ShowContinuePrompt();
                                RunAdminService(2);
                                break;
                            }
                            else if (selectedOption == 4)
                            {
                                Training training = new(contract.ContractorId);
                                int currentYear = DateTime.UtcNow.Year;
                                int count = 0, totalcount = 0;

                                List<QueryResultRow> row = training.ListTraining;
                                for (int i = 0; i < row.Count; i++)
                                {
                                    _ = long.TryParse(row[i]["date"], out long unixdate);
                                    DateTime date = DateTimeOffset.FromUnixTimeSeconds(unixdate).UtcDateTime;
                                    if (date.Year == currentYear)
                                    {
                                        count++;
                                    }
                                    totalcount++;
                                }
                                if ((count > 3 && contract.Job == "Ouvrier") || (count > 2 && contract.Job == "Employé"))
                                {
                                    Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, Ce membre a déjà atteint le nombre maximal des formations par an.");
                                    ShowContinuePrompt();
                                    RunAdminService(2);
                                    break;
                                }

                            retryTrainingDate: string crewMemberTrainingDate = Methodes.ReadUserInput("Enter la date du formation: ") ?? string.Empty;
                                long crewMemberUnixDate = 0;

                                if (!string.IsNullOrWhiteSpace(crewMemberTrainingDate))
                                {
                                    if (DateTime.TryParse(crewMemberTrainingDate, out var d))
                                    {
                                        if (d < DateTime.Now)
                                        {
                                            Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, la date doit être dans le futur.");
                                            goto retryTrainingDate;
                                        }
                                        crewMemberUnixDate = ((DateTimeOffset)d).ToUnixTimeSeconds();
                                    }
                                    else
                                    {
                                        Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, La date doit être valide et ne doit pas être vide.");
                                        goto retryTrainingDate;
                                    }
                                }

                            retryCrewMemberFormateur: string crewMemberFormateur = Methodes.ReadUserInput("Entrer le nom de l'entreprise/formateur: ") ?? string.Empty;

                                if (string.IsNullOrWhiteSpace(crewMemberFormateur) || crewMemberFormateur.Length < 6)
                                {
                                    Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, L'adresse doit être composée d'au moins 6 caractères et ne doit pas être vide.");
                                    goto retryCrewMemberFormateur;
                                }

                            retryCrewMemberTypeTraining: string crewMemberTypeTraining = Methodes.ReadUserInput("Entrer le nom de le domaine du formation: ") ?? string.Empty;

                                if (string.IsNullOrWhiteSpace(crewMemberTypeTraining) || crewMemberTypeTraining.Length < 6)
                                {
                                    Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, Le domaine du formation doit être composée d'au moins 6 caractères et ne doit pas être vide.");
                                    goto retryCrewMemberTypeTraining;
                                }

                            retryCrewMemberAddress: string crewMemberAddress = Methodes.ReadUserInput("Entrer l'adresse du formateur: ") ?? string.Empty;

                                if (string.IsNullOrWhiteSpace(crewMemberAddress) || crewMemberAddress.Length < 15)
                                {
                                    Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, L'adresse doit être composée d'au moins 15 caractères et ne doit pas être vide.");
                                    goto retryCrewMemberAddress;
                                }

                                var parameters = new Dictionary<string, object>
                                {
                                    { "@contractorId", contract.ContractorId },
                                    { "@type", crewMemberTypeTraining },
                                    { "@address", crewMemberAddress },
                                    { "@formateur", crewMemberFormateur },
                                    { "@date", crewMemberUnixDate }
                                };

                                bool dataInserted = training.AuthorizeTraining(parameters);
                                if (dataInserted) Methodes.PrintConsole(Config.sourceApplicationController, $"Votre demande a été enregistré avec success.");
                                else Methodes.PrintConsole(Config.sourceApplicationController, $"Un erreur s'est produite, Veuillez réessayer s'il vous plaît...");

                                if (totalcount == 9 && contract.Job == "Ouvrier")
                                {
                                    Methodes.PrintConsole(Config.sourceApplicationController, "Remarque! Ce ouvrier va bénificer d'une 5% d'augmantation du salaire dès le prochain virement.");
                                    contract.UpdateSalary(contract.Salary + (contract.Salary * 0.05));
                                }
                                else if (totalcount == 4 && contract.Job == "Employé")
                                {
                                    Methodes.PrintConsole(Config.sourceApplicationController, "Remarque! Ce employé peut postuler dès maintenant à une meilleur fonction.");
                                }
                                ShowContinuePrompt();
                                RunAdminService(2);
                                break;
                            }
                            else if (selectedOption == 5)
                            {
                                PaidLeave paidLeave = new(contract.ContractorId);
                                int currentYear = DateTime.UtcNow.Year;
                                int count = 0;


                                List<QueryResultRow> row = paidLeave.ListPaidLeave;
                                for (int i = 0; i < row.Count; i++)
                                {
                                    _ = long.TryParse(row[i]["startDate"], out long unixsdate);
                                    _ = long.TryParse(row[i]["endDate"], out long unixedate);
                                    DateTime sdate = DateTimeOffset.FromUnixTimeSeconds(unixsdate).UtcDateTime;
                                    DateTime edate = DateTimeOffset.FromUnixTimeSeconds(unixedate).UtcDateTime;
                                    TimeSpan span = edate - sdate;

                                    if (sdate.Year == currentYear)
                                    {
                                        count += span.Days;
                                    }
                                }

                                int totalbonus = 0;
                                if (contract.Job == "Employé")
                                {
                                    long secondsTotal = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - contract.StartDate;
                                    double yearsTotal = secondsTotal / (365 * 24 * 60 * 60);
                                    totalbonus = (int)(yearsTotal / 3);
                                }

                                if ((count > 19 && contract.Job == "Ouvrier") || (count > (19 + totalbonus) && contract.Job == "Employé") || (count > 19 && contract.Job == "Consultant"))
                                {
                                    Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, Ce membre a déjà atteint le nombre maximal du congé payé par an.");
                                    ShowContinuePrompt();
                                    RunAdminService(2);
                                    break;
                                }

                            retryPaidLeave: string paidLeaveStartDate = Methodes.ReadUserInput("Enter la date du début de congé: ") ?? string.Empty;
                                string paidLeaveEndDate = Methodes.ReadUserInput("Enter la date du fin de congé: ") ?? string.Empty;
                                long paidLeaveUnixStartDate = 0, paidLeaveUnixEndDate = 0;

                                if (string.IsNullOrWhiteSpace(paidLeaveStartDate) || string.IsNullOrWhiteSpace(paidLeaveEndDate))
                                {
                                    Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, Les deux date doit être valide et ne doit pas être vide.");
                                    goto retryPaidLeave;
                                }
                                if (DateTime.TryParse(paidLeaveStartDate, out var sd) && DateTime.TryParse(paidLeaveEndDate, out var ed))
                                {
                                    if (sd < DateTime.Now || ed < DateTime.Now || ed <= sd)
                                    {
                                        Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, la date du début et du fin doit être dans le futur.");
                                        goto retryPaidLeave;
                                    }
                                    paidLeaveUnixStartDate = ((DateTimeOffset)sd.AddDays(1)).ToUnixTimeSeconds();
                                    paidLeaveUnixEndDate = ((DateTimeOffset)ed.AddDays(1)).ToUnixTimeSeconds();

                                    TimeSpan difference = ed - sd;
                                    int paidLeaveDaysLeft = 20 + totalbonus - count;
                                    if (difference.Days > paidLeaveDaysLeft)
                                    {
                                        Methodes.PrintConsole(Config.sourceApplicationController, $"Erreur, Ce membre ne peut pas prendre un congé de plus que {paidLeaveDaysLeft} jours.");
                                        ShowContinuePrompt();
                                        RunAdminService(2);
                                        break;
                                    }
                                }

                            retryPaidLeaveReason: string paidLeaveReason = Methodes.ReadUserInput("Enter la raison du congé payé: ") ?? string.Empty;
                                if (string.IsNullOrWhiteSpace(paidLeaveReason) || paidLeaveReason.Length < 4)
                                {
                                    Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, La raison du congé doit être composée d'au moins 4 caractères et ne doit pas être vide.");
                                    goto retryPaidLeaveReason;
                                }

                                var parameters = new Dictionary<string, object>
                                {
                                    { "@contractorId", contract.ContractorId },
                                    { "@startDate", paidLeaveUnixStartDate },
                                    { "@endDate", paidLeaveUnixEndDate },
                                    { "@reason", paidLeaveReason }
                                };

                                bool dataInserted = paidLeave.AuthorizePaidLeave(parameters);
                                if (dataInserted) Methodes.PrintConsole(Config.sourceApplicationController, $"Votre demande a été enregistré avec success.");
                                else Methodes.PrintConsole(Config.sourceApplicationController, $"Un erreur s'est produite, Veuillez réessayer s'il vous plaît...");
                                ShowContinuePrompt();
                                RunAdminService(2);
                                break;
                            }
                            else if (selectedOption == 6)
                            {
                            retryWorkTravel: string workTravelStartDate = Methodes.ReadUserInput("Enter la date du début de déplacement: ") ?? string.Empty;
                                string workTravelEndDate = Methodes.ReadUserInput("Enter la date du fin de déplacement: ") ?? string.Empty;
                                long workTravelUnixStartDate = 0, workTravelUnixEndDate = 0;

                                if (string.IsNullOrWhiteSpace(workTravelStartDate) || string.IsNullOrWhiteSpace(workTravelEndDate))
                                {
                                    Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, Les deux date doit être valide et ne doit pas être vide.");
                                    goto retryWorkTravel;
                                }
                                if (DateTime.TryParse(workTravelStartDate, out var sd) && DateTime.TryParse(workTravelEndDate, out var ed))
                                {
                                    if (sd < DateTime.Now || ed < DateTime.Now || ed <= sd)
                                    {
                                        Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, la date du début et du fin doit être dans le futur.");
                                        goto retryWorkTravel;
                                    }
                                    workTravelUnixStartDate = ((DateTimeOffset)sd).ToUnixTimeSeconds();
                                    workTravelUnixEndDate = ((DateTimeOffset)ed).ToUnixTimeSeconds();
                                }

                            retryWorkTravelAddr: string WorkTravelAddr = Methodes.ReadUserInput("Enter l'adresse du mission: ") ?? string.Empty;
                                if (string.IsNullOrWhiteSpace(WorkTravelAddr) || WorkTravelAddr.Length < 15)
                                {
                                    Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, L'adresse doit être composée d'au moins 15 caractères et ne doit pas être vide.");
                                    goto retryWorkTravelAddr;
                                }

                            retryWorkTravelDescription: string WorkTravelDescription = Methodes.ReadUserInput("Enter la description du mission: ") ?? string.Empty;
                                if (string.IsNullOrWhiteSpace(WorkTravelDescription) || WorkTravelDescription.Length < 24)
                                {
                                    Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, La description doit être composée d'au moins 24 caractères et ne doit pas être vide.");
                                    goto retryWorkTravelDescription;
                                }

                                var parameters = new Dictionary<string, object>
                                {
                                    { "@contractorId", contract.ContractorId },
                                    { "@startDate", workTravelUnixStartDate },
                                    { "@endDate", workTravelUnixEndDate },
                                    { "@address", WorkTravelAddr },
                                    { "@description", WorkTravelDescription }
                                };

                                WorkTravel worktravel = new(contract.ContractorId);
                                bool dataInserted = worktravel.AuthorizeWorkTravel(parameters);
                                if (dataInserted) Methodes.PrintConsole(Config.sourceApplicationController, $"Votre demande a été enregistré avec success.");
                                else Methodes.PrintConsole(Config.sourceApplicationController, $"Un erreur s'est produite, Veuillez réessayer s'il vous plaît...");
                                ShowContinuePrompt();
                                RunAdminService(2);
                                break;
                            }
                            else if (selectedOption == 7)
                            {
                            retryMissionDate: string missionStartDate = Methodes.ReadUserInput("Enter la date du mission: ") ?? string.Empty;
                                long missionUnixDate = 0;

                                if (!string.IsNullOrWhiteSpace(missionStartDate))
                                {
                                    if (DateTime.TryParse(missionStartDate, out var d))
                                    {
                                        if (d < DateTime.Now)
                                        {
                                            Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, la date doit être dans le futur.");
                                            goto retryMissionDate;
                                        }
                                        missionUnixDate = ((DateTimeOffset)d).ToUnixTimeSeconds();
                                    }
                                    else
                                    {
                                        Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, La date doit être valide et ne doit pas être vide.");
                                        goto retryMissionDate;
                                    }
                                }

                            retryMissionAddr: string missionDescription = Methodes.ReadUserInput("Entrer la description du mission: ") ?? string.Empty;

                                if (string.IsNullOrWhiteSpace(missionDescription) || missionDescription.Length < 24)
                                {
                                    Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, la description doit être composée d'au moins 24 caractères et ne doit pas être vide.");
                                    goto retryMissionAddr;
                                }

                                var parameters = new Dictionary<string, object>
                                {
                                    { "@contractorId", contract.ContractorId },
                                    { "@description", missionDescription },
                                    { "@date", missionUnixDate }
                                };

                                Mission mission = new(contract.ContractorId);
                                bool dataInserted = mission.AssignMission(parameters);
                                if (dataInserted) Methodes.PrintConsole(Config.sourceApplicationController, $"Votre demande a été enregistré avec success.");
                                else Methodes.PrintConsole(Config.sourceApplicationController, $"Un erreur s'est produite, Veuillez réessayer s'il vous plaît...");
                                ShowContinuePrompt();
                                RunAdminService(2);
                                break;
                            }
                            else if (selectedOption == 8)
                            {
                                Methodes.PrintConsole(Config.sourceApplicationController, $"Votre demande a été enregistré avec success, Veuillez patientez...");
                                Methodes.GenerateAndZipPayslips(contract.ContractorId);
                                ShowContinuePrompt();
                                RunAdminService(2);
                                break;
                            }
                            else if (selectedOption == 9) break;
                            else
                            {
                                Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, Votre choix doit être entre 1 et 9, Veuillez réssayer s'il vous plaît...");
                                ShowContinuePrompt();
                                goto retrySelectOption;
                            }
                            break;
                        }
                    }
            }
            return "";
        }
        private static void ShowContinuePrompt()
        {
            if (!Config.productionRun) return;
            Methodes.PrintConsole(Config.sourceApplicationController, "Appuyez sur n'importe quelle touche pour revenir au menu précédent...\n");
            try
            {
                _ = Console.ReadKey();
            }
            catch (Exception)
            {
                Environment.Exit(0);
            }
        }

        [GeneratedRegex(@"\s+")]
        private static partial Regex MyRegex();
    }
}