using System.Collections.Generic;

public class Wordbank4 : AdaptiveWordbankAI
{
    private static readonly string[] words =
    {
        "จักรวาล", "อิทธิฤทธิ์", "ศิริชัย", "สัมฤทธิ์", "พิชัยสงคราม", "ทศพิธ", "สงเคราะห์", "พราหมณ์",
        "วิมาน", "ปรีชา", "วิสุทธิ์", "กฤษณะ", "ธรรมจักร", "มโนรมย์", "ทวยเทพ", "วิรุฬห์",
        "พิพากษ์", "มหาเทพ", "ปรมัตถ์", "พิราลัย"
    };

    protected override IReadOnlyList<string> OriginalWords => words;
}
