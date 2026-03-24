using System.Collections.Generic;

public class Wordbank2 : AdaptiveWordbankAI
{
    private static readonly string[] words =
    {
        "ทศกัณฐ์", "อินทรชิต", "สุครีพ", "พาลี", "พิเภก", "มณโฑ", "สีดา", "เบญจกาย",
        "มเหสี", "ราชา", "วานร", "สังฆะ", "ขุนพล", "กุมภกรรณ", "ลักษมณ์", "ราชนคร",
        "อโยธยา", "ขีดขิน", "ปราบ", "อสูร"
    };

    protected override IReadOnlyList<string> OriginalWords => words;

    protected override string DefaultTieredListResourcesPath => "Wordbanks/Ramayana_TieredList_1_3";
}
