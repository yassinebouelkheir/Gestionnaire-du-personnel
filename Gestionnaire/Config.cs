namespace Gestionnaire
{
    class Config
    {
        // Program configuration
        public const bool productionRun = true;
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
        
        public static int consoleDateTime = 2;
        /*
            const int consoleDateTime

            This constant set the way the date is formatted in the console.
            Set values:
                0 to disable the datetime completely
                1 for yyyy/MM/dd HH:mm:ss
                2 for HH:mm:ss
                3 for yyyy/MM/dd
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
            `date` date NOT NULL,
            `reason` varchar(128) DEFAULT NULL,
            `justificativeDocument` varchar(128) DEFAULT NULL,
            PRIMARY KEY (`id`)
            ) ENGINE=MyISAM DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci;


            CREATE TABLE IF NOT EXISTS `Contracts` (
            `contractorId` int(11) NOT NULL AUTO_INCREMENT,
            `fullname` varchar(64) NOT NULL,
            `gsm` varchar(16) NOT NULL,
            `email` varchar(32) NOT NULL,
            `address` text NOT NULL,
            `startDate` date NOT NULL,
            `endDate` date DEFAULT NULL,
            `hours` int(11) DEFAULT NULL,
            `salary` double NOT NULL,
            `type` varchar(32) NOT NULL,
            `locality` varchar(64) NOT NULL,
            `responsableId` int(11) NOT NULL,
            `signedDocument` varchar(128) NOT NULL,
            PRIMARY KEY (`contractorId`)
            ) ENGINE=MyISAM DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci;

            CREATE TABLE IF NOT EXISTS `Mission` (
            `id` int(11) NOT NULL,
            `contractorId` int(11) NOT NULL,
            `type` varchar(128) NOT NULL,
            `address` varchar(256) NOT NULL,
            `description` varchar(256) NOT NULL,
            `date` date NOT NULL,
            PRIMARY KEY (`id`)
            ) ENGINE=MyISAM DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci;

            CREATE TABLE IF NOT EXISTS `PaidLeave` (
            `Id` int(11) NOT NULL AUTO_INCREMENT,
            `contractorId` int(11) NOT NULL,
            `startDate` date NOT NULL,
            `endDate` date NOT NULL,
            `reason` varchar(128) NOT NULL,
            PRIMARY KEY (`Id`)
            ) ENGINE=MyISAM DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci;

            CREATE TABLE IF NOT EXISTS `Training` (
            `id` int(11) NOT NULL AUTO_INCREMENT,
            `contractorId` int(11) NOT NULL,
            `type` varchar(128) NOT NULL,
            `address` varchar(256) NOT NULL,
            `formateur` varchar(128) NOT NULL,
            `date` date NOT NULL,
            PRIMARY KEY (`id`)
            ) ENGINE=MyISAM DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci;

            CREATE TABLE IF NOT EXISTS `Utilisateurs` (
            `id` int(11) NOT NULL AUTO_INCREMENT,
            `username` varchar(32) NOT NULL,
            `password_hash` varchar(256) NOT NULL,
            `salt` varchar(32) NOT NULL,
            PRIMARY KEY (`id`)
            ) ENGINE=MyISAM DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci;

            CREATE TABLE IF NOT EXISTS `WorkTravel` (
            `id` int(11) NOT NULL,
            `contractorId` int(11) NOT NULL,
            `startDate` date NOT NULL,
            `endDate` date NOT NULL,
            `address` varchar(128) NOT NULL,
            `description` varchar(128) NOT NULL,
            PRIMARY KEY (`id`)
            ) ENGINE=MyISAM DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci;
            ";
        /*
            const string skeleton

            This constant string contains the structure of the application mysql database.
            Set values:
                SQL Code (CREATE TABLE, VIEWS)
        */

        // Program constants
        public const string sourceMySQL = "MySQLController";
        public const string sourceProgram = "Program";
        public const string sourceDataset = "Dataset";
        public const string sourceMethodes = "Methodes";
        public const string sourceApplicationController = "ApplicationController";
        public const string errorMessage = "Une erreur est survenue. L'opération n'a pas pu aboutir. Veuillez réessayer ultérieurement ou contacter notre support technique en cas de besoin.";
        public const int maxLoginAttempts = 3;
    }
}