using ConsoleApp2;
using EnumsAreNowGreat;
using RecordsAreNowGreat;

Console.WriteLine(
    States.Off.Switch(
        on: () => "on",
        off: () => "off"
    )
);

Console.WriteLine(
    Languages.HTML.Switch(
        html: () => "aytch-tee-em-ell",
        javaScript: () => "js",
        csharp: () => "c-trash-tag"
    )
);

Console.WriteLine(
    new Result.OkResult("everything is ok").Switch(
        notFoundResult: nfr => "not found",
        okResult: okr => okr.Message
    )
);