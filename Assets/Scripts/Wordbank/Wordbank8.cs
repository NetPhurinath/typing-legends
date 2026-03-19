using System.Collections.Generic;

public class Wordbank8 : AdaptiveWordbankAI
{
    private static readonly string[] words =
    {
        "อนัตตสัญญา", "วิสุทธิจิตตัง", "ปรมัตถธรรม", "มหาบุรุษลักษณะ", "อนุโมทนากถา", "ปฏิจจสมุปปันนธรรม", "มหาสติปัฏฐานสูตร", "วิปัสสนาญาณทัสสนะ",
        "สัมโพธิญาณ", "มหาภิเนษกรมณ์", "ปัญญาสัมโพธิ", "มหาปรินิพพาน", "ธรรมวิจยสัมโพชฌงค์", "อริยมัคคญาณ", "สัมโพธิปักขธรรม", "พุทธกิจจกรรม",
        "วิริยสัมโพธิ", "มโนวิญญาณธาตุ", "มหาปุริสวิทยา", "ธัมมานุปัสสนา"
    };

    protected override IReadOnlyList<string> OriginalWords => words;
}
