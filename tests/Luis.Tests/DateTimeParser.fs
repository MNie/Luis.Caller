module Luis.Tests.DateTimeParser

open Xunit
open Shouldly
open System
open FsCheck

type PeriodMock =
    {
        howMany: int
        typeOf: string
        startP: DateTime
        endP: DateTime
    }

type DurationMock =
    {
        howMany: int
        typeOf: string
    }

type DateTimeParserSpecs =
    [<Fact>]
    member this.``should properly parse period``(PeriodMock mock)
        true = false

    [<Fact>]
    member this.``should properly parse duration``(DurationMock mock)
        true = false

