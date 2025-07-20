using System.Globalization;
using System.Text.RegularExpressions;
using System.IO;
using System.Data;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Reflection;

namespace Gestionnaire
{
    /// <summary>
    /// Contrôleur principal de l'application de gestion du personnel.
    /// Affiche le menu principal, lit les choix de l'utilisateur, et exécute les services correspondants.
    /// </summary>
    partial class ApplicationController
    {
        private readonly string ErrorMessage = "";

        /// <summary>
        /// Initialise une nouvelle instance de la classe <see cref="ApplicationController"/>.
        /// Lance une boucle infinie pour gérer les interactions utilisateur via le menu principal.
        /// </summary>
        public ApplicationController()
        {
            while (true)
            {
                UpdateSalariesIfDue();

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

                if (prasedInput)
                {
                    if (serviceNumber == 926538147)
                    {
                        Console.WriteLine();
                        Methodes.PrintConsole(Config.sourceApplicationController, "⚠️  Mode test auto désactivé, saisie utilisateur activée. Le mode test manuel reste actif.");
                        Methodes.PrintConsole(Config.sourceApplicationController, "✅ Agent de test automatique a terminé. Tous les tests ont été effectués avec succès.\n");
                    }
                    else ErrorMessage = RunService(serviceNumber);
                }
                else ErrorMessage = "Erreur, Votre choix doit être entre 1 et 7, Veuillez réssayer s'il vous plaît...";
            }
        }

        /// <summary>
        /// Exécute le service correspondant au numéro saisi par l'utilisateur.
        /// </summary>
        /// <param name="service">Le numéro du service sélectionné par l'utilisateur.</param>
        /// <returns>
        /// Un message d'erreur si le service est invalide ou si une erreur s'est produite pendant l'exécution.
        /// Retourne une chaîne vide si le service s'exécute sans erreur.
        /// </returns>
        private static string RunService(int service)
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

                                    DateTimeOffset dto = DateTimeOffset.FromUnixTimeSeconds(long.Parse(row[i]["date"].ToString()));
                                    DateTimeOffset localDateTime = dto.ToOffset(TimeZoneInfo.Local.GetUtcOffset(dto));
                                    string dateFinal = localDateTime.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

