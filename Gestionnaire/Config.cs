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
        
        public const string sftpServer = "localhost";
        /*
            const string serverAddress

            This constant set the server address ip for FTP services. (IPv4 format)
            Set values:
                string ***.***.***.***
        */
    public const string sftpPort = "22";
        /*
            const string serverAddress

            This constant set the server address ip for FTP services. (IPv4 format)
            Set values:
                string ***.***.***.***
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
            `reason` varchar(128) DEFAULT NULL,
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
                        SELECT contractorId,
                            COUNT(DISTINCT FROM_UNIXTIME(date, '%Y-%m-%d')) AS total_abs_days
                        FROM Absences
                        WHERE date BETWEEN UNIX_TIMESTAMP(DATE_FORMAT(CURRENT_DATE, '%Y-%m-01')) AND UNIX_TIMESTAMP(CURRENT_DATE)
                        GROUP BY contractorId
                    ) abs ON abs.contractorId = c.contractorId
                    WHERE j.name IN ('Employé', 'Consultant');
                END IF;
            END;
            ";

        public const string debugScript = @"
            INSERT IGNORE INTO Contracts (fullname, gsm, email, address, startDate, endDate, hours, salary, fonction) VALUES
            ('Lucas Dupont', '0470123456', 'lucas.dupont@mail.be', 'Rue de la Loi 10, Bruxelles', 1654041600, 0, 40, 3200.00, 1),
            ('Emma Janssens', '0498765432', 'emma.janssens@mail.be', 'Avenue Louise 50, Bruxelles', 1648771200, 0, 38, 2900.00, 2),
            ('Noah Peeters', '0487654321', 'noah.peeters@mail.be', 'Rue Kartuizerstraat 23, Anvers', 1672444800, 0, 40, 3100.00, 3),
            ('Mila Vermeulen', '0466123456', 'mila.vermeulen@mail.be', 'Grand-Place 1, Gand', 1661990400, 0, 40, 3000.00, 4),
            ('Louis Martens', '0477123456', 'louis.martens@mail.be', 'Rue Royale 77, Bruxelles', 1659312000, 0, 40, 3050.00, 1),
            ('Lotte De Smet', '0491234567', 'lotte.desmet@mail.be', 'Langestraat 45, Bruges', 1680307200, 0, 35, 2800.00, 2),
            ('Finn Jacobs', '0489123456', 'finn.jacobs@mail.be', 'Grand-Place 5, Louvain', 1646092800, 0, 40, 3200.00, 3),
            ('Zoe Claes', '0467123456', 'zoe.claes@mail.be', 'Veldstraat 12, Courtrai', 1656633600, 0, 40, 2950.00, 4),
            ('Arthur Willems', '0478123456', 'arthur.willems@mail.be', 'Meir 90, Anvers', 1664582400, 0, 40, 3100.00, 1),
            ('Lina Maes', '0494123456', 'lina.maes@mail.be', 'Korenmarkt 3, Gand', 1677628800, 0, 40, 3000.00, 2);

            INSERT IGNORE INTO Absences (contractorId, date, reason, justificativeDocument) VALUES
            (1, 1690809600, 'Maladie', 'justificativeAbsence_0470123456.pdf'),
            (1, 1691472000, 'Déplacement professionnel', 'justificativeAbsence_0470123456.pdf'),
            (1, 1689456000, 'Personnel', NULL),
            (1, 1692057600, 'Problème familial', 'justificativeAbsence_0470123456.pdf'),
            (1, 1692649600, 'Maladie', 'justificativeAbsence_0470123456.pdf'),
            (1, 1689849600, 'Conditions météo', NULL),
            (1, 1693238400, 'Problème de transport', 'justificativeAbsence_0470123456.pdf'),

            (2, 1690809600, 'Problème familial', NULL),
            (2, 1691472000, 'Personnel', 'justificativeAbsence_0498765432.pdf'),
            (2, 1689456000, 'Maladie', 'justificativeAbsence_0498765432.pdf'),
            (2, 1692057600, 'Déplacement professionnel', 'justificativeAbsence_0498765432.pdf'),
            (2, 1692649600, 'Maladie', NULL),
            (2, 1689849600, 'Conditions météo', 'justificativeAbsence_0498765432.pdf'),
            (2, 1693238400, 'Personnel', 'justificativeAbsence_0498765432.pdf'),

            (3, 1690809600, 'Maladie', 'justificativeAbsence_0487654321.pdf'),
            (3, 1691472000, 'Déplacement professionnel', NULL),
            (3, 1689456000, 'Personnel', 'justificativeAbsence_0487654321.pdf'),
            (3, 1692057600, 'Problème familial', 'justificativeAbsence_0487654321.pdf'),
            (3, 1692649600, 'Maladie', NULL),
            (3, 1689849600, 'Problème de transport', 'justificativeAbsence_0487654321.pdf'),
            (3, 1693238400, 'Conditions météo', 'justificativeAbsence_0487654321.pdf'),

            (4, 1690809600, 'Personnel', 'justificativeAbsence_0466123456.pdf'),
            (4, 1691472000, 'Problème familial', NULL),
            (4, 1689456000, 'Déplacement professionnel', 'justificativeAbsence_0466123456.pdf'),
            (4, 1692057600, 'Maladie', 'justificativeAbsence_0466123456.pdf'),
            (4, 1692649600, 'Personnel', NULL),
            (4, 1689849600, 'Conditions météo', 'justificativeAbsence_0466123456.pdf'),
            (4, 1693238400, 'Problème de transport', 'justificativeAbsence_0466123456.pdf'),

            (5, 1690809600, 'Déplacement professionnel', 'justificativeAbsence_0477123456.pdf'),
            (5, 1691472000, 'Maladie', NULL),
            (5, 1689456000, 'Problème familial', 'justificativeAbsence_0477123456.pdf'),
            (5, 1692057600, 'Personnel', 'justificativeAbsence_0477123456.pdf'),
            (5, 1692649600, 'Conditions météo', NULL),
            (5, 1689849600, 'Maladie', 'justificativeAbsence_0477123456.pdf'),
            (5, 1693238400, 'Problème de transport', 'justificativeAbsence_0477123456.pdf'),

            (6, 1690809600, 'Maladie', 'justificativeAbsence_0491234567.pdf'),
            (6, 1691472000, 'Déplacement professionnel', NULL),
            (6, 1689456000, 'Personnel', 'justificativeAbsence_0491234567.pdf'),
            (6, 1692057600, 'Problème familial', 'justificativeAbsence_0491234567.pdf'),
            (6, 1692649600, 'Maladie', NULL),
            (6, 1689849600, 'Conditions météo', 'justificativeAbsence_0491234567.pdf'),
            (6, 1693238400, 'Problème de transport', 'justificativeAbsence_0491234567.pdf'),

            (7, 1690809600, 'Personnel', 'justificativeAbsence_0489123456.pdf'),
            (7, 1691472000, 'Problème familial', NULL),
            (7, 1689456000, 'Déplacement professionnel', 'justificativeAbsence_0489123456.pdf'),
            (7, 1692057600, 'Maladie', 'justificativeAbsence_0489123456.pdf'),
            (7, 1692649600, 'Personnel', NULL),
            (7, 1689849600, 'Conditions météo', 'justificativeAbsence_0489123456.pdf'),
            (7, 1693238400, 'Problème de transport', 'justificativeAbsence_0489123456.pdf'),

            (8, 1690809600, 'Déplacement professionnel', 'justificativeAbsence_0467123456.pdf'),
            (8, 1691472000, 'Maladie', NULL),
            (8, 1689456000, 'Problème familial', 'justificativeAbsence_0467123456.pdf'),
            (8, 1692057600, 'Personnel', 'justificativeAbsence_0467123456.pdf'),
            (8, 1692649600, 'Conditions météo', NULL),
            (8, 1689849600, 'Maladie', 'justificativeAbsence_0467123456.pdf'),
            (8, 1693238400, 'Problème de transport', 'justificativeAbsence_0467123456.pdf'),

            (9, 1690809600, 'Maladie', 'justificativeAbsence_0478123456.pdf'),
            (9, 1691472000, 'Déplacement professionnel', NULL),
            (9, 1689456000, 'Personnel', 'justificativeAbsence_0478123456.pdf'),
            (9, 1692057600, 'Problème familial', 'justificativeAbsence_0478123456.pdf'),
            (9, 1692649600, 'Maladie', NULL),
            (9, 1689849600, 'Conditions météo', 'justificativeAbsence_0478123456.pdf'),
            (9, 1693238400, 'Problème de transport', 'justificativeAbsence_0478123456.pdf'),

            (10, 1690809600, 'Personnel', 'justificativeAbsence_0494123456.pdf'),
            (10, 1691472000, 'Problème familial', NULL),
            (10, 1689456000, 'Déplacement professionnel', 'justificativeAbsence_0494123456.pdf'),
            (10, 1692057600, 'Maladie', 'justificativeAbsence_0494123456.pdf'),
            (10, 1692649600, 'Personnel', NULL),
            (10, 1689849600, 'Conditions météo', 'justificativeAbsence_0494123456.pdf'),
            (10, 1693238400, 'Problème de transport', 'justificativeAbsence_0494123456.pdf');

            INSERT IGNORE INTO Mission (contractorId, description, date) VALUES
            (1, 'Inspection des installations électriques', 1688121600),
            (1, 'Réunion avec le client', 1688803200),
            (1, 'Rédaction du rapport de mission', 1689398400),
            (1, 'Suivi des travaux sur site', 1689984000),
            (1, 'Maintenance préventive', 1690560000),
            (1, 'Analyse des risques', 1691145600),
            (1, 'Formation interne sécurité', 1691731200),

            (2, 'Installation de serveurs réseau', 1688121600),
            (2, 'Test de performance', 1688803200),
            (2, 'Réunion équipe projet', 1689398400),
            (2, 'Mise à jour de documentation', 1689984000),
            (2, 'Support technique client', 1690560000),
            (2, 'Audit qualité', 1691145600),
            (2, 'Optimisation des processus', 1691731200),

            (3, 'Développement de nouvelle fonctionnalité', 1688121600),
            (3, 'Correction de bugs', 1688803200),
            (3, 'Revue de code', 1689398400),
            (3, 'Tests automatisés', 1689984000),
            (3, 'Déploiement en production', 1690560000),
            (3, 'Documentation technique', 1691145600),
            (3, 'Formation utilisateur final', 1691731200),

            (4, 'Gestion des stocks', 1688121600),
            (4, 'Commande de matériel', 1688803200),
            (4, 'Suivi des livraisons', 1689398400),
            (4, 'Réunion fournisseurs', 1689984000),
            (4, 'Analyse des coûts', 1690560000),
            (4, 'Mise à jour base de données', 1691145600),
            (4, 'Préparation rapport mensuel', 1691731200),

            (5, 'Audit de sécurité', 1688121600),
            (5, 'Évaluation des risques', 1688803200),
            (5, 'Mise à jour politique sécurité', 1689398400),
            (5, 'Formation des employés', 1689984000),
            (5, 'Inspection des équipements', 1690560000),
            (5, 'Réunion avec le comité', 1691145600),
            (5, 'Suivi conformité', 1691731200),

            (6, 'Test de produit', 1688121600),
            (6, 'Validation qualité', 1688803200),
            (6, 'Analyse résultats', 1689398400),
            (6, 'Réunion équipe R&D', 1689984000),
            (6, 'Préparation rapport', 1690560000),
            (6, 'Optimisation prototype', 1691145600),
            (6, 'Formation technique', 1691731200),

            (7, 'Campagne marketing', 1688121600),
            (7, 'Analyse marché', 1688803200),
            (7, 'Création supports', 1689398400),
            (7, 'Gestion réseaux sociaux', 1689984000),
            (7, 'Réunion équipe', 1690560000),
            (7, 'Suivi budget', 1691145600),
            (7, 'Événement client', 1691731200),

            (8, 'Support client', 1688121600),
            (8, 'Installation logiciel', 1688803200),
            (8, 'Formation utilisateur', 1689398400),
            (8, 'Maintenance système', 1689984000),
            (8, 'Mise à jour infrastructure', 1690560000),
            (8, 'Tests sécurité', 1691145600),
            (8, 'Réunion support', 1691731200),

            (9, 'Conception graphique', 1688121600),
            (9, 'Retouche photos', 1688803200),
            (9, 'Création maquettes', 1689398400),
            (9, 'Réunion client', 1689984000),
            (9, 'Présentation projet', 1690560000),
            (9,  'Mise à jour portfolio', 1691145600),
            (9, 'Formation logiciel', 1691731200),

            (10, 'Analyse financière', 1688121600),
            (10, 'Préparation budget', 1688803200),
            (10, 'Réunion comité', 1689398400),
            (10, 'Rapport trimestriel', 1689984000),
            (10, 'Suivi des dépenses', 1690560000),
            (10, 'Audit interne', 1691145600),
            (10, 'Formation gestion', 1691731200);

            INSERT IGNORE INTO PaidLeave (contractorId, startDate, endDate, reason) VALUES
            (1, 1685606400, 1686192000, 'Vacances annuelles'),
            (1, 1693564800, 1694035200, 'Congé parental'),
            (1, 1689379200, 1689638400, 'Congé maladie'),

            (2, 1685712000, 1686297600, 'Vacances annuelles'),
            (2, 1693459200, 1693929600, 'Congé maternité'),
            (2, 1689465600, 1689724800, 'Congé formation'),

            (3, 1685798400, 1686384000, 'Vacances annuelles'),
            (3, 1693353600, 1693824000, 'Congé parental'),
            (3, 1689552000, 1689811200, 'Congé maladie'),

            (4, 1685884800, 1686470400, 'Vacances annuelles'),
            (4, 1693248000, 1693718400, 'Congé formation'),
            (4, 1689638400, 1689897600, 'Congé maternité'),

            (5, 1685971200, 1686556800, 'Vacances annuelles'),
            (5, 1693142400, 1693612800, 'Congé parental'),
            (5, 1689724800, 1689984000, 'Congé maladie'),

            (6, 1686057600, 1686643200, 'Vacances annuelles'),
            (6, 1693036800, 1693507200, 'Congé maternité'),
            (6, 1689811200, 1690070400, 'Congé formation'),

            (7, 1686144000, 1686739200, 'Vacances annuelles'),
            (7, 1692931200, 1693401600, 'Congé parental'),
            (7, 1689897600, 1690156800, 'Congé maladie'),

            (8, 1686230400, 1686816000, 'Vacances annuelles'),
            (8, 1692825600, 1693296000, 'Congé formation'),
            (8, 1689984000, 1690243200, 'Congé maternité'),

            (9, 1686316800, 1686902400, 'Vacances annuelles'),
            (9, 1692720000, 1693190400, 'Congé parental'),
            (9, 1690070400, 1690329600, 'Congé maladie'),

            (10, 1686403200, 1686988800, 'Vacances annuelles'),
            (10, 1692614400, 1693084800, 'Congé formation'),
            (10, 1690156800, 1690416000, 'Congé maternité');

            INSERT IGNORE INTO Training (contractorId, type, address, formateur, date) VALUES
            (1, 'Sécurité électrique', 'Centre de formation Bruxelles', 'M. Dupuis', 1685702400),
            (1, 'Gestion de projet', 'Université de Bruxelles', 'Mme Lambert', 1688313600),
            (1, 'Logiciels bureautiques', 'Centre IT Bruxelles', 'M. Moreau', 1690924800),

            (2, 'Communication efficace', 'Centre Liège', 'Mme Dubois', 1685788800),
            (2, 'Gestion des conflits', 'Université Liège', 'M. Petit', 1688400000),
            (2, 'Anglais professionnel', 'Centre langues Liège', 'Mme Martin', 1691011200),

            (3, 'Développement web', 'Campus Anvers', 'M. Janssen', 1685875200),
            (3, 'Bases de données', 'Institut Tech Anvers', 'Mme Claes', 1688486400),
            (3, 'Cybersécurité', 'Centre Tech Anvers', 'M. Peeters', 1691097600),

            (4, 'Comptabilité', 'Centre Gand', 'Mme Verhoeven', 1685961600),
            (4, 'Fiscalité', 'Université Gand', 'M. Maes', 1688572800),
            (4, 'Audit financier', 'Institut Gand', 'Mme Willems', 1691184000),

            (5, 'Marketing digital', 'Centre Bruges', 'M. Jacobs', 1686048000),
            (5, 'Réseaux sociaux', 'Université Bruges', 'Mme De Smet', 1688659200),
            (5, 'Publicité en ligne', 'Centre médias Bruges', 'M. Martens', 1691270400),

            (6, 'Gestion RH', 'Centre Louvain', 'Mme Claes', 1686134400),
            (6, 'Droit du travail', 'Université Louvain', 'M. Claes', 1688745600),
            (6, 'Leadership', 'Institut Louvain', 'Mme Janssens', 1691356800),

            (7, 'Gestion de projet', 'Centre Courtrai', 'M. Claes', 1686220800),
            (7, 'Communication', 'Université Courtrai', 'Mme Willems', 1688832000),
            (7, 'Techniques de vente', 'Centre formation Courtrai', 'M. Dupont', 1691443200),

            (8, 'Support technique', 'Centre Anvers', 'Mme Maes', 1686307200),
            (8, 'Maintenance informatique', 'Université Anvers', 'M. Peeters', 1688918400),
            (8, 'Sécurité des systèmes', 'Institut Tech Anvers', 'Mme Vermeulen', 1691529600),

            (9, 'Design graphique', 'Centre Gand', 'Mme De Smet', 1686393600),
            (9, 'Photographie', 'Université Gand', 'M. Willems', 1689004800),
            (9, 'Illustration numérique', 'Institut Gand', 'Mme Maes', 1691616000),

            (10, 'Finance d’entreprise', 'Centre Bruxelles', 'M. Martens', 1686480000),
            (10, 'Audit interne', 'Université Bruxelles', 'Mme Dupont', 1689091200),
            (10, 'Analyse financière', 'Institut Bruxelles', 'M. Janssens', 1691702400);

            INSERT IGNORE INTO WorkTravel (contractorId, address, startDate, endDate, description) VALUES
            (1, 'Bruxelles', 1685702400, 1686307200, 'Initial Visit'),
            (1, 'Liège', 1688313600, 1688918400, 'Follow-up Inspection'),
            (1, 'Anvers', 1690924800, 1691529600, 'Final Review'),

            (2, 'Gand', 1685788800, 1686393600, 'Initial Visit'),
            (2, 'Bruges', 1688400000, 1689004800, 'Follow-up Inspection'),
            (2, 'Louvain', 1691011200, 1691616000, 'Final Review'),

            (3, 'Courtrai', 1685875200, 1686480000, 'Initial Visit'),
            (3, 'Namur', 1688486400, 1689091200, 'Follow-up Inspection'),
            (3, 'Charleroi', 1691097600, 1691702400, 'Final Review'),

            (4, 'Hasselt', 1685961600, 1686566400, 'Initial Visit'),
            (4, 'Mons', 1688572800, 1689177600, 'Follow-up Inspection'),
            (4, 'Bruxelles', 1691184000, 1691788800, 'Final Review'),

            (5, 'Liège', 1686048000, 1686652800, 'Initial Visit'),
            (5, 'Anvers', 1688659200, 1689264000, 'Follow-up Inspection'),
            (5, 'Gand', 1691270400, 1691875200, 'Final Review'),

            (6, 'Bruges', 1686134400, 1686739200, 'Initial Visit'),
            (6, 'Louvain', 1688745600, 1689350400, 'Follow-up Inspection'),
            (6, 'Namur', 1691356800, 1691961600, 'Final Review'),

            (7, 'Charleroi', 1686220800, 1686825600, 'Initial Visit'),
            (7, 'Hasselt', 1688832000, 1689436800, 'Follow-up Inspection'),
            (7, 'Mons', 1691443200, 1692048000, 'Final Review'),

            (8, 'Bruxelles', 1686307200, 1686912000, 'Initial Visit'),
            (8, 'Liège', 1688918400, 1689523200, 'Follow-up Inspection'),
            (8, 'Anvers', 1691529600, 1692134400, 'Final Review'),

            (9, 'Gand', 1686393600, 1686998400, 'Initial Visit'),
            (9, 'Bruges', 1689004800, 1689609600, 'Follow-up Inspection'),
            (9, 'Louvain', 1691616000, 1692220800, 'Final Review'),

            (10, 'Courtrai', 1686480000, 1687084800, 'Initial Visit'),
            (10, 'Namur', 1689091200, 1689696000, 'Follow-up Inspection'),
            (10, 'Charleroi', 1691702400, 1692307200, 'Final Review');";

        public const string generatePayslips =@"
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
            SELECT contractorId,
                COUNT(DISTINCT FROM_UNIXTIME(`date`, '%Y-%m-%d')) AS total_abs_days
            FROM Absences
            WHERE `date` BETWEEN UNIX_TIMESTAMP(DATE_FORMAT(CURRENT_DATE, '%Y-%m-01')) AND UNIX_TIMESTAMP(CURRENT_DATE)
            GROUP BY contractorId
            ) abs ON abs.contractorId = c.contractorId
            WHERE j.name IN ('Employé', 'Consultant');
            ";
        /*
            const string skeleton
            const string debugScript (ce script est pour tester l'application en mode debug)

            This constant string contains the structure of the application mysql database.
            Set values:
                SQL Code (CREATE TABLE IF NOT EXISTS)
        */

        // Program constants
        public const string sourceMySQL = "MySQLController";
        public const string sourceProgram = "Program";
        public const string sourceDataset = "Dataset";
        public const string sourceMethodes = "Methodes";
        public const string sourceApplicationController = "ApplicationController";
        public const string errorMessage = "Un erreur est survenue. L'opération n'a pas pu aboutir. Veuillez réessayer ultérieurement ou contacter notre support technique en cas de besoin.";
    }
}