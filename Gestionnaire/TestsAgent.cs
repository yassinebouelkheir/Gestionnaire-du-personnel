
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
                5 => "3",
                6 => "X",
                7 => "1", // Absences
                8 => "Marie Dupont",
                9 => "02/01/2025",
                10 => "1",
                11 => "Marie Dupont",
                12 => " ",
                13 => "30",
                14 => "2", // Vacances
                15 => "Sophie Lambert",
                16 => " ",
                17 => "30",
                18 => "2",
                19 => "Sophie Lambert",
                20 => "05/11/2025",
                21 => "3", // Formation
                22 => "Camille Gerard",
                23 => " ",
                24 => "30",
                25 => "3",
                26 => "Camille Gerard",
                27 => "12/04/2025",
                28 => "4", // Mission
                29 => "Elodie Simon",
                30 => " ",
                31 => "30",
                32 => "4",
                33 => "Elodie Simon",
                34 => "09/11/2025",
                35 => "5", // Déplacement
                36 => "Luc Moreau",
                37 => " ",
                38 => "30",
                39 => "5",
                40 => "Luc Moreau",
                41 => "05/11/2025",
                42 => "6", // Paramètres administration
                43 => Config.adminSettingsPIN.ToString(),
                44 => "1",
                45 => "1",
                46 => "2",
                47 => "Testeur",
                48 => "5",
                49 => "3",
                50 => "Testeur",
                51 => "1",
                52 => "Beta-Test",
                53 => "6",
                54 => "1",
                55 => "3",
                56 => "Beta-Test",
                57 => "2",
                58 => "OUI",
                59 => "4",
                _ => string.Empty,
            };
            return outputString;
        }
    }
}