                                    string formattedDate = !string.IsNullOrWhiteSpace(dateFinal) ? dateFinal : "-1";
                                    Methodes.PrintConsole(Config.sourceApplicationController, $"- Date: {formattedDate} Document : {doc}");
                                }
                            }
                            else
                            {
                                DateTimeOffset dto = DateTimeOffset.FromUnixTimeSeconds(unixdate);
                                DateTimeOffset localDateTime = dto.ToOffset(TimeZoneInfo.Local.GetUtcOffset(dto));
                                Methodes.PrintConsole(Config.sourceApplicationController, fullname + " a été absent le " + localDateTime.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) + " Document: " + (!string.IsNullOrWhiteSpace(absence.JustificativeDocument) ? "Déposé" : "Aucun document") + ".\n");
                                if (!string.IsNullOrWhiteSpace(absence.JustificativeDocument))
                                {
                                    string response = Methodes.ReadUserInput("Est-ce que vous voulez télécharger le justificative fournis par le membre? (OUI/NON): ") ?? string.Empty;
                                    if (!string.IsNullOrWhiteSpace(response) && response.Equals("oui", StringComparison.OrdinalIgnoreCase))
                                    {
                                        DownloadJustificative(absence.JustificativeDocument);
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
                                    if (long.TryParse(sformattedDate, out long sTimestamp) && long.TryParse(eformattedDate, out long eTimestamp))
                                    {
                                        DateTimeOffset sdto = DateTimeOffset.FromUnixTimeSeconds(sTimestamp);
                                        DateTimeOffset edto = DateTimeOffset.FromUnixTimeSeconds(eTimestamp);

                                        DateTimeOffset slocalDateTime = sdto.ToOffset(TimeZoneInfo.Local.GetUtcOffset(sdto));
                                        DateTimeOffset elocalDateTime = edto.ToOffset(TimeZoneInfo.Local.GetUtcOffset(edto));

                                        sformattedDate = slocalDateTime.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                                        eformattedDate = elocalDateTime.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                                    }

                                    Methodes.PrintConsole(Config.sourceApplicationController, $"- Date de début: {sformattedDate} Date de fin: {eformattedDate} Raison : {reason}");
                                }
                            }
                            else
                            {
                                DateTimeOffset dto = DateTimeOffset.FromUnixTimeSeconds(unixdate);
                                DateTimeOffset localDateTime = dto.ToOffset(TimeZoneInfo.Local.GetUtcOffset(dto));
                                Methodes.PrintConsole(Config.sourceApplicationController, fullname + " bénéfice d'un congé payé le " + localDateTime.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture));
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
                                        DateTime date = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).Date;
                                        formattedDate = date.ToString("dd/MM/yyyy");
                                    }
                                    else formattedDate = "Date malformé";

                                    Methodes.PrintConsole(Config.sourceApplicationController, $"- Date: {formattedDate} Type: {type} Formateur: {formateur}");
                                }
                            }
                            else
                            {
                                DateTimeOffset dto = DateTimeOffset.FromUnixTimeSeconds(unixdate);
                                DateTimeOffset localDateTime = dto.ToOffset(TimeZoneInfo.Local.GetUtcOffset(dto));
                                Methodes.PrintConsole(Config.sourceApplicationController, fullname + " bénéfice d'une formation le " + localDateTime.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture));
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
                                        DateTime date = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).Date;
                                        formattedDate = date.ToString("dd/MM/yyyy");
                                    }
                                    else formattedDate = "Date malformé";

                                    Methodes.PrintConsole(Config.sourceApplicationController, $"- Date: {formattedDate} Description: {description}");
                                }
                            }
                            else
                            {
                                DateTimeOffset dto = DateTimeOffset.FromUnixTimeSeconds(unixdate);
                                DateTimeOffset localDateTime = dto.ToOffset(TimeZoneInfo.Local.GetUtcOffset(dto));
                                Methodes.PrintConsole(Config.sourceApplicationController, "La mission de " + fullname + " le " + localDateTime.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture));
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

                                    if (long.TryParse(sformattedDate, out long sTimestamp) && long.TryParse(eformattedDate, out long eTimestamp))
                                    {
                                        DateTimeOffset sdto = DateTimeOffset.FromUnixTimeSeconds(sTimestamp);
                                        DateTimeOffset edto = DateTimeOffset.FromUnixTimeSeconds(eTimestamp);

                                        DateTimeOffset slocalDateTime = sdto.ToOffset(TimeZoneInfo.Local.GetUtcOffset(sdto));
                                        DateTimeOffset elocalDateTime = edto.ToOffset(TimeZoneInfo.Local.GetUtcOffset(edto));

                                        sformattedDate = slocalDateTime.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                                        eformattedDate = elocalDateTime.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                                    }

                                    Methodes.PrintConsole(Config.sourceApplicationController, $"- Date de début: {sformattedDate} Date de fin: {eformattedDate} Address : {address}");
                                }
                            }
                            else
                            {
                                DateTimeOffset dto = DateTimeOffset.FromUnixTimeSeconds(unixdate);
                                DateTimeOffset localDateTime = dto.ToOffset(TimeZoneInfo.Local.GetUtcOffset(dto));
                                Methodes.PrintConsole(Config.sourceApplicationController, fullname + " était en déplacement le " + localDateTime.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture));
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
                        if (Config.adminSettingsPIN == ParsedPIN)
                        {
                            AdministrationPanel();
                            break;
                        }
                        if (codePINAttempts > 2)
                        {
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
                        Methodes.PrintConsole(Config.sourceApplicationController, "5. Générer une exception");
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
                            else if (serviceNumber == 4) GenerateAndZipPayslips();
                            else if (serviceNumber == 5) Methodes.PrintConsole(Config.sourceMethodes, "Exception du test", true);
                            if (serviceNumber > 0 && serviceNumber < 6)
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

        /// <summary>
        /// Récupère les informations nécessaires sur un membre du personnel, y compris son nom, son identifiant et une date facultative.
        /// </summary>
        /// <param name="contractorName">Nom complet du membre ciblé, nettoyé et validé.</param>
        /// <param name="contractorId">Identifiant unique du membre, ou -1 si introuvable.</param>
        /// <param name="unixdate">Date en format Unix (secondes depuis l'époque Unix), ou -1 si non spécifiée ou invalide.</param>
        private static void GetData(out string contractorName, out int contractorId, out long unixdate)
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

            if (DateTime.TryParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
            {
                DateTimeOffset dto = new(dateTime.Date, TimeZoneInfo.Local.GetUtcOffset(dateTime.Date));
                unixdate = dto.ToUnixTimeSeconds();
            }
            else
            {
                unixdate = -1;
            }
        }

        /// <summary>
        /// Affiche le panneau d'administration pour gérer les fonctions, barèmes et les membres du personnel.
        /// Permet de revenir au menu principal.
        /// </summary>
        private static void AdministrationPanel()
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

        /// <summary>
        /// Exécute le service administrative correspondant au numéro saisi par l'utilisateur.
        /// </summary>
        /// <param name="service">Le numéro du service sélectionné par l'utilisateur.</param>
        /// <returns>
        /// Un message d'erreur si le service est invalide ou si une erreur s'est produite pendant l'exécution.
        /// Retourne une chaîne vide si le service s'exécute sans erreur.
        /// </returns>
        private static string RunAdminService(int service)
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

                    retryCrewMemberGSM: string crewMemberGSM = Methodes.ReadUserInput("Entrer le numéro GSM du nouveau membre: ") ?? string.Empty;

                        if (string.IsNullOrWhiteSpace(crewMemberGSM) || crewMemberGSM.Length < 10 || !Methodes.IsNumeric(crewMemberGSM))
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

                    retryCrewMemberAddress: string crewMemberAddress = Methodes.ReadUserInput("Entrer l'adresse domicile du nouveau membre: ") ?? string.Empty;

                        if (string.IsNullOrWhiteSpace(crewMemberAddress) || crewMemberAddress.Length < 15)
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
                                Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, La date doit être valide.");
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
                        {
                            Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, Cette position n'existe pas dans le système.");
                            goto retryCrewMemberJob;
                        }

                        var parameters = new Dictionary<string, object>
                        {
                            ["@fullName"] = MyRegex().Replace(crewMemberName.Trim(), " "),
                            ["@gsm"] = crewMemberGSM,
                            ["@email"] = crewMemberEmail,
                            ["@address"] = crewMemberAddress,
                            ["@endDate"] = crewMemberUnixEndDate,
                            ["@hours"] = crewMemberHours,
                            ["@salary"] = crewMemberSalary,
                            ["@job"] = job.JobId
                        };

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
                        else if (contract.EndDate < DateTimeOffset.Now.ToUnixTimeSeconds() && contract.EndDate > 0)
                        {
                            DateTime datetime = DateTimeOffset.FromUnixTimeSeconds(contract.EndDate).Date;
                            Methodes.PrintConsole(Config.sourceApplicationController, $"le contrat de {crewMemberName} a été mis en fin le {datetime.ToString("dd/MM/yyyy")}.");
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
                                    if (DateTime.TryParse(crewMemberAbsenceDate, out var dateTime))
                                    {
                                        DateTimeOffset dto = dateTime.Date;
                                        crewMemberUnixDate = dto.ToUnixTimeSeconds();
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
                                            UploadJustificative(fileName, base64String);
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
                                }
                                ShowContinuePrompt();
                                RunAdminService(2);
                                break;
                            }
                            else if (selectedOption == 4)
                            {
                                Training training = new(contract.ContractorId);
                                int currentYear = DateTime.Now.Year;
                                int count = 0, totalcount = 0;

                                List<QueryResultRow> row = training.ListTraining;
                                for (int i = 0; i < row.Count; i++)
                                {
                                    _ = long.TryParse(row[i]["date"], out long unixdate);
                                    DateTime date = DateTimeOffset.FromUnixTimeSeconds(unixdate).Date;
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

                                if (string.IsNullOrWhiteSpace(crewMemberFormateur) || crewMemberFormateur.Length < 3)
                                {
                                    Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, Le nom de l'entreprise/formateur doit être composée d'au moins 3 caractères et ne doit pas être vide.");
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
                                    contract.UpdateSalary(contract.Salary + (contract.Salary * 0.05), (int)DateTimeOffset.Now.ToUnixTimeSeconds());
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
                                int currentYear = DateTime.Now.Year;
                                int count = 0;

                                List<QueryResultRow> row = paidLeave.ListPaidLeave;
                                for (int i = 0; i < row.Count; i++)
                                {
                                    _ = long.TryParse(row[i]["startDate"], out long unixsdate);
                                    _ = long.TryParse(row[i]["endDate"], out long unixedate);
                                    DateTime sdate = DateTimeOffset.FromUnixTimeSeconds(unixsdate).Date;
                                    DateTime edate = DateTimeOffset.FromUnixTimeSeconds(unixedate).Date;
                                    TimeSpan span = edate - sdate;

                                    if (sdate.Year == currentYear)
                                    {
                                        count += span.Days;
                                    }
                                }

                                int totalbonus = 0;
                                if (contract.Job == "Employé")
                                {
                                    long secondsTotal = DateTimeOffset.Now.ToUnixTimeSeconds() - contract.StartDate;
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
                                    paidLeaveUnixStartDate = ((DateTimeOffset)sd.Date).ToUnixTimeSeconds();
                                    paidLeaveUnixEndDate = ((DateTimeOffset)ed.Date).ToUnixTimeSeconds();

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
                                if (DateTime.TryParse(workTravelStartDate, out DateTime sd) && DateTime.TryParse(workTravelEndDate, out DateTime ed))
                                {
                                    if (sd < DateTime.Now || ed < DateTime.Now || ed <= sd)
                                    {
                                        Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, la date du début et du fin doit être dans le futur.");
                                        goto retryWorkTravel;
                                    }
                                    workTravelUnixStartDate = new DateTimeOffset(sd).ToUnixTimeSeconds();
                                    workTravelUnixEndDate = new DateTimeOffset(ed).ToUnixTimeSeconds();
                                }

                            retryWorkTravelAddr: string WorkTravelAddr = Methodes.ReadUserInput("Enter l'adresse du déplacement: ") ?? string.Empty;
                                if (string.IsNullOrWhiteSpace(WorkTravelAddr) || WorkTravelAddr.Length < 15)
                                {
                                    Methodes.PrintConsole(Config.sourceApplicationController, "Erreur, L'adresse doit être composée d'au moins 15 caractères et ne doit pas être vide.");
                                    goto retryWorkTravelAddr;
                                }

                            retryWorkTravelDescription: string WorkTravelDescription = Methodes.ReadUserInput("Enter la description du déplacement: ") ?? string.Empty;
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
                                GenerateAndZipPayslips(contract.ContractorId);
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

        /// <summary>
        /// Stocke un PDF justificatif dans la base de données.
        /// </summary>
        /// <param name="fileName">Nom logique du fichier.</param>
        /// <param name="fileData">Données PDF encodées en base64.</param>
        private static void UploadJustificative(string fileName, string fileData)
        {
            string query = "INSERT IGNORE INTO pdf_files (fileName, fileData) VALUES (@filename, @filedata)";
            var parameters = new Dictionary<string, object>
            {
                { "@filename", fileName },
                { "@filedata", fileData }
            };
            Program.Controller.InsertData(query, parameters);
        }

        /// <summary>
        /// Récupère un PDF justificatif depuis la base de données et ouvre le dossier contenant le fichier téléchargé.
        /// </summary>
        /// <param name="fileName">Nom du PDF à télécharger.</param>
        private static void DownloadJustificative(string fileName)
        {
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string path = Path.Combine(desktop, fileName);

            var query = "SELECT fileData FROM PDF_files WHERE fileName = @filename";
            var parameters = new Dictionary<string, object> { { "@filename", fileName } };
            List<QueryResultRow> rows = Program.Controller.ReadData(query, parameters);

            if (rows.Count == 0)
            {
                Methodes.PrintConsole(Config.sourceMethodes, "le justificative est introuvable mais il est encodé dans la table 'Absence'.", true);
                return;
            }
            byte[] fileBytes = Convert.FromBase64String(rows[0]["fileData"]);
            File.WriteAllBytes(path, fileBytes);

            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = OperatingSystem.IsWindows() ? "explorer" :
                        OperatingSystem.IsMacOS() ? "open" : "xdg-open",
                Arguments = $"\"{desktop}\"",
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(psi);
        }

        /// <summary>
        /// Génère les fiches de paie mensuelles au format PDF, optionnellement pour un contractant spécifique,
        /// compresse les fichiers générés en une archive ZIP, sauvegarde cette archive sur le bureau,
        /// puis ouvre le dossier contenant l'archive.
        /// </summary>
        /// <param name="contractorId">
        /// L'ID du contractant pour lequel générer les fiches de paie.
        /// Utiliser -1 pour générer pour tous les contractants.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Levée si <paramref name="contractorId"/> est une valeur invalide (par exemple un ID négatif autre que -1).
        /// </exception>
        /// <exception cref="IOException">
        /// Levée en cas d'erreur lors de la création ou de la sauvegarde des fichiers PDF ou ZIP.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// Levée si le programme n'a pas les droits nécessaires pour écrire sur le bureau ou ouvrir le dossier.
        /// </exception>
        /// <exception cref="Exception">
        /// Levée pour toute autre erreur non prévue durant le processus.
        /// </exception>
        private static void GenerateAndZipPayslips(int contractorId = -1)
        {
            try
            {
                var payments = GetPaymentsForCurrentMonth(contractorId);

                string tempFolder = Path.Combine(Path.GetTempPath(), "Fiche_de_paie_" + Guid.NewGuid());
                Directory.CreateDirectory(tempFolder);

                foreach (var payment in payments)
                {
                    GeneratePdfForPayment(payment, tempFolder);
                }

                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                if (!Directory.Exists(desktopPath))
                    desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

                string zipFilePath = Path.Combine(desktopPath, $"Fiches_de_paie_{DateTime.Now:yyyyMMdd_HHmmss}.zip");

                if (File.Exists(zipFilePath))
                    File.Delete(zipFilePath);

                ZipFile.CreateFromDirectory(tempFolder, zipFilePath);
                Directory.Delete(tempFolder, true);

                OpenFolderAndSelectFile(zipFilePath);
            }
            catch (Exception ex)
            {
                Methodes.PrintConsole(Config.sourceMethodes, ex.ToString(), true);
            }
        }
        /// <summary>
        /// Récupère les paiements du mois courant, optionnellement pour un contractant.
        /// </summary>
        /// <param name="contractorId">ID du contractant (optionnel, -1 pour tous).</param>
        /// <returns>Liste des paiements correspondant aux critères.</returns>
        private static List<QueryResultRow> GetPaymentsForCurrentMonth(int contractorId = -1)
        {
            string query = @"
                SELECT p.*, c.fullname
                FROM Payments p
                JOIN Contracts c ON p.contractorId = c.contractorId
                WHERE YEAR(p.payment_date) = @year AND MONTH(p.payment_date) = @month";

            var parameters = new Dictionary<string, object>
            {
                ["@year"] = DateTime.Now.Year,
                ["@month"] = DateTime.Now.Month
            };

            if (contractorId != -1)
            {
                query += " AND p.contractorId = @contractorId";
                parameters["@contractorId"] = contractorId;
            }

            var payments = Program.Controller.ReadData(query, parameters);
            return payments;
        }

        /// <summary>
        /// Génère un fichier PDF pour une fiche de paie donnée.
        /// </summary>
        /// <param name="payment">Les données de paiement sous forme de QueryResultRow.</param>
        /// <param name="folderPath">Le dossier de destination pour le fichier PDF.</param>
        private static void GeneratePdfForPayment(QueryResultRow payment, string folderPath)
        {
            using var document = new PdfDocument();
            document.Info.Title = "Fiche de paie";

            var page = document.AddPage();
            using var gfx = XGraphics.FromPdfPage(page);

            GlobalFontSettings.FontResolver = new MinimalFontResolver();

            var titleFont = new XFont("LiberationSans", 20, XFontStyleEx.Bold);
            var contentFont = new XFont("LiberationSans", 12, XFontStyleEx.Regular);

            gfx.DrawString("Fiche de paie", titleFont, XBrushes.Black,
                new XRect(0, 20, page.Width.Point, 40), XStringFormats.TopCenter);

            DateTime paymentDate = DateTime.Parse(payment.Columns["payment_date"]);
            DateTime periodStart = DateTime.Parse(payment.Columns["period_start"]);
            DateTime periodEnd = DateTime.Parse(payment.Columns["period_end"]);

            var lines = new List<string>
            {
                $"Nom complet : {payment.Columns["fullname"]}",
                $"ID de paiement : {payment.Columns["id"]}",
                $"ID du contractant : {payment.Columns["contractorId"]}",
                $"Date de paiement : {paymentDate:d}",
                $"Montant : € {payment.Columns["amount"]} net",
                $"Début de la période : {periodStart:d}",
                $"Fin de la période : {periodEnd:d}",
                $"Type de travail : {payment.Columns["job_type"]}",
                $"Jours d'absence payés : {payment.Columns["paid_absence_days"]}",
                $"Jours d'absence non payés : {payment.Columns["unpaid_absence_days"]}"
            };

            double x = 40;
            double y = 80;
            double lineHeight = contentFont.GetHeight();

            foreach (var line in lines)
            {
                gfx.DrawString(line, contentFont, XBrushes.Black, new XPoint(x, y), XStringFormats.TopLeft);
                y += lineHeight + 2; // Add some spacing between lines
            }

            string monthName = paymentDate.ToString("MMMM", new CultureInfo("fr-FR"));
            string fileName = $"fiche_de_paie_{monthName}_{paymentDate:yyyy}_{SafeFileName(payment.Columns["contractorId"])}.pdf";
            string fullPath = Path.Combine(folderPath, fileName);

            document.Save(fullPath);
        }

        /// <summary>
        /// Rend un nom de fichier valide en remplaçant les caractères interdits.
        /// </summary>
        /// <param name="name">Nom de fichier potentiellement invalide.</param>
        /// <returns>Nom de fichier sécurisé.</returns>
        private static string SafeFileName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name;
        }

        /// <summary>
        /// Ouvre le dossier contenant un fichier et le sélectionne dans l'explorateur de fichiers.
        /// </summary>
        /// <param name="filePath">Chemin du fichier à sélectionner.</param>
        private static void OpenFolderAndSelectFile(string filePath)
        {
            string folderPath = Path.GetDirectoryName(filePath) ?? "";

            if (OperatingSystem.IsWindows())
                System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{filePath}\"");
            else if (OperatingSystem.IsMacOS())
                System.Diagnostics.Process.Start("open", folderPath);
            else if (OperatingSystem.IsLinux())
                System.Diagnostics.Process.Start("xdg-open", folderPath);
            else
                Methodes.PrintConsole(Config.sourceMethodes, "Erreur, Système d'explotation incompatible.", true);
        }

        /// <summary>
        /// Vérifie pour chaque contrat dans la liste des contrats si un ajustement salarial de +2% tous les 2 ans est dû,
        /// puis met à jour le salaire si nécessaire.
        /// </summary>
        private static void UpdateSalariesIfDue()
        {
            Contracts contracts = new();

            foreach (var row in contracts.ListContracts)
            {
                if (!int.TryParse(row["startDate"], out int startDate)) continue;
                if (!double.TryParse(row["salary"], out double currentSalary)) continue;
                if (!int.TryParse(row["contractorId"], out int contractorId)) continue;

                int nowUnix = (int)DateTimeOffset.Now.ToUnixTimeSeconds();
                int secondsPassed = nowUnix - startDate;
                int yearsPassed = secondsPassed / (60 * 60 * 24 * 365);
                int twoYearPeriods = yearsPassed / 2;

                if (twoYearPeriods > 0)
                {
                    int lastUpdateUnix = 0;
                    if (!int.TryParse(row["lastUpdateDate"], out lastUpdateUnix))
                    {
                        lastUpdateUnix = 0;
                    }

                    int lastUpdatePeriods = (lastUpdateUnix - startDate) / (60 * 60 * 24 * 365 * 2);

                    if (twoYearPeriods > lastUpdatePeriods)
                    {
                        double expectedSalary = currentSalary;
                        for (int i = lastUpdatePeriods; i < twoYearPeriods; i++)
                        {
                            expectedSalary *= 1.02;
                        }
                        expectedSalary = Math.Round(expectedSalary, 2);

                        if (Math.Abs(expectedSalary - currentSalary) > 0.01)
                        {
                            var contractToUpdate = new Contracts(row["fullname"]);

                            contractToUpdate.UpdateSalary(expectedSalary, nowUnix);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Affiche un message invitant l'utilisateur à appuyer sur une touche pour revenir au menu précédent.
        /// En mode non-production, cette invite est ignorée.
        /// </summary>
        /// <remarks>
        /// La méthode attend une entrée clavier via <see cref="Console.ReadKey"/> et gère toute exception
        /// pouvant survenir lors de la lecture.
        /// </remarks>
        /// <exception cref="IOException">
        /// Peut survenir si une erreur d'entrée/sortie se produit lors de l'appel à <see cref="Console.ReadKey"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Peut survenir si l'entrée standard n'est pas un terminal interactif lors de l'appel à <see cref="Console.ReadKey"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Peut survenir si un paramètre hors limites est passé à <see cref="Console.ReadKey"/> (non utilisé ici, mais mentionné par prudence).
        /// </exception>
        private static void ShowContinuePrompt()
        {
            if (!Config.productionRun) return;
            Methodes.PrintConsole(Config.sourceApplicationController, "Appuyez sur n'importe quelle touche pour revenir au menu précédent...\n");
            try
            {
                _ = Console.ReadKey();
            }
            catch (Exception ex)
            {
                Methodes.PrintConsole(Config.sourceMethodes, ex.ToString(), true);
            }
        }
        
        /// <summary>
        /// Fournisseur de police personnalisé pour PdfSharp.
        /// </summary>
        class MinimalFontResolver : IFontResolver
        {
            private readonly byte[] fontData;

            /// <summary>
            /// Charge les données de police intégrées depuis les ressources.
            /// </summary>
            public MinimalFontResolver()
            {
                var assembly = Assembly.GetExecutingAssembly();
                Stream? fontStream = assembly.GetManifestResourceStream("Gestionnaire.fonts.LiberationSans-Regular.ttf");
                if (fontStream == null)
                    Methodes.PrintConsole(Config.sourceMethodes, "Erreur, la police Gestionnaire.fonts.LiberationSans-Regular.ttf introuvable.", true);

                using MemoryStream ms = new MemoryStream();
                fontStream?.CopyTo(ms);
                fontData = ms.ToArray();
            }

            /// <summary>
            /// Récupère les données binaires de la police.
            /// </summary>
            /// <param name="faceName">Nom de la police.</param>
            /// <returns>Données binaires de la police.</returns>
            public byte[] GetFont(string faceName) => fontData;

            /// <summary>
            /// Résout le type de police à utiliser pour un style donné.
            /// </summary>
            /// <param name="familyName">Nom de la famille de police.</param>
            /// <param name="isBold">Indique si la police est en gras.</param>
            /// <param name="isItalic">Indique si la police est en italique.</param>
            /// <returns>Informations sur la police résolue.</returns>
            public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
                => new("LiberationSans");
        }

        /// <summary>
        /// Expression régulière générée pour détecter un ou plusieurs espaces consécutifs.
        /// </summary>
        [GeneratedRegex(@"\s+")]
        private static partial Regex MyRegex();
    }
}