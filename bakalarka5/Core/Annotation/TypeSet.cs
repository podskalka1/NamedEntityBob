using System.Collections.Generic;

namespace bakalarka5.Core.Annotation;

public static class TypeSet
{
    public static readonly NeType NumbersType = new NeType("A", "[A] Čísla v Adresách");
    public static readonly List<NeType> Numbers =
    [
        new NeType("ah","[ah] Čísla ulíc"),
        new NeType("at","[at] Telefónne čísla"),
        new NeType("az","[az] PSČ")
    ];

    public static readonly NeType PlacesType = new NeType("G", "[G] Zemepisné Názvy"); 
    public static readonly List<NeType> Places =
    [
        new NeType("gc", "[gc] Štáty"),
        new NeType("gh", "[gh] Hydronymá"),
        new NeType("gl", "[gl] Prírodné útvary, ostrovy"),
        new NeType("gm", "[gm] Fiktívne, mýtické lokality"),
        new NeType("gq", "[gq] Mestské časti"),
        new NeType("gp", "[gp] Parky, cintoríny, záhrady"),
        new NeType("gr", "[gr] Územné útvary"),
        new NeType("gs", "[gs] Ulice, námestia"),
        new NeType("gt", "[gt] Geografické regióny, kontinenty"),
        new NeType("gu", "[gu] Mestá, obce"),
        new NeType("g_", "[g_] Unspecified")
    ];
    
    public static readonly NeType InstitutionType = new NeType("I", "[I] Inštitúcie");
    public static readonly List<NeType> Institutions =
    [
        new NeType("ia", "[ia] Konferencie, súťaže, festivaly"),
        new NeType("ic", "[ic] Inštitúcie, firmy, organizácie"),
        new NeType("ig", "[ig] Hudobné skupiny, kluby"),
        new NeType("io", "[io] Vládne, politické, vojenské"),
        new NeType("i_", "[i_] Unspecified")
    ];
    
    public static readonly NeType MediaType = new NeType("M", "[M] Médiá");
    public static readonly List<NeType> Media =
    [
        new NeType("me", "[me] e-mailové adresy"),
        new NeType("mi", "[mi] Internetové odkazy"),
        new NeType("mn", "[mn] Periodiká"),
        new NeType("ms", "[ms] Rádio a TV stanice")
    ];

    public static readonly NeType NumberExpressionsType = new NeType("N", "[N] Číselné Vyjadrenia");
    public static readonly List<NeType> NumberExpressions =
    [
        new NeType("na", "[na] Vek"),
        new NeType("nb", "[nb] Kapitoly, strany, verše"),
        new NeType("nc", "[nc] Základné číslovky"),
        new NeType("ni", "[ni] Číslovaný zoznam"),
        new NeType("nm", "[nm] Násobné číslovky"),
        new NeType("no", "[no] Radové číslovky"),
        new NeType("ns", "[ns] Športové skóre"),
        new NeType("n_", "[n_] Unspecified")
    ];

    public static readonly NeType ArtifactType = new NeType("O", "[O] Názvy Artefaktov");
    public static readonly List<NeType> Artifacts =
    [
        new NeType("oa", "[oa] Knihy, filmy, artefakty"),
        new NeType("oe", "[oe] Meracie jednotky"),
        new NeType("oi", "[oi] Miestnosti"),
        new NeType("om", "[om] Meny"),
        new NeType("op", "[op] Produkty"),
        new NeType("or", "[or] Zákony, normy"),
        new NeType("os", "[os] Stavby, monumenty"),
        new NeType("o_", "[o_] Unspecified")
    ];
    
    public static readonly NeType PersonsType = new NeType("P", "[P] Mená osôb");
    public static readonly List<NeType> Persons =
    [
        new NeType("p1", "[p1] Antické jednoslovné mená"),
        new NeType("pc", "[pc] Obyvateľské mená, kmene"),
        new NeType("pd", "[pd] Tituly a hodnosti"),
        new NeType("pf", "[pf] Krstné mená"),
        new NeType("ph", "[ph] Rody a rodiny"),
        new NeType("pn", "[pn] Prezývky, umelecké mená"),
        new NeType("pm", "[pm] Stredné mená"),
        new NeType("pp", "[pp] Mýtické, literárne mená"),
        new NeType("ps", "[ps] Priezviská"),
        new NeType("p_", "[p_] Unspecified")
    ];
    
    public static readonly NeType TimeType = new NeType("T", "[T] Časové Vyjadrenia");
    public static readonly List<NeType> Time =
    [
        new NeType("te", "[te] Historické udalosti"),
        new NeType("td", "[td] Dni"),
        new NeType("tf", "[tf] Sviatky"),
        new NeType("th", "[th] Časy"),
        new NeType("tm", "[tm] Mesiace"),
        new NeType("tp", "[tp] Obdobia"),
        new NeType("ty", "[ty] Roky"),
        new NeType("t_", "[t_] Skratky")
    ];

    public static readonly NeType NoneType = new NeType(null, "Odstráň typ");
}