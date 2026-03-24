using System.Collections.Generic;

public class Wordbank7 : AdaptiveWordbankAI
{
    private static readonly string[] words =
    {
        "มหิทธานุภาพ", "พิชัยสงคราม", "จักรวาลวิทยา", "อนัตตลักษณะ", "สัมโพธิญาณ", "อุปสมบทกรรม", "วิสุทธิญาณ", "มหาสติปัฏฐาน",
        "ธรรมจักรวัตติ", "วิปัสสนาภูมิ", "ปฏิจจสมุปบาท", "อนุสาสนีปาฐะ", "มหากรุณาธาร", "ปรมัตถสัจจะ", "วิมุตติญาณทัสสนะ", "มหาสงคราม",
        "ธรรมาธิปไตย", "มหาปุริสลักษณะ", "สัพเพสัตตา", "สัมปชัญญะ"
    };

    protected override IReadOnlyList<string> OriginalWords => words;

    protected override string DefaultTieredListResourcesPath => "Wordbanks/Ramayana_TieredList_7_10";
}
