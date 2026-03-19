using System.Collections.Generic;

public class Wordbank6 : AdaptiveWordbankAI
{
    private static readonly string[] words =
    {
        "อนิจจัง", "ทุกขัง", "อนัตตา", "สังสารวัฏ", "วิญญาณ", "ปัญญา", "สมาธิ", "วิปัสสนา",
        "ปราณี", "พุทธะ", "มโนปุพพังคมา", "เจโตวิมุตติ", "วิมุตติญาณ", "กัมมวิบาก", "อนุสัย", "ปฏิจจสมุปบาท",
        "สัมมาทิฏฐิ", "อริยสัจ", "ปรินิพพาน", "ปฏิปทา"
    };

    protected override IReadOnlyList<string> OriginalWords => words;
}
