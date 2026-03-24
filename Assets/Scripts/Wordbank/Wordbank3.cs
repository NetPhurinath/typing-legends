using System.Collections.Generic;

public class Wordbank3 : AdaptiveWordbankAI
{
    private static readonly string[] words =
    {
        "นารายณ์", "อิศวร", "พรหมา", "มนตรา", "อาคม", "พิธี", "สังเวย", "อัญเชิญ",
        "มงกุฎ", "กำเนิด", "บุญญา", "วิทยา", "ศักดิ์สิทธิ์", "อำนาจ", "มารยา", "วิชา",
        "เมตตา", "กตัญญู", "สัจจะ", "ธรรมะ"
    };

    protected override IReadOnlyList<string> OriginalWords => words;

    protected override string DefaultTieredListResourcesPath => "Wordbanks/Ramayana_TieredList_4_6";
}
