using System.Collections.Generic;

public class Wordbank1 : AdaptiveWordbankAI
{
    private static readonly string[] words =
    {
        "วานร","อสุรา","มัจฉา","ปักษา","คีรี","นภา","ราตรี","กระบี่",
        "กุมภ์","นาคา","นารายณ์","ลักษณ์","สีดา","บรรพต","วายุ",
        "หาว","พนา","ชลธี","จันทร","อาทิตย์"
    };

    protected override IReadOnlyList<string> OriginalWords => words;

    protected override string DefaultTieredListResourcesPath => "Wordbanks/Ramayana_TieredList_1_3";
}

