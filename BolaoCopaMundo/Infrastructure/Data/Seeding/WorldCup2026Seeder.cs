using BolaoCopaMundo.Domain.Entities;
using BolaoCopaMundo.Domain.Enums;
using BolaoCopaMundo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BolaoCopaMundo.Infrastructure.Data.Seeding;

/// <summary>
/// Seed oficial da Copa do Mundo 2026 (EUA/Canadá/México)
/// Grupos e calendário baseados no sorteio de 5 de dezembro de 2025.
/// Todos os horários em UTC.
/// </summary>
public class WorldCup2026Seeder(AppDbContext context, ILogger<WorldCup2026Seeder> logger)
{
    public async Task SeedAsync()
    {
        if (await context.Groups.AnyAsync())
        {
            logger.LogInformation("Dados da Copa 2026 já existem. Pulando seed.");
            return;
        }

        logger.LogInformation("Iniciando seed da Copa do Mundo 2026...");

        var groups = CreateGroups();
        await context.Groups.AddRangeAsync(groups);
        await context.SaveChangesAsync();

        var teams = CreateTeams(groups);
        await context.Teams.AddRangeAsync(teams);
        await context.SaveChangesAsync();

        var matches = CreateMatches(groups, teams);
        await context.Matches.AddRangeAsync(matches);
        await context.SaveChangesAsync();

        logger.LogInformation("Seed concluído: {G} grupos, {T} times, {M} jogos da fase de grupos.",
            groups.Count, teams.Count, matches.Count);
    }

    private static List<Group> CreateGroups() =>
        Enumerable.Range(0, 12)
            .Select(i => new Group { Name = ((char)('A' + i)).ToString() })
            .ToList();

