using ConsoleApp2;
using EnumsAreNowGreat;

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