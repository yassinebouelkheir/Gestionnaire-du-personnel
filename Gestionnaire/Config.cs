namespace Gestionnaire
{
    class Config
    {
        // Program configuration
        public const bool productionRun = false;
        /*
            const bool productionRun

            This constant enable or disable debug mode.
            Set values:
                true  to run production mode and disable debug mode.
                false to run staging mode and enable debug mode.
        */

        public const int adminSettingsPIN = 123456;
        /*
            const int adminSettingsPIN

            For security purposes, this section does not have documentation.
        */

        public const int consoleDateTime = 2;
        /*
            const int consoleDateTime

            This constant set the way the date is formatted in the console.
            Set values:
                0 to disable the datetime completely
                1 for yyyy/MM/dd HH:mm:ss
                2 for HH:mm:ss
                3 for yyyy/MM/dd
        */

        public const int maxLoginAttempts = 3;
        /*
            const int maxLoginAttempts

            This constant set the maximum user login attempts before the application shutdown.
            Set values:
                int (0-inf)
        */

        // SQLite configurations
        public const string mysqlServer = "localhost";
        /*
            const string mysqlServer

            For security purposes, this section does not have documentation.
        */

        public const int mysqlPort = 3306;
        /*
            const int mysqlServer

            For security purposes, this section does not have documentation.
        */

        public const string mysqlUsername = "root";
        /*
            const string mysqlUsername

            For security purposes, this section does not have documentation.
        */

        public const string mysqlPassword = "";
        /*
            const string mysqlPassword

            For security purposes, this section does not have documentation.
        */

        public const string mysqlDatabase = "database";
        /*
            const string mysqlDatabase

            For security purposes, this section does not have documentation.
        */

        public const string skeleton = @"CREATE TABLE IF NOT EXISTS `Absences` (
            `id` int(11) NOT NULL AUTO_INCREMENT,
            `contractorId` int(11) NOT NULL,
            `date` int(11) NOT NULL,
            `justificativeDocument` varchar(128) DEFAULT NULL,
            PRIMARY KEY (`id`)
            );

            CREATE TABLE IF NOT EXISTS `Contracts` (
            `contractorId` int(11) NOT NULL AUTO_INCREMENT,
            `fullname` varchar(64) NOT NULL,
            `gsm` varchar(16) NOT NULL,
            `email` varchar(32) NOT NULL,
            `address` text NOT NULL,
            `startDate` int(11) NOT NULL DEFAULT unix_timestamp(),
            `endDate` int(11) NOT NULL DEFAULT 0,
            `hours` int(11) NOT NULL DEFAULT 40,
            `salary` double NOT NULL,
            `fonction` int(11) NOT NULL,
            PRIMARY KEY (`contractorId`)
            );

            CREATE TABLE IF NOT EXISTS `Jobs` (
            `id` int(11) NOT NULL AUTO_INCREMENT,
            `authorityLevel` int(11) NOT NULL,
            `name` varchar(32) NOT NULL,
            PRIMARY KEY (`id`)
            );

            CREATE TABLE IF NOT EXISTS `Mission` (
            `id` int(11) NOT NULL AUTO_INCREMENT,
            `contractorId` int(11) NOT NULL,
            `description` varchar(256) NOT NULL,
            `date` int(11) NOT NULL,
            PRIMARY KEY (`id`)
            );

            CREATE TABLE IF NOT EXISTS `PaidLeave` (
            `Id` int(11) NOT NULL AUTO_INCREMENT,
            `contractorId` int(11) NOT NULL,
            `startDate` int(11) NOT NULL,
            `endDate` int(11) NOT NULL,
            `reason` varchar(128) NOT NULL,
            PRIMARY KEY (`Id`)
            );

            CREATE TABLE IF NOT EXISTS `Training` (
            `id` int(11) NOT NULL AUTO_INCREMENT,
            `contractorId` int(11) NOT NULL,
            `type` varchar(128) NOT NULL,
            `address` varchar(256) NOT NULL,
            `formateur` varchar(128) NOT NULL,
            `date` int(11) NOT NULL,
            PRIMARY KEY (`id`)
            );

            CREATE TABLE IF NOT EXISTS `Users` (
            `id` int(11) NOT NULL AUTO_INCREMENT,
            `username` varchar(32) NOT NULL,
            `password_hash` varchar(256) NOT NULL,
            `salt` varchar(32) NOT NULL,
            PRIMARY KEY (`id`)
            );

            CREATE TABLE IF NOT EXISTS `WorkTravel` (
            `id` int(11) NOT NULL AUTO_INCREMENT,
            `contractorId` int(11) NOT NULL,
            `startDate` int(11) NOT NULL,
            `endDate` int(11) NOT NULL,
            `address` varchar(128) NOT NULL,
            `description` varchar(128) NOT NULL,
            PRIMARY KEY (`id`)
            );

            CREATE TABLE IF NOT EXISTS Payments (
            id INT AUTO_INCREMENT,
            contractorId INT NOT NULL,
            payment_date DATE NOT NULL,
            amount DOUBLE NOT NULL,
            period_start DATE NOT NULL,
            period_end DATE NOT NULL,
            job_type VARCHAR(32) NOT NULL,
            paid_absence_days INT DEFAULT 0,
            unpaid_absence_days INT DEFAULT 0,
            PRIMARY KEY (`id`)
            );

            CREATE TABLE IF NOT EXISTS PDF_files (
                id INT AUTO_INCREMENT PRIMARY KEY,
                fileName VARCHAR(255) UNIQUE,
                fileData LONGTEXT,
                uploaded_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            );

            INSERT IGNORE INTO `Jobs` (`id`, `authorityLevel`, `name`) VALUES
            (1, 1, 'Administrateur'),
            (2, 2, 'Employé'),
            (3, 3, 'Ouvrier'),
            (4, 4, 'Consultant');

            INSERT IGNORE INTO `Users` (`id`, `username`, `password_hash`, `salt`) VALUES
            (1, 'admin', '24euMIjLiutdFt52gv/nIsNi8OKtyMcHEGH3WnYfvTI=', '6GiAEASB7JnuM3SjrG6Hag==');

            SET GLOBAL event_scheduler = ON;
            
            CREATE EVENT IF NOT EXISTS generate_salary_payments
            ON SCHEDULE EVERY 1 DAY
            DO
            BEGIN
                DECLARE day_of_month INT DEFAULT DAYOFMONTH(CURRENT_DATE());

                IF day_of_month IN (15, 28) THEN
                    INSERT IGNORE INTO Payments (contractorId, payment_date, amount, period_start, period_end, job_type, paid_absence_days, unpaid_absence_days)
                    SELECT 
                        c.contractorId,
                        CURRENT_DATE,
                        (c.salary / 2) * (1 - (GREATEST(total_abs_days - 14, 0) / 10)),
                        DATE_SUB(CURRENT_DATE, INTERVAL 15 DAY),
                        CURRENT_DATE,
                        j.name,
                        LEAST(total_abs_days, 14),
                        GREATEST(total_abs_days - 14, 0)
                    FROM Contracts c
                    JOIN Jobs j ON c.fonction = j.id
                    LEFT JOIN (
                        SELECT contractorId,
                            COUNT(DISTINCT FROM_UNIXTIME(date, '%Y-%m-%d')) AS total_abs_days
                        FROM Absences
                        WHERE date BETWEEN UNIX_TIMESTAMP(DATE_SUB(CURRENT_DATE, INTERVAL 14 DAY)) AND UNIX_TIMESTAMP(CURRENT_DATE)
                        GROUP BY contractorId
                    ) abs ON abs.contractorId = c.contractorId
                    WHERE j.name = 'Ouvrier';
                END IF;

                IF day_of_month = 28 THEN
                    INSERT IGNORE INTO Payments (contractorId, payment_date, amount, period_start, period_end, job_type, paid_absence_days, unpaid_absence_days)
                    SELECT
                        c.contractorId,
                        CURRENT_DATE,
                        CASE
                            WHEN j.name = 'Employé' THEN
                                CASE
                                    WHEN total_abs_days > 30 THEN 0
                                    ELSE c.salary * (1 - (GREATEST(total_abs_days - 30, 0) / 30))
                                END
                            WHEN j.name = 'Consultant' THEN
                                (c.salary / 20) * (20 - total_abs_days)
                            ELSE 0
                        END,
                        DATE_FORMAT(CURRENT_DATE, '%Y-%m-01'),
                        CURRENT_DATE,
                        j.name,
                        CASE WHEN j.name = 'Employé' THEN LEAST(total_abs_days, 30) ELSE 0 END,
                        CASE WHEN j.name = 'Employé' THEN GREATEST(total_abs_days - 30, 0) ELSE total_abs_days END
                    FROM Contracts c
                    JOIN Jobs j ON c.fonction = j.id
                LEFT JOIN (
                SELECT a.contractorId,
                        COUNT(DISTINCT FROM_UNIXTIME(a.date, '%Y-%m-%d')) AS total_abs_days
                FROM Absences a
                WHERE a.date BETWEEN UNIX_TIMESTAMP(DATE_SUB(CURRENT_DATE, INTERVAL 14 DAY))
                                AND UNIX_TIMESTAMP(CURRENT_DATE)
                    AND NOT EXISTS (
                        SELECT 1
                        FROM PaidLeave pl
                        WHERE pl.contractorId = a.contractorId
                        AND a.date BETWEEN pl.startDate AND pl.endDate
                    )
                GROUP BY a.contractorId
                ) abs ON abs.contractorId = c.contractorId
                    WHERE j.name IN ('Employé', 'Consultant');
                END IF;
            END;
            ";
        /*
            const string skeleton

            This constant is the SQL Schéma for our program.
        */

        public const string debugScript = @"
            INSERT IGNORE INTO `Contracts` (fullname, gsm, email, address, startDate, endDate, hours, salary, fonction) VALUES
            ('Marie Dupont', '+32 471 12 34 56', 'marie.dupont@exemple.be', 'Rue de la Loi 16, 1000 Bruxelles', 1762000000, 0, 38, 3200, 1),
            ('Jean Martin', '+32 486 23 45 67', 'jean.martin@exemple.be', 'Avenue Louise 45, 1050 Ixelles', 1762086400, 0, 38, 2800, 2),
            ('Sophie Lambert', '+32 478 34 56 78', 'sophie.lambert@exemple.be', 'Chaussee de Waterloo 120, 1180 Uccle', 1762172800, 0, 38, 3000, 3),
            ('Luc Moreau', '+32 472 45 67 89', 'luc.moreau@exemple.be', 'Place Sainte Catherine 22, 1000 Bruxelles', 1762259200, 0, 38, 2700, 2),
            ('Isabelle Bernard', '+32 475 56 78 90', 'isabelle.bernard@exemple.be', 'Rue Neuve 55, 1000 Bruxelles', 1762345600, 0, 38, 3100, 1),
            ('Pierre Dubois', '+32 480 67 89 01', 'pierre.dubois@exemple.be', 'Boulevard Anspach 30, 1000 Bruxelles', 1762432000, 0, 38, 2900, 3),
            ('Camille Gerard', '+32 466 78 90 12', 'camille.gerard@exemple.be', 'Rue des Alexiens 17, 1000 Bruxelles', 1762518400, 0, 38, 3050, 2),
            ('Marc Lefebvre', '+32 474 89 01 23', 'marc.lefebvre@exemple.be', 'Avenue de Tervueren 50, 1150 Woluwe Saint Pierre', 1762604800, 0, 38, 3150, 1),
            ('Elodie Simon', '+32 473 90 12 34', 'elodie.simon@exemple.be', 'Rue Royale 100, 1000 Bruxelles', 1762691200, 0, 38, 2650, 3),
            ('Thomas Petit', '+32 470 12 34 56', 'thomas.petit@exemple.be', 'Chaussee de Ixelles 75, 1050 Ixelles', 1762777600, 0, 38, 2800, 2);

            INSERT IGNORE INTO `Absences` (contractorId, date, justificativeDocument) VALUES
            (1, 1735800000, NULL),
            (2, 1737000000, NULL),
            (3, 1738500000, NULL),
            (4, 1740000000, NULL),
            (5, 1741500000, NULL),
            (6, 1743000000, NULL),
            (7, 1744500000, NULL),
            (8, 1746000000, NULL),
            (9, 1747500000, NULL),
            (10, 1749000000, NULL);

            INSERT IGNORE INTO `Mission` (contractorId, description, date) VALUES
            (1, 'Correction du bug de connexion', 1762000000),
            (2, 'Developpement de la page d accueil', 1762086400),
            (3, 'Optimisation de la base de donnees', 1762172800),
            (4, 'Reunion avec le client', 1762259200),
            (5, 'Preparation du rapport mensuel', 1762345600),
            (6, 'Revue de code', 1762432000),
            (7, 'Conception des maquettes UI', 1762518400),
            (8, 'Deploiement de la mise a jour', 1762604800),
            (9, 'Audit de securite', 1762691200),
            (10, 'Session de formation interne', 1762777600);

            INSERT IGNORE INTO `PaidLeave` (contractorId, startDate, endDate, reason) VALUES
            (1, 1762100000, 1762186400, 'Conge annuel'),
            (2, 1762186400, 1762272800, 'Conge maladie'),
            (3, 1762272800, 1762359200, 'Conge personnel'),
            (4, 1762359200, 1762445600, 'Urgence familiale'),
            (5, 1762445600, 1762532000, 'Conge annuel'),
            (6, 1762532000, 1762618400, 'Formation'),
            (7, 1762618400, 1762704800, 'Conge annuel'),
            (8, 1762704800, 1762791200, 'Conge maladie'),
            (9, 1762791200, 1762877600, 'Conge personnel'),
            (10, 1762877600, 1762964000, 'Conge annuel');

            INSERT IGNORE INTO `Training` (contractorId, type, address, formateur, date) VALUES
            (1, 'Leadership', 'Centre de Formation Bruxelles', 'Jean Dupuis', 1736000000),
            (2, 'Securite', 'Centre de Formation Bruxelles', 'Marie Lefevre', 1737200000),
            (3, 'Competences techniques', 'Centre de Formation Bruxelles', 'Michel Bernard', 1738600000),
            (4, 'Service client', 'Centre de Formation Bruxelles', 'Anne Dubois', 1740200000),
            (5, 'Gestion de projet', 'Centre de Formation Bruxelles', 'Paul Martin', 1741600000),
            (6, 'Communication', 'Centre de Formation Bruxelles', 'Lucie Simon', 1743100000),
            (7, 'Developpement logiciel', 'Centre de Formation Bruxelles', 'David Lambert', 1744500000),
            (8, 'Cohesion equipe', 'Centre de Formation Bruxelles', 'Emma Moreau', 1745900000),
            (9, 'Premiers secours', 'Centre de Formation Bruxelles', 'Christophe Bernard', 1747300000),
            (10, 'Gestion du temps', 'Centre de Formation Bruxelles', 'Nina Dupont', 1748700000);


            INSERT IGNORE INTO `WorkTravel` (contractorId, startDate, endDate, address, description) VALUES
            (1, 1762000000, 1762086400, 'Bureau de Bruxelles', 'Reunion client et lancement du projet'),
            (2, 1762086400, 1762172800, 'Bureau de Liege', 'Atelier et formation'),
            (3, 1762172800, 1762259200, 'Bureau de Gand', 'Evaluation technique'),
            (4, 1762259200, 1762345600, 'Bureau d Anvers', 'Livraison du projet'),
            (5, 1762345600, 1762432000, 'Bureau de Namur', 'Evenements reseau'),
            (6, 1762432000, 1762518400, 'Bureau de Charleroi', 'Support client'),
            (7, 1762518400, 1762604800, 'Bureau de Mons', 'Consultation'),
            (8, 1762604800, 1762691200, 'Bureau de Louvain', 'Formation et developpement'),
            (9, 1762691200, 1762777600, 'Bureau de Bruges', 'Reunions strategiques'),
            (10, 1762777600, 1762864000, 'Bureau de Hasselt', 'Evaluation du projet');";
        /*
            const string debugScript

            This constant is fill-in for our database (Debug mode only).
        */

        public const string generatePayslips = @"
            INSERT IGNORE INTO Payments (contractorId, payment_date, amount, period_start, period_end, job_type, paid_absence_days, unpaid_absence_days)
            SELECT 
            c.contractorId,
            CURRENT_DATE,
            (c.salary / 160) * (c.hours * 10 / 5) * (1 - (GREATEST(COALESCE(abs.total_abs_days, 0) - 14, 0) / 10)),
            DATE_SUB(CURRENT_DATE, INTERVAL 15 DAY),
            CURRENT_DATE,
            j.name,
            LEAST(COALESCE(abs.total_abs_days, 0), 14),
            GREATEST(COALESCE(abs.total_abs_days, 0) - 14, 0)
            FROM Contracts c
            JOIN Jobs j ON c.fonction = j.id
            LEFT JOIN (
            SELECT contractorId,
                COUNT(DISTINCT FROM_UNIXTIME(`date`, '%Y-%m-%d')) AS total_abs_days
            FROM Absences
            WHERE `date` BETWEEN UNIX_TIMESTAMP(DATE_SUB(CURRENT_DATE, INTERVAL 14 DAY)) AND UNIX_TIMESTAMP(CURRENT_DATE)
            GROUP BY contractorId
            ) abs ON abs.contractorId = c.contractorId
            WHERE j.name = 'Ouvrier';

            INSERT IGNORE INTO Payments (contractorId, payment_date, amount, period_start, period_end, job_type, paid_absence_days, unpaid_absence_days)
            SELECT
            c.contractorId,
            CURRENT_DATE,
            CASE
                WHEN j.name = 'Employé' THEN
                CASE
                    WHEN COALESCE(abs.total_abs_days, 0) > 30 THEN 0
                    ELSE c.salary * (1 - (GREATEST(COALESCE(abs.total_abs_days, 0) - 30, 0) / 30))
                END
                WHEN j.name = 'Consultant' THEN
                (c.salary / 20) * (20 - COALESCE(abs.total_abs_days, 0))
                ELSE 0
            END,
            DATE_FORMAT(CURRENT_DATE, '%Y-%m-01'),
            CURRENT_DATE,
            j.name,
            CASE WHEN j.name = 'Employé' THEN LEAST(COALESCE(abs.total_abs_days, 0), 30) ELSE 0 END,
            CASE WHEN j.name = 'Employé' THEN GREATEST(COALESCE(abs.total_abs_days, 0) - 30, 0) ELSE COALESCE(abs.total_abs_days, 0) END
            FROM Contracts c
            JOIN Jobs j ON c.fonction = j.id
            LEFT JOIN (
                SELECT a.contractorId,
                        COUNT(DISTINCT FROM_UNIXTIME(a.date, '%Y-%m-%d')) AS total_abs_days
                FROM Absences a
                WHERE a.date BETWEEN UNIX_TIMESTAMP(DATE_SUB(CURRENT_DATE, INTERVAL 14 DAY))
                                AND UNIX_TIMESTAMP(CURRENT_DATE)
                    AND NOT EXISTS (
                        SELECT 1
                        FROM PaidLeave pl
                        WHERE pl.contractorId = a.contractorId
                        AND a.date BETWEEN pl.startDate AND pl.endDate
                    )
                GROUP BY a.contractorId
                ) abs ON abs.contractorId = c.contractorId
            WHERE j.name IN ('Employé', 'Consultant');
            ";
        /*
            const string generatePayslips

            This constant is fill-in 'payments' table with payslip informations (Debug mode only).
        */

        // Program constants
        public const string sourceMySQL = "MySQLController";
        public const string sourceProgram = "Program";
        public const string sourceDataset = "Dataset";
        public const string sourceMethodes = "Methodes";
        public const string sourceApplicationController = "ApplicationController";
        /*
            const string source(*)

            These constants are name of each .cs source file we access. (for debug purposes)
        */
        public const string errorMessage = "Un erreur est survenue. L'opération n'a pas pu aboutir. Veuillez réessayer ultérieurement ou contacter notre support technique en cas de besoin.";
        /*
            const string errorMessage

            This constant is default error message to show to the client when there's a serious error.
        */
    }
}