    private static List<Team> CreateTeams(List<Group> groups)
    {
        // Sorteio oficial — 5 de dezembro de 2025, Kennedy Center, Washington D.C.
        // (Nome PT-BR, código FIFA, grupo)
        var data = new[]
        {
            // ── Grupo A ─────────────────────────────────────────────
            ("México",               "MEX", "A"),
            ("África do Sul",        "RSA", "A"),
            ("Coreia do Sul",        "KOR", "A"),
            ("República Tcheca",     "CZE", "A"),
            // ── Grupo B ─────────────────────────────────────────────
            ("Canadá",               "CAN", "B"),
            ("Bósnia-Herzegovina",   "BIH", "B"),
            ("Qatar",                "QAT", "B"),
            ("Suíça",                "SUI", "B"),
            // ── Grupo C ─────────────────────────────────────────────
            ("Brasil",               "BRA", "C"),
            ("Marrocos",             "MAR", "C"),
            ("Haiti",                "HTI", "C"),
            ("Escócia",              "SCO", "C"),
            // ── Grupo D ─────────────────────────────────────────────
            ("Estados Unidos",       "USA", "D"),
            ("Paraguai",             "PAR", "D"),
            ("Austrália",            "AUS", "D"),
            ("Turquia",              "TUR", "D"),
            // ── Grupo E ─────────────────────────────────────────────
            ("Alemanha",             "GER", "E"),
            ("Curaçao",              "CUW", "E"),
            ("Costa do Marfim",      "CIV", "E"),
            ("Equador",              "ECU", "E"),
            // ── Grupo F ─────────────────────────────────────────────
            ("Países Baixos",        "NED", "F"),
            ("Japão",                "JPN", "F"),
            ("Suécia",               "SWE", "F"),
            ("Tunísia",              "TUN", "F"),
            // ── Grupo G ─────────────────────────────────────────────
            ("Bélgica",              "BEL", "G"),
            ("Egito",                "EGY", "G"),
            ("Irã",                  "IRN", "G"),
            ("Nova Zelândia",        "NZL", "G"),
            // ── Grupo H ─────────────────────────────────────────────
            ("Espanha",              "ESP", "H"),
            ("Cabo Verde",           "CPV", "H"),
            ("Arábia Saudita",       "KSA", "H"),
            ("Uruguai",              "URU", "H"),
            // ── Grupo I ─────────────────────────────────────────────
            ("França",               "FRA", "I"),
            ("Senegal",              "SEN", "I"),
            ("Iraque",               "IRQ", "I"),
            ("Noruega",              "NOR", "I"),
            // ── Grupo J ─────────────────────────────────────────────
            ("Argentina",            "ARG", "J"),
            ("Argélia",              "ALG", "J"),
            ("Áustria",              "AUT", "J"),
            ("Jordânia",             "JOR", "J"),
            // ── Grupo K ─────────────────────────────────────────────
            ("Portugal",             "POR", "K"),
            ("Congo (RD)",           "COD", "K"),
            ("Uzbequistão",          "UZB", "K"),
            ("Colômbia",             "COL", "K"),
            // ── Grupo L ─────────────────────────────────────────────
            ("Inglaterra",           "ENG", "L"),
            ("Croácia",              "CRO", "L"),
            ("Gana",                 "GHA", "L"),
            ("Panamá",               "PAN", "L"),
        };

        // Mapeamento FIFA code → ISO 3166-1 alpha-2 para flagcdn.com
        // Scotland e England usam subdivisões do Reino Unido
        var isoMap = new Dictionary<string, string>
        {
            ["MEX"] = "mx", ["RSA"] = "za", ["KOR"] = "kr", ["CZE"] = "cz",
            ["CAN"] = "ca", ["BIH"] = "ba", ["QAT"] = "qa", ["SUI"] = "ch",
            ["BRA"] = "br", ["MAR"] = "ma", ["HTI"] = "ht", ["SCO"] = "gb-sct",
            ["USA"] = "us", ["PAR"] = "py", ["AUS"] = "au", ["TUR"] = "tr",
            ["GER"] = "de", ["CUW"] = "cw", ["CIV"] = "ci", ["ECU"] = "ec",
            ["NED"] = "nl", ["JPN"] = "jp", ["SWE"] = "se", ["TUN"] = "tn",
            ["BEL"] = "be", ["EGY"] = "eg", ["IRN"] = "ir", ["NZL"] = "nz",
            ["ESP"] = "es", ["CPV"] = "cv", ["KSA"] = "sa", ["URU"] = "uy",
            ["FRA"] = "fr", ["SEN"] = "sn", ["IRQ"] = "iq", ["NOR"] = "no",
            ["ARG"] = "ar", ["ALG"] = "dz", ["AUT"] = "at", ["JOR"] = "jo",
            ["POR"] = "pt", ["COD"] = "cd", ["UZB"] = "uz", ["COL"] = "co",
            ["ENG"] = "gb-eng", ["CRO"] = "hr", ["GHA"] = "gh", ["PAN"] = "pa",
        };

        var groupDict = groups.ToDictionary(g => g.Name);
        return data.Select(t => new Team
        {
            Name     = t.Item1,
            FifaCode = t.Item2,
            GroupId  = groupDict[t.Item3].Id,
            FlagUrl  = isoMap.TryGetValue(t.Item2, out var iso)
                         ? $"https://flagcdn.com/{iso}.svg"
                         : null
        }).ToList();
    }

