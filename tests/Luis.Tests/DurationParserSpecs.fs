namespace Luis.Tests.DateTimeParser.Helpers
open Xunit
open Shouldly
open Newtonsoft.Json
open Luis.Parser.DateTimeV2

type DurationParserSpecs() =
    let toCheck = "{\"timex\":\"P5M\",\"type\":\"duration\",\"value\":\"123\"}"
    let res = JsonConvert.DeserializeObject<Duration>(toCheck)

    [<Fact>]
    member this.``parsed duration should have duration as a type``() =
        res.Type.ShouldBe(TimeType.Duration)

    [<Fact>]
    member this.``parsed duration should have 123 as a value``() =
        res.Value.ShouldBe(123)

    [<Fact>]
    member this.``parsed duration should have month as a type of timex``() =
        res.Timex.Type.ShouldBe(PeriodType.Month)

    [<Fact>]
    member this.``parsed duration should have 5 as a interval in timex``() =
        res.Timex.Interval.ShouldBe(5)

