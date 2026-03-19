using System.Collections.Generic;

public class Wordbank5 : AdaptiveWordbankAI
{
    private static readonly string[] words =
    {
        "สหัสเดชะ", "มหิทธานุภาพ", "ธรรมานุภาพ", "ปรมัตถธรรม", "อนัตตลักษณะ", "สังขารธรรม", "อิทธิบาท", "พุทธานุภาพ",
        "ชัยมงคล", "สรรพสิ่ง", "สมเด็จ", "พระบารมี", "วิสุทธิธรรม", "อนุเคราะห์", "อนุสรณ์", "มหากรุณา",
        "อนุโมทนา", "มโนธรรม", "อุปสรรค", "มหาปรารถนา"
    };

    protected override IReadOnlyList<string> OriginalWords => words;
}
