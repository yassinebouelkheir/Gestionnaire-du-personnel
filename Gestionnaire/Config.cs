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
        
        public const string serverAddress = "192.168.1.1";
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
        
        public const string skeleton = @"
            CREATE TABLE IF NOT EXISTS `Absences` (
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
            `startDate` int(11) NOT NULL,
            `endDate` int(11) DEFAULT NULL,
            `hours` int(11) NOT NULL DEFAULT 8,
            `salary` double NOT NULL,
            `fonction` int(11) NOT NULL,
            PRIMARY KEY (`contractorId`)
            );

            CREATE TABLE IF NOT EXISTS `Fonctions` (
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
            );";

        public const string debugScript = @"
            INSERT IGNORE INTO `Contracts` (`contractorId`, `fullname`, `gsm`, `email`, `address`, `startDate`, `endDate`, `hours`, `salary`, `fonction`) VALUES
            (1, 'test', '0000000000', 'test@gmail.com', 'test', 1749217847, NULL, 40, 3000, 3);

            INSERT IGNORE INTO `Absences` (`id`, `contractorId`, `date`, `reason`, `justificativeDocument`) VALUES
            (1, 1, 1748799000, NULL, NULL);

            INSERT IGNORE INTO `Fonctions` (`id`, `authorityLevel`, `name`) VALUES
            (1, 1, 'Consultant'),
            (2, 2, 'Ouvrier'),
            (3, 3, 'Employé');

            INSERT IGNORE INTO `Mission` (`contractorId`, `description`, `date`) VALUES
            (1, 'Envoi exceptionel', 1749217847);

            INSERT IGNORE INTO `PaidLeave` (`Id`, `contractorId`, `startDate`, `endDate`, `reason`) VALUES
            (1, 1, 1749254400, 1749772800, 'test');

            INSERT IGNORE INTO `Users` (`id`, `username`, `password_hash`, `salt`) VALUES
            (1, 'admin', '24euMIjLiutdFt52gv/nIsNi8OKtyMcHEGH3WnYfvTI=', '6GiAEASB7JnuM3SjrG6Hag==');

            INSERT IGNORE INTO `WorkTravel` (`id`, `contractorId`, `startDate`, `endDate`, `address`, `description`) VALUES
            (1, 1, 1749513600, 1749772800, 'test, test', 'test description');
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