    private static List<Match> CreateMatches(List<Group> groups, List<Team> teams)
    {
        var teamByCode = teams.ToDictionary(t => t.FifaCode);
        var groupByName = groups.ToDictionary(g => g.Name);

        // (Casa, Fora, Grupo, Data UTC, Estádio, Rodada)
        // Horários convertidos de Brasília (UTC-3) para UTC
        var fixtures = new[]
        {
            // ═══════════════════════════════════════════════════════
            // RODADA 1  (11–17 jun)
            // ═══════════════════════════════════════════════════════

            // Grupo A
            ("MEX","RSA","A", new DateTime(2026,6,11,19,0,0,DateTimeKind.Utc), "Estadio Azteca, Cidade do México",       1),
            ("KOR","CZE","A", new DateTime(2026,6,12, 2,0,0,DateTimeKind.Utc), "Estadio Akron, Guadalajara",             1),

            // Grupo B
            ("CAN","BIH","B", new DateTime(2026,6,12,19,0,0,DateTimeKind.Utc), "BMO Field, Toronto",                    1),
            ("QAT","SUI","B", new DateTime(2026,6,13,19,0,0,DateTimeKind.Utc), "Levi's Stadium, San Francisco",         1),

            // Grupo D
            ("USA","PAR","D", new DateTime(2026,6,13, 1,0,0,DateTimeKind.Utc), "SoFi Stadium, Los Angeles",             1),
            ("AUS","TUR","D", new DateTime(2026,6,13, 4,0,0,DateTimeKind.Utc), "BC Place, Vancouver",                   1),

            // Grupo C
            ("BRA","MAR","C", new DateTime(2026,6,13,22,0,0,DateTimeKind.Utc), "MetLife Stadium, Nova York/NJ",         1),
            ("HTI","SCO","C", new DateTime(2026,6,14, 1,0,0,DateTimeKind.Utc), "Gillette Stadium, Boston",              1),

            // Grupo E
            ("GER","CUW","E", new DateTime(2026,6,14,17,0,0,DateTimeKind.Utc), "NRG Stadium, Houston",                  1),
            ("CIV","ECU","E", new DateTime(2026,6,14,23,0,0,DateTimeKind.Utc), "Lincoln Financial Field, Philadelphia", 1),

            // Grupo F
            ("NED","JPN","F", new DateTime(2026,6,14,20,0,0,DateTimeKind.Utc), "AT&T Stadium, Dallas",                  1),
            ("SWE","TUN","F", new DateTime(2026,6,15, 2,0,0,DateTimeKind.Utc), "Estadio BBVA, Monterrey",               1),

            // Grupo H
            ("ESP","CPV","H", new DateTime(2026,6,15,16,0,0,DateTimeKind.Utc), "Mercedes-Benz Stadium, Atlanta",        1),
            ("KSA","URU","H", new DateTime(2026,6,15,22,0,0,DateTimeKind.Utc), "Hard Rock Stadium, Miami",              1),

            // Grupo G
            ("BEL","EGY","G", new DateTime(2026,6,15,19,0,0,DateTimeKind.Utc), "Lumen Field, Seattle",                  1),
            ("IRN","NZL","G", new DateTime(2026,6,16, 1,0,0,DateTimeKind.Utc), "SoFi Stadium, Los Angeles",             1),

            // Grupo J
            ("AUT","JOR","J", new DateTime(2026,6,16, 4,0,0,DateTimeKind.Utc), "Levi's Stadium, San Francisco",         1),
            ("ARG","ALG","J", new DateTime(2026,6,17, 1,0,0,DateTimeKind.Utc), "Arrowhead Stadium, Kansas City",        1),

            // Grupo I
            ("FRA","SEN","I", new DateTime(2026,6,16,19,0,0,DateTimeKind.Utc), "MetLife Stadium, Nova York/NJ",         1),
            ("IRQ","NOR","I", new DateTime(2026,6,16,22,0,0,DateTimeKind.Utc), "Gillette Stadium, Boston",              1),

            // Grupo K
            ("POR","COD","K", new DateTime(2026,6,17,17,0,0,DateTimeKind.Utc), "NRG Stadium, Houston",                  1),
            ("UZB","COL","K", new DateTime(2026,6,18, 2,0,0,DateTimeKind.Utc), "Estadio Azteca, Cidade do México",      1),

            // Grupo L
            ("ENG","CRO","L", new DateTime(2026,6,17,20,0,0,DateTimeKind.Utc), "AT&T Stadium, Dallas",                  1),
            ("GHA","PAN","L", new DateTime(2026,6,17,23,0,0,DateTimeKind.Utc), "BMO Field, Toronto",                    1),

            // ═══════════════════════════════════════════════════════
            // RODADA 2  (18–23 jun)
            // ═══════════════════════════════════════════════════════

            // Grupo A
            ("CZE","RSA","A", new DateTime(2026,6,18,16,0,0,DateTimeKind.Utc), "Mercedes-Benz Stadium, Atlanta",        2),
            ("MEX","KOR","A", new DateTime(2026,6,19, 1,0,0,DateTimeKind.Utc), "Estadio Akron, Guadalajara",            2),

            // Grupo B
            ("SUI","BIH","B", new DateTime(2026,6,18,19,0,0,DateTimeKind.Utc), "SoFi Stadium, Los Angeles",             2),
            ("CAN","QAT","B", new DateTime(2026,6,18,22,0,0,DateTimeKind.Utc), "BC Place, Vancouver",                   2),

            // Grupo D
            ("TUR","PAR","D", new DateTime(2026,6,19, 4,0,0,DateTimeKind.Utc), "Levi's Stadium, San Francisco",         2),
            ("USA","AUS","D", new DateTime(2026,6,19,19,0,0,DateTimeKind.Utc), "Lumen Field, Seattle",                  2),

            // Grupo C
            ("SCO","MAR","C", new DateTime(2026,6,19,22,0,0,DateTimeKind.Utc), "Gillette Stadium, Boston",              2),
            ("BRA","HTI","C", new DateTime(2026,6,20, 0,30,0,DateTimeKind.Utc),"Lincoln Financial Field, Philadelphia", 2),

            // Grupo F
            ("TUN","JPN","F", new DateTime(2026,6,20, 4,0,0,DateTimeKind.Utc), "Estadio BBVA, Monterrey",               2),
            ("NED","SWE","F", new DateTime(2026,6,20,17,0,0,DateTimeKind.Utc), "NRG Stadium, Houston",                  2),

            // Grupo E
            ("GER","CIV","E", new DateTime(2026,6,20,20,0,0,DateTimeKind.Utc), "BMO Field, Toronto",                    2),
            ("ECU","CUW","E", new DateTime(2026,6,21, 0,0,0,DateTimeKind.Utc), "Arrowhead Stadium, Kansas City",        2),

            // Grupo H
            ("ESP","KSA","H", new DateTime(2026,6,21,16,0,0,DateTimeKind.Utc), "Mercedes-Benz Stadium, Atlanta",        2),
            ("URU","CPV","H", new DateTime(2026,6,21,22,0,0,DateTimeKind.Utc), "Hard Rock Stadium, Miami",              2),

            // Grupo G
            ("BEL","IRN","G", new DateTime(2026,6,21,19,0,0,DateTimeKind.Utc), "SoFi Stadium, Los Angeles",             2),
            ("NZL","EGY","G", new DateTime(2026,6,22, 1,0,0,DateTimeKind.Utc), "BC Place, Vancouver",                   2),

            // Grupo J
            ("JOR","ALG","J", new DateTime(2026,6,22, 3,0,0,DateTimeKind.Utc), "Levi's Stadium, San Francisco",         2),
            ("ARG","AUT","J", new DateTime(2026,6,22,17,0,0,DateTimeKind.Utc), "AT&T Stadium, Dallas",                  2),

            // Grupo I
            ("FRA","IRQ","I", new DateTime(2026,6,22,21,0,0,DateTimeKind.Utc), "Lincoln Financial Field, Philadelphia", 2),
            ("NOR","SEN","I", new DateTime(2026,6,23, 0,0,0,DateTimeKind.Utc), "MetLife Stadium, Nova York/NJ",         2),

            // Grupo K
            ("POR","UZB","K", new DateTime(2026,6,23,17,0,0,DateTimeKind.Utc), "NRG Stadium, Houston",                  2),
            ("COL","COD","K", new DateTime(2026,6,24, 2,0,0,DateTimeKind.Utc), "Estadio Akron, Guadalajara",            2),

            // Grupo L
            ("ENG","GHA","L", new DateTime(2026,6,23,20,0,0,DateTimeKind.Utc), "Gillette Stadium, Boston",              2),
            ("PAN","CRO","L", new DateTime(2026,6,23,23,0,0,DateTimeKind.Utc), "BMO Field, Toronto",                    2),

            // ═══════════════════════════════════════════════════════
            // RODADA 3  (24–27 jun) — jogos simultâneos por grupo
            // ═══════════════════════════════════════════════════════

            // Grupo B — simultâneos
            ("SUI","CAN","B", new DateTime(2026,6,24,19,0,0,DateTimeKind.Utc), "BC Place, Vancouver",                   3),
            ("BIH","QAT","B", new DateTime(2026,6,24,19,0,0,DateTimeKind.Utc), "Lumen Field, Seattle",                  3),

            // Grupo C — simultâneos
            ("SCO","BRA","C", new DateTime(2026,6,24,22,0,0,DateTimeKind.Utc), "Hard Rock Stadium, Miami",              3),
            ("MAR","HTI","C", new DateTime(2026,6,24,22,0,0,DateTimeKind.Utc), "Mercedes-Benz Stadium, Atlanta",        3),

            // Grupo A — simultâneos
            ("CZE","MEX","A", new DateTime(2026,6,25, 1,0,0,DateTimeKind.Utc), "Estadio Azteca, Cidade do México",      3),
            ("RSA","KOR","A", new DateTime(2026,6,25, 1,0,0,DateTimeKind.Utc), "Estadio BBVA, Monterrey",               3),

            // Grupo E — simultâneos
            ("CUW","CIV","E", new DateTime(2026,6,25,20,0,0,DateTimeKind.Utc), "Lincoln Financial Field, Philadelphia", 3),
            ("ECU","GER","E", new DateTime(2026,6,25,20,0,0,DateTimeKind.Utc), "MetLife Stadium, Nova York/NJ",         3),

            // Grupo F — simultâneos
            ("JPN","SWE","F", new DateTime(2026,6,25,23,0,0,DateTimeKind.Utc), "AT&T Stadium, Dallas",                  3),
            ("TUN","NED","F", new DateTime(2026,6,25,23,0,0,DateTimeKind.Utc), "Arrowhead Stadium, Kansas City",        3),

            // Grupo D — simultâneos
            ("TUR","USA","D", new DateTime(2026,6,26, 2,0,0,DateTimeKind.Utc), "SoFi Stadium, Los Angeles",             3),
            ("PAR","AUS","D", new DateTime(2026,6,26, 2,0,0,DateTimeKind.Utc), "Levi's Stadium, San Francisco",         3),

            // Grupo I — simultâneos
            ("NOR","FRA","I", new DateTime(2026,6,26,19,0,0,DateTimeKind.Utc), "Gillette Stadium, Boston",              3),
            ("SEN","IRQ","I", new DateTime(2026,6,26,19,0,0,DateTimeKind.Utc), "BMO Field, Toronto",                    3),

            // Grupo H — simultâneos
            ("CPV","KSA","H", new DateTime(2026,6,27, 0,0,0,DateTimeKind.Utc), "NRG Stadium, Houston",                  3),
            ("URU","ESP","H", new DateTime(2026,6,27, 0,0,0,DateTimeKind.Utc), "Estadio Akron, Guadalajara",            3),

            // Grupo G — simultâneos
            ("EGY","IRN","G", new DateTime(2026,6,27, 3,0,0,DateTimeKind.Utc), "Lumen Field, Seattle",                  3),
            ("NZL","BEL","G", new DateTime(2026,6,27, 3,0,0,DateTimeKind.Utc), "BC Place, Vancouver",                   3),

            // Grupo L — simultâneos
            ("PAN","ENG","L", new DateTime(2026,6,27,21,0,0,DateTimeKind.Utc), "MetLife Stadium, Nova York/NJ",         3),
            ("CRO","GHA","L", new DateTime(2026,6,27,21,0,0,DateTimeKind.Utc), "Lincoln Financial Field, Philadelphia", 3),

            // Grupo K — simultâneos
            ("COL","POR","K", new DateTime(2026,6,27,23,30,0,DateTimeKind.Utc),"Hard Rock Stadium, Miami",              3),
            ("COD","UZB","K", new DateTime(2026,6,27,23,30,0,DateTimeKind.Utc),"Mercedes-Benz Stadium, Atlanta",        3),

            // Grupo J — simultâneos
            ("ALG","AUT","J", new DateTime(2026,6,28, 2,0,0,DateTimeKind.Utc), "Arrowhead Stadium, Kansas City",        3),
            ("JOR","ARG","J", new DateTime(2026,6,28, 2,0,0,DateTimeKind.Utc), "AT&T Stadium, Dallas",                  3),
        };

        return fixtures.Select(f =>
        {
            var home  = teamByCode[f.Item1];
            var away  = teamByCode[f.Item2];
            var grp   = groupByName[f.Item3];
            return new Match
            {
                HomeTeamId = home.Id,
                AwayTeamId = away.Id,
                GroupId    = grp.Id,
                Phase      = MatchPhase.GroupStage,
                Status     = MatchStatus.Scheduled,
                Matchday   = f.Item6,
                MatchDate  = f.Item4,
                Venue      = f.Item5,
                MatchLabel = $"Grupo {f.Item3} - Rodada {f.Item6}: {home.Name} x {away.Name}"
            };
        }).ToList();
    }
}
