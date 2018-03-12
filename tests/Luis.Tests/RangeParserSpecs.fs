namespace Luis.Tests.DateTimeParser.Helpers
open Xunit
open Shouldly
open Newtonsoft.Json
open Luis.Parser.DateTimeV2
open System

type RangeParserSpecs() =
    let toCheck = "{\"timex\":\"(2018-02-24,2018-03-04,P8D)\",\"type\":\"daterange\",\"start\":\"2018-02-24\",\"end\":\"2018-03-04\"}"
    let res = JsonConvert.DeserializeObject<DateRange>(toCheck)
    let getRange t = 
        match t with
        | TimexType.Range d -> d
        | _ -> raise(Exception("should be a DateRange"))
    [<Fact>]
    member this.``parsed dateRange should have daterange as a type``() =
        res.Type.ShouldBe(TimeType.DateRange)

    [<Fact>]
    member this.``parsed dateRange should have 24 2 2018 as a start date``() =
        res.Start.ShouldBe(DateTime(2018, 2, 24))

    [<Fact>]
    member this.``parsed dateRange should have 4 3 2018 as an end date``() =
        res.End.ShouldBe(DateTime(2018, 2, 24))

    [<Fact>]
    member this.``parsed dateRange should have month as a type of timex period``() =
        let r = getRange res.Timex
        let _, _, p = Range.unwrap r
        p.Type.ShouldBe(PeriodType.Day)

    [<Fact>]
    member this.``parsed dateRange should have 8 as an interval of timex period``() =
        let r = getRange res.Timex
        let _, _, p = Range.unwrap r
        p.Interval.ShouldBe(8)

    [<Fact>]
    member this.``parsed dateRange should have 24 2 2018 as a start date of timex period``() =
        let r = getRange res.Timex
        let s, _, _ = Range.unwrap r
        s.ShouldBe(DateTime(2018, 2, 24))

    [<Fact>]
    member this.``parsed dateRange should have 4 3 2018 as an end date of timex period``() =
        let r = getRange res.Timex
        let _, e, _ = Range.unwrap r
        e.ShouldBe(DateTime(2018, 3, 4))