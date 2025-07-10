
namespace Gestionnaire
{
    public interface ITestsAgent
    {
        public string RunTest(int testid);
    }

    public class TestsAgent : ITestsAgent
    {
        public string RunTest(int testid)
        {
            string outputString = testid switch
            {
                0 => "admin",
                1 => "admin",
                2 => "8", // Préparation du base des données
                3 => "2",
                4 => "1",
                5 => "X",
                6 => "1", // Absences
                7 => "Marie Dupont",
                8 => "02/01/2025",
                9 => "1",
                10 => "Marie Dupont",
                11 => " ",
                12 => "30",
                13 => "2", // Vacances
                14 => "Sophie Lambert",
                15 => " ",
                16 => "30",
                17 => "2",
                18 => "Sophie Lambert",
                19 => "05/11/2025",
                20 => "3", // Formation
                21 => "Camille Gerard",
                22 => " ",
                23 => "30",
                24 => "3",
                25 => "Camille Gerard",
                26 => "12/04/2025",
                27 => "4", // Mission
                28 => "Elodie Simon",
                29 => " ",
                30 => "30",
                31 => "4",
                32 => "Elodie Simon",
                33 => "09/11/2025",
                34 => "5", // Déplacement
                35 => "Luc Moreau",
                36 => " ",
                37 => "30",
                38 => "5",
                39 => "Luc Moreau",
                40 => "05/11/2025",
                41 => "6", // Paramètres administration
                42 => Config.adminSettingsPIN.ToString(),
                43 => "1",
                44 => "1",
                45 => "2",
                46 => "Testeur",
                47 => "5",
                48 => "3",
                49 => "Testeur",
                50 => "1",
                51 => "Beta-Test",
                52 => "6",
                53 => "1",
                54 => "3",
                55 => "Beta-Test",
                56 => "2",
                57 => "OUI",
                58 => "4",
                59 => "2",
                60 => "1",
                61 => "Yassine Bouelkheir",
                62 => "0600000000",
                63 => "yassinebouelkheir@gmail.com",
                64 => "Rue du exemple 31, 1000 Exemple",
                65 => "",
                66 => "25",
                67 => "2500",
                68 => "Employé",
                69 => "2",
                70 => "2",
                71 => "Yassine Bouelkheir",
                72 => "2",
                73 => "01/07/2025",
                74 => "2",
                75 => "Yassine Bouelkheir",
                76 => "4",
                77 => "01/10/2025",
                78 => "Odoo",
                79 => "Informatique Industrielle",
                80 => "Rue de exemple 10, 1000 Exemple",
                81 => "2",
                82 => "Yassine Bouelkheir",
                83 => "5",
                84 => "01/10/2025",
                85 => "03/10/2025",
                86 => "Congé annuel",
                _ => string.Empty,
            };
            return outputString;
        }
    }
}

