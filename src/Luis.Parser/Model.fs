namespace Luis.Parser.DateTimeV2

open System
open Newtonsoft.Json
open Microsoft.FSharp.Reflection

exception ParseError of string
type YearValue = YearValue of int
type PeriodType = Year | Month | Week | Day
type TimeType = Duration | DateRange

type Period = 
    {
        [<JsonProperty("interval")>]
        Interval: int
        [<JsonProperty("type")>]
        Type: PeriodType
    }
type Range = RangeValue of startDate:DateTime * endDate:DateTime * period:Period

module Range =
    let unwrap (RangeValue(s, e, p)) = s, e, p

type private SerializedRange = DateTime * DateTime * string
type TimexType = 
    | Year of YearValue
    | Period of Period
    | Range of Range

module Period =
    let parsePeriod (toParse: string) =
        let penulimate = toParse.Length - 2
        let interval = toParse.Substring(1, penulimate) |> int
        let t = toParse |> Seq.last |> fun x -> 
                match x with
                | 'D' -> PeriodType.Day
                | 'W' -> PeriodType.Week
                | 'M' -> PeriodType.Month
                | 'Y' -> PeriodType.Year
                | _ -> raise (ParseError(sprintf "%c period type not found" x))
        {Interval = interval; Type = t}

type PeriodConverter() =
    inherit JsonConverter()

    override this.WriteJson(writer: JsonWriter, value: obj, serializer: JsonSerializer) =
        writer.WriteValue(sprintf "%A" value)

    override this.ReadJson(reader: JsonReader, objectType: Type, existingValue: obj, serializer: JsonSerializer): obj =
        let timex = downcast reader.Value: string
        Period.parsePeriod timex :> obj

    override this.CanConvert(objType: Type): bool = 
        typedefof<Period> = objType

type TimeTypeConverter() =
    inherit JsonConverter()

    override this.WriteJson(writer: JsonWriter, value: obj, serializer: JsonSerializer) =
        writer.WriteValue(sprintf "%A" value)

    override this.ReadJson(reader: JsonReader, objectType: Type, existingValue: obj, serializer: JsonSerializer): obj =
        (downcast reader.Value: string)
        |> fun x ->
            match x with
            | "duration" -> TimeType.Duration
            | "daterange" -> TimeType.DateRange
            | _ -> raise (ParseError(sprintf "%s time type not found" x))
        :> obj

    override this.CanConvert(objType: Type): bool =
        typedefof<TimeType> = objType

type TimexTypeConverter() =
    inherit JsonConverter()

    let parseAsExpected(toCheck: obj, serializer: JsonSerializer, reader: JsonReader): TimexType  =
        let check = downcast toCheck: string
        let s = ref 0
        if Int32.TryParse(check, s) then Year(YearValue s.Value)
        else
            try
                Period.parsePeriod check |> Period
            with
            | _ -> 
                let values = serializer.Deserialize(reader, typeof<obj[]>) :?> obj[]
                let st, en, pe = downcast FSharpValue.MakeTuple(values, typedefof<Range>): SerializedRange
                let p = Period.parsePeriod pe
                Range(RangeValue(st, en, p))

    override this.WriteJson(writer: JsonWriter, value: obj, serializer: JsonSerializer) =
        writer.WriteValue(sprintf "%A" value)

    override this.ReadJson(reader: JsonReader, objectType: Type, existingValue: obj, serializer: JsonSerializer): obj =
        let toCheck = downcast reader.Value: string
        parseAsExpected(toCheck, serializer, reader) :> obj

    override this.CanConvert(objType: Type): bool =
        typedefof<TimexType> = objType


type Duration = 
    {
        [<JsonProperty("timex")>]
        [<JsonConverter(typeof<PeriodConverter>)>]
        Timex: Period
        [<JsonProperty("type")>]
        [<JsonConverter(typeof<TimeTypeConverter>)>]
        Type: TimeType
        [<JsonProperty("value")>]
        Value: int
    }

type DateRange = 
    {
        [<JsonProperty("timex")>]
        [<JsonConverter(typeof<TimexTypeConverter>)>]
        Timex: TimexType
        [<JsonProperty("type")>]
        [<JsonConverter(typeof<TimeTypeConverter>)>]
        Type: TimeType
        [<JsonProperty("start")>]
        Start: DateTime
        [<JsonProperty("end")>]
        End: DateTime
    }
