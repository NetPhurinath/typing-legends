using System.Collections.Generic;

public class Wordbank : AdaptiveWordbankAI
{
    private static readonly string[] words =
    {
        "หนุมาน", "หนอง", "หนึ่ง", "หยาด", "แห่ง", "รักส์", "พิษ", "พรหม",
        "พราน", "กล้า", "ครั้น", "กรัม", "พรึง", "กษัตริย์", "ศักดิ์", "หงส์",
        "หนาว", "หมาย", "หมอก", "หยิ่ง"
    };

    protected override IReadOnlyList<string> OriginalWords => words;

    protected override bool AutoLoadDefaultTieredListFromResources => false;
